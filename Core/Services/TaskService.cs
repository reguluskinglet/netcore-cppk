using Dapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Api.Dto.EventTable;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using static Rzdppk.Api.Dto.EventTable.TaskByIdDto;
using static Rzdppk.Core.Other.DevExtremeTableData;
using static Rzdppk.Core.Other.Other;
using static Rzdppk.Core.Repositoryes.TaskRepository;
using TaskStatus = Rzdppk.Model.Enums.TaskStatus;

namespace Rzdppk.Core.Services
{
    public class TaskService
    {
        private readonly ILogger _logger;

        public TaskService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<TrainTaskWithUpdateStatusDto> TrainTaskAdd(TrainTaskaddUi input, User user)
        {
            var sqlRTask = new TaskRepository(_logger);
            var sqlRStatus = new TaskStatusRepository(_logger);
            var sqlREquipmentModel = new EquipmentModelsRepository(_logger);
            var sqlREquipment = new EquipmentRepository(_logger);

            //Проверяем по [CarriageId] и [EquipmentModelId] наличие подобной незакрытой задачи (EquipmentId c UI это EquipmentModelId...)
            var allTasks = await sqlRTask.GetAllTrainTask();
            //TODO перенести логику выборки в sql
            allTasks = allTasks.Where(e => e.CarriageId == input.CarriageId && e.EquipmentModelId == input.EquipmentId)
                .ToList();
            var trainTaskToAdd = await CheckTrainTaskExist(sqlRStatus, allTasks);

            //Начинаем транзакцию
            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                var isUpdated = true;

                if (trainTaskToAdd == null)
                {
                    //Делаем пустую таску
                    trainTaskToAdd = await sqlRTask.AddTrainTaskWithoutAttributes(input, user);
                    isUpdated = false;
                }

                await sqlRTask.UpdateEditDate(trainTaskToAdd.Id);

                //добавляем новый атрибут к существующей или свеже созданной
                var newTaskAtribute = await sqlRTask.AddAttributesToTrainTask(trainTaskToAdd, input);
                trainTaskToAdd.TrainTaskAttributes = new List<TrainTaskAttribute> { newTaskAtribute };

                //прихуярить статус
                var updateTaskData = new UpdateTaskData
                {
                    StatusId = (int)input.TaskStatus,
                    TraintaskId = trainTaskToAdd.Id,
                    FilesId = input.FilesId,
                    CommentText = input.Text,
                    TrainTaskExecutorsId = (int)input.Executor
                };


                await UpdateTask(updateTaskData, user, true);
                var equipmentModel = await sqlREquipmentModel.ById(trainTaskToAdd.EquipmentModelId);
                var equipment = await sqlREquipment.ById(equipmentModel.EquipmentId);

                transaction.Complete();



                var result = new TrainTaskWithUpdateStatusDto
                {
                    TrainTask = trainTaskToAdd,
                    IsUpdated = isUpdated,
                    EquipmentName = equipment.Name
                };

                return result;
            }
        }

        public async Task UpdateTask(UpdateTaskData data, User user, bool newtask = false, bool timeShift = true)
        {
            var sqlRExecutor = new ExecutorRepository(_logger);
            var sqlRTaskStatus = new TaskStatusRepository(_logger);
            var sqlRComment = new CommentRepository(_logger);
            var sqlRDocuments = new DocumentRepository(_logger);
            var sqlRTask = new TaskRepository(_logger);

            int permissions = user.Role.Permissions;

            await sqlRTask.UpdateEditDate(data.TraintaskId);

            //Надо добавить к таске нового исполнителя
            data.TrainTaskExecutorsId = (await sqlRExecutor.AddNewExecutorToTask(data, user, timeShift)).Id;

            //Надо поставить статус указанный изначально, а потом запустить автосмену, в случае это не не админ с новой задачей
            await ChangeStatusAndExecutors(data, user, newtask, permissions);

            if (data.StatusId != null)
                await sqlRTaskStatus.ChangeStatusByTrainTaskId(data.TraintaskId, data.StatusId.Value, user, newtask, timeShift);


            //Добавляем комент
            var commentId = 0;
            data.CommentText = data.CommentText ?? "";
            commentId = await sqlRComment.AddCommentByTrainTaskId(data.TraintaskId, data.CommentText, user);
            

            //Добавляем файлы, если есть комент
            if (data.FilesId != null && data.FilesId.Length > 0 && commentId != 0)
            {
                await sqlRDocuments.AddToCommentId(data.FilesId, commentId);

            }

        }



        private async Task ChangeStatusAndExecutors(UpdateTaskData data, User user, bool newtask, int permissions)
        {
            var needPermissionBits = -1;
            var res = permissions & needPermissionBits;
            //если админ то итак нихуя неизменится. нахера флаг с новой таской. хм
            //if (res != needPermissionBits && !newtask)
            if (res != needPermissionBits)
            {
                //Меняем статус
                if (data.StatusId != null)
                {
                    //меняем исполнителя, если статус подходящий

                    int brigadeType = 999;
                    //тут начинается ебоклюйство с правами.
                    // бригады локомативщиков 6
                    needPermissionBits = 64;
                    res = permissions & needPermissionBits;
                    if (res == needPermissionBits)
                    {
                        if (data.StatusId == (int)TaskStatus.Remake)
                            brigadeType = (int)BrigadeType.Depo;
                    }
                    // приемщиков 8 
                    needPermissionBits = 256;
                    res = permissions & needPermissionBits;
                    if (res == needPermissionBits)
                    {
                        if (data.StatusId == (int)TaskStatus.Confirmation)
                            brigadeType = (int)BrigadeType.Locomotiv;
                        if (data.StatusId == (int)TaskStatus.Remake)
                            brigadeType = (int)BrigadeType.Depo;
                    }

                    if (brigadeType != 999)
                    {

                        var trainTaskExecutor = new TrainTaskExecutor
                        {
                            TrainTaskId = data.TraintaskId,
                            BrigadeType = (BrigadeType)brigadeType,
                            UserId = user.Id
                        };
                        var sqlRExecutor = new ExecutorRepository(_logger);
                        await sqlRExecutor.AddNewExecutorToTask(trainTaskExecutor, user, false);
                    }
                }
            }
        }


        public async Task<TaskByIdDto> GetTaskById(int id, int permissions = -1)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sqlRExecutor = new ExecutorRepository(_logger);
                var sqlRTaskStatus = new TaskStatusRepository(_logger);
                var sqlRTask = new TaskSqls();


                var fromSql = (await conn.QueryAsync<TaskDetailFromSql>(sqlRTask.GetTrainTaskByIdAndAttributeId(id))).ToList();

                var task = fromSql.First();

                var result = new TaskByIdDto
                {
                    Id = task.Id,
                    Data = task.CreateDate,
                    InitiatorName = task.InitiatorName,
                    TrainName = task.TrainName,
                    EquipmentName = task.EquipmentName,
                    CarriageSerial = task.CarriageSerial, //CreateCarriageName(task.TrainName, task.CarriageNumber),
                    TaskType = task.TaskType
                };

                //Текущий статус
                var taskStatuses = await sqlRTaskStatus.ByTaskId(id);
                var currentTaskStatus = taskStatuses.Last().Status;
                result.StatusId = (int)currentTaskStatus;

                //Текущий исполнитель
                var taskExecutors = await sqlRExecutor.GetByTaskId(id);

                BrigadeType currentTaskExecutor = 0;
                if (taskExecutors.Count > 0)
                {
                    currentTaskExecutor = taskExecutors.Last().BrigadeType;
                    result.BrigadeType = (int)currentTaskExecutor;
                }                

                //Уровень задачи
                var taskLevel = fromSql.OrderBy(x => x.TaskLevel).Select(x => x.TaskLevel).First();
                result.TaskLevel = taskLevel.HasValue ? (int)taskLevel : 0;

                //Доступные статусы
                var possibleTaskStatuses = sqlRTaskStatus.GetAvailableStatusesNewTask(currentTaskStatus, currentTaskExecutor, permissions);
                result.PossibleTaskStatuses = possibleTaskStatuses.Select(x => (int)x).ToList();

                //Неисправности из атрибутов
                result.Faults = new List<FaultTaskDto>();
                result.Inspections = new List<InspectionTaskDto>();
                foreach (var item in fromSql)
                {
                    if (item.AttributeFaultId != null)
                    {
                        await AddFaultData(result, item);
                    }

                    if (item.AttributeInspectionId != 0)
                    {
                        await AddInspectionData(result, item);
                    }

                }
                //Добавить в фаулты данные по чеклистам из инпекции
                var withCheckListEquipmentId = fromSql.Where(x => x.AttributeCheckListEquipmentId != null)
                    .DistinctBy(y => y.AttributeCheckListEquipmentId).ToList();
                foreach (var item in withCheckListEquipmentId)
                {
                    await AddFaultDataFromCheckListEquipmentId(result, item);
                }

                result.History = await AddHistoryData(id);

                return result;
            }

        }
        private async Task AddFaultDataFromCheckListEquipmentId(TaskByIdDto result, TaskDetailFromSql item)
        {
            var sqlRUsers = new UserRepository(_logger);
            var attributeUser = await sqlRUsers.ById(item.AttributeUserId);

            var faultToAdd = new FaultTaskDto
            {
                User = attributeUser.Name,
                Date = item.AttributeUpdateDate,
                Id = item.AttributeId,
                Text = item.CheckListEquipmentTaskName
            };
            result.Faults.Add(faultToAdd);
        }

        private async Task AddFaultData(TaskByIdDto result, TaskDetailFromSql item)
        {
            var sqlRUsers = new UserRepository(_logger);
            var attributeUser = await sqlRUsers.ById(item.AttributeUserId);
            var faultToAdd = new FaultTaskDto
            {
                User = attributeUser.Name,
                Date = item.AttributeUpdateDate,
                Id = item.AttributeId,
                Text = item.FaultName
            };
            result.Faults.Add(faultToAdd);
        }

        private async Task AddInspectionData(TaskByIdDto result, TaskDetailFromSql item)
        {
            var sqlRInspection = new InspectionRepository(_logger);
            var sqlRUsers = new UserRepository(_logger);
            var sqlRBrigade = new BrigadeRepository(_logger);

            var inspection = await sqlRInspection.ById(item.AttributeInspectionId);
            var inspectionUser = await sqlRUsers.ById(inspection.UserId);
            int? inspectionBrigadeType = null;
            if (inspectionUser.BrigadeId.HasValue)
            {
                var inspectionBrigade = await sqlRBrigade.ById(inspectionUser.BrigadeId.Value);
                inspectionBrigadeType = (int)inspectionBrigade.BrigadeType;
            }

            var inspectionToAdd = new InspectionTaskDto
            {
                Id = inspection.Id,
                DateStart = inspection.DateStart,
                DateEnd = inspection.DateEnd,
                User = inspectionUser.Name,
                Type = (int)inspection.CheckListType,
                BrigadeType = inspectionBrigadeType,
                Texts = new List<string>()
            };

            if (item.AttributeCheckListEquipmentId.HasValue)
            {
                var toAddText = $"{item.EquipmentName} -> {item.CheckListEquipmentTaskName} -> ";
                var checkListStringValue = string.Empty;
                var taskAttributeStringValue = string.Empty;
                if (item.CheckListEquipmentValueType == 1)
                {
                    if (item.CheckListEquipmentValue == 0)
                        checkListStringValue = "Нет";
                    if (item.CheckListEquipmentValue == 1)
                        checkListStringValue = "Да";

                    if (item.AttributeValue == 0)
                        taskAttributeStringValue = "Нет";
                    if (item.AttributeValue == 1)
                        taskAttributeStringValue = "да";
                }
                else
                {
                    checkListStringValue = item.CheckListEquipmentValue.ToString();
                    taskAttributeStringValue = item.AttributeValue.ToString();
                }

                toAddText += $"{checkListStringValue} -> {taskAttributeStringValue}";

                inspectionToAdd.Texts.Add(toAddText);
            }


            // Для ТО-1 ТО-2 и тд
            if (inspection.CheckListType == CheckListType.TO1 || inspection.CheckListType == CheckListType.TO2)
            {
                //  Уровень критичности → Высокий
                if (item.TaskLevel.HasValue)
                {
                    var toAddText = $"Уровень критичности -> {GetStringTaskLevel(item.TaskLevel.Value)}";
                    inspectionToAdd.Texts.Add(toAddText);
                }

                //Смотрим что добавилось в рамках мероприятия TaskLevel или FaultId, пишим в текст
                if (item.AttributeFaultId.HasValue)
                {
                    var toAddText = $"{item.FaultName} -> {item.EquipmentName}";
                    inspectionToAdd.Texts.Add(toAddText);
                }
            }

            result.Inspections.Add(inspectionToAdd);
        }

        public async Task<List<HistoryTaskDto>> AddHistoryData(int id)
        {
            var list = new List<HistoryTaskDto>();

            var cr = new CommentRepository(_logger);
            var dr = new DocumentRepository(_logger);
            var er = new ExecutorRepository(_logger);
            var sr = new TaskStatusRepository(_logger);

            var comments = await cr.GetByTaskId(id);
            var docs = await dr.GetByTaskId(id);
            var executors = await er.GetByTaskId(id);
            var statuses = await sr.ByTaskId(id);

            foreach (var comment in comments)
            {
                var docsComment = docs.Where(item => item.TrainTaskCommentId == comment.Id).ToList();
                var filesDto = new List<FileTaskDto>();
                foreach (var item in docsComment)
                {
                    var toAdd = new FileTaskDto
                    {
                        DocumentType = (int)item.DocumentType,
                        Id = item.Id,
                        Name = item.Name
                    };
                    filesDto.Add(toAdd);
                }

                if (filesDto.Count != 0 || !string.IsNullOrWhiteSpace(comment.Text))
                list.Add(new HistoryTaskDto
                {
                    Date = comment.Date,
                    Files = filesDto,
                    Type = TaskHistoryType.Comment.ToString(),
                    User = comment.User.Name,
                    UserBrigadeType = (int?)comment.User.Brigade?.BrigadeType,
                    Text = comment.Text
                });
            }

            TrainTaskExecutor prevExec = null;
            foreach (var exec in executors)
            {
                list.Add(new HistoryTaskDto
                {
                    Date = exec.Date,
                    NewExecutorBrigadeType = (int)exec.BrigadeType,
                    OldExecutorBrigadeType = prevExec == null ? null : (int?)prevExec.BrigadeType,
                    Type = TaskHistoryType.Executor.ToString(),
                    User = exec.User.Name,
                    UserBrigadeType = (int?)exec.User.Brigade?.BrigadeType,
                });
                prevExec = exec;
            }

            TrainTaskStatus prevSt = null;
            foreach (var st in statuses)
            {
                list.Add(new HistoryTaskDto
                {
                    Date = st.Date,
                    NewStatus = (int)st.Status,
                    OldStatus = prevSt == null ? null : (int?)prevSt.Status,
                    Type = TaskHistoryType.Status.ToString(),
                    User = st.User.Name,
                    UserBrigadeType = (int?)st.User.Brigade?.BrigadeType,
                });
                prevSt = st;
            }

            var ret = list.OrderBy(o => o.Date).ToList();
            return ret;
        }


        private async static Task<TrainTask> CheckTrainTaskExist(TaskStatusRepository sqlRStatus, List<TrainTask> allTasks)
        {
            TrainTask trainTaskToAdd = null;
            if (allTasks.Count > 0)
                foreach (var value in allTasks)
                {
                    var statuses = await sqlRStatus.ByTaskId(value.Id);
                    if (statuses.Length > 0 && statuses.Last().Status != TaskStatus.Closed)
                        trainTaskToAdd = value;

                }

            return trainTaskToAdd;
        }

        public class TaskDetailFromSql
        {
            public int Id { get; set; }
            public string FaultName { get; set; }
            public DateTime CreateDate { get; set; }
            public string InitiatorName { get; set; }
            public string TrainName { get; set; }
            public int CarriageNumber { get; set; }
            public int FaultTypeId { get; set; }
            public string EquipmentName { get; set; }
            public string Description { get; set; }
            //public int InspectionId { get; set; }
            public int TaskType { get; set; }
            public string CarriageSerial { get; set; }
            public int? CarriageModelTypeId { get; set; }
            public TaskLevel? TaskLevel { get; set; }
            public int AttributeId { get; set; }
            /// <summary>
            /// Неебу что за валуе. Попросили екго брать из таблицы TrainTasks
            /// </summary>
            public int Value { get; set; }

            public DateTime AttributeUpdateDate { get; set; }
            public int AttributeUserId { get; set; }
            public int AttributeInspectionId { get; set; }
            public int? AttributeFaultId { get; set; }
            public int? AttributeCheckListEquipmentId { get; set; }
            public int? AttributeValue { get; set; }
            public string CheckListEquipmentTaskName { get; set; }
            public int CheckListEquipmentValueType { get; set; }
            public int CheckListEquipmentValue { get; set; }
        }

        public class TrainTaskHistoryUi
        {
            public DateTime Date { get; set; }
            public string User { get; set; }
            public int? UserBrigadeType { get; set; }
            public string Type { get; set; }
            public int? OldStatus { get; set; }
            public int? NewStatus { get; set; }
            public int? OldExecutorBrigadeType { get; set; }
            public int? NewExecutorBrigadeType { get; set; }
            public string Text { get; set; }
            public Document[] Files { get; set; }
        }

        public class UpdateTaskData
        {
            public int? StatusId { get; set; }
            public int TraintaskId { get; set; }
            public int AttributeId { get; set; }
            public int[] FilesId { get; set; }
            public int? TrainTaskExecutorsId { get; set; }
            public string CommentText { get; set; }
        }
    }
}