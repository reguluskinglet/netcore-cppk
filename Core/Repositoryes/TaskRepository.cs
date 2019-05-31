using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Newtonsoft.Json;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using Rzdppk.Core.Services;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;
using static Rzdppk.Core.Other.Other;
using TaskStatus = Rzdppk.Model.Enums.TaskStatus;

namespace Rzdppk.Core.Repositoryes
{
    public class TaskRepository 
    {
        private readonly TrainTaskSql _sql;
        private readonly ILogger _logger;
        private readonly string _tableName;

        public TaskRepository(ILogger logger)
        {
            _logger = logger;
            _sql = new TrainTaskSql();
            _tableName = "TrainTasks";
        }

        

        public async Task<List<TrainTaskWithAttribUpdateDate>> CriticalTasksByTrainIdAndDate(int trainId, DateTime currentDate)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var tasks = await conn.QueryAsync<TrainTaskWithAttribUpdateDate>(_sql.CriticalTasksByTrainIdAndDate(trainId, currentDate.Date));
                return tasks.ToList();
            }

        }

        public class TrainTaskWithAttribUpdateDate : TrainTask
        {
            public DateTime AttribUpdateDate { get; set;}
        }


        /// <summary>
        /// Получить все таски по id поезда
        /// </summary>
        public async Task<List<TrainTask>> ByTrainId(int trainId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var tasks = await conn.QueryAsync<TrainTask>(_sql.ByTrainId(trainId));
                return tasks.ToList();
            }

        }


        public async Task<TrainTask> AddTrainTaskWithoutAttributes(TrainTaskaddUi input, User user)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new TaskSqls();
                var taskId = await conn.QueryFirstOrDefaultAsync<int>(sql.AddTaskWithoutAttributes,
                    new
                    {
                        carriageId = input.CarriageId,
                        description = input.Text,
                        equipmentModelId = input.EquipmentId,
                        taskType = input.TaskType,
                        userId = user.Id
                    });
                var result = await GetTrainTaskWithoutAttributes(taskId);
                return result;
            }
        }


        public async Task<TrainTask> GetTrainTaskWithoutAttributes(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new TaskSqls();
                var task = await conn.QueryFirstOrDefaultAsync<TrainTask>(sql.GetTaskWithoutAttributes,
                    new { id });
                return task;
            }
        }

        public async Task<TrainTaskAttribute> AddAttributesToTrainTask(TrainTask trainTask, TaskRepository.TrainTaskaddUi input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new TaskSqls();
                var attributeId = await conn.QueryFirstOrDefaultAsync<int>(sql.AddAtributeToTask,
                    new
                    {
                        faultId = input.FaultId,
                        trainTaskId = trainTask.Id,
                        userId = trainTask.UserId,
                        taskLevel = input.TaskLevel
                    });
                var result = await GetTrainTaskAttribute(attributeId);
                return result;
            }
        }

        public async Task<TrainTaskAttribute> GetTrainTaskAttribute(int attributeId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new TaskSqls();
                var attribute = await conn.QueryFirstOrDefaultAsync<TrainTaskAttribute>(sql.GetTrainTaskAttributeById,
                    new { attributeId });
                return attribute;
            }
        }


        public async Task<TrainTask> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryFirstOrDefaultAsync<TrainTask>(_sql.ById(id));
                return result;
            }
        }

        /// <summary>
        /// получить атрибуты по id задачи
        /// </summary>
        public async Task<List<TrainTaskAttribute>> GetTrainTaskAttributes(int trainTaskId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new TaskSqls();
                var attributes = await conn.QueryAsync<TrainTaskAttribute>(sql.GetTrainTaskAttributes,
                    new { trainTaskId });
                return attributes.ToList();
            }
        }

        /// <summary>
        /// Получить все таски
        /// </summary>
        public async Task<List<TrainTask>> GetAllTrainTask()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new TaskSqls();
                var tasks = await conn.QueryAsync<TrainTask>(sql.GetAllTrainTask);
                return tasks.ToList();
            }
        }



        //private double timeshit = 0.00006;


        public async Task<Dictionary<string, List<TrainTaskPdf>>> GetTrainTasksForPdf(int[] task_ids)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = (await conn.QueryAsync(TaskCommon.SqlByTrainPdf,
                    new[]
                    {
                        typeof(TrainTaskWithRfidLabel), typeof(Carriage), typeof(Train), typeof(EquipmentModel),
                        typeof(EquipmentModel), typeof(Equipment), typeof(Equipment), typeof(Fault), typeof(User), typeof(Label)
                    },
                    (objects) =>
                    {
                        var task = (TrainTaskWithRfidLabel)objects[0];
                        task.Carriage = (Carriage)objects[1];
                        task.Carriage.Train = (Train)objects[2];
                        if (objects[3] != null)
                        {
                            task.EquipmentModel = (EquipmentModel)objects[3];
                            task.EquipmentModel.Equipment = (Equipment)objects[5];
                        }

                        if (objects[4] != null)
                        {
                            task.EquipmentModel.Parent = (EquipmentModel)objects[4];
                            task.EquipmentModel.Parent.Equipment = (Equipment)objects[6];
                        }

                        task.Fault = (Fault)objects[7];

                        if (objects[8] != null)
                        {
                            task.User = (User)objects[8];
                        }

                        if (objects[9] != null)
                        {
                            task.Label = (Label)objects[9];
                        }                        

                        return task;
                    }, new {taskIds = task_ids}
                )).ToList();

                var ret = new Dictionary<string, List<TrainTaskPdf>>();
                if (result.Any())
                {
                    var list = result.Select(task => new TrainTaskPdf
                        {
                            Carriage = task.Carriage.Number.ToString(),
                            CarriageSerial = task.Carriage.Serial.ToString(),
                            Description = task.Description,
                            Equipment = task.EquipmentModel.Equipment.Name,
                            Location = task.EquipmentModel.Parent?.Equipment.Name,
                            Fault = task.Fault?.Name,
                            Label = task.Label?.Rfid,
                            Created = task.CreateDate,
                            Id = task.Id,
                            TrainName = task.Carriage.Train.Name,
                            UserName = task.User?.Name
                        }).ToList();

                    var trains = list.Select(e => e.TrainName).Distinct();

                    foreach (var train in trains)
                    {
                        ret.Add(train, list.Where(e => e.TrainName == train).ToList());
                    }
                    //var ret1 = new TasksByTrain
                    //{
                    //    Train = train_name,
                    //    Tasks = list.ToArray()
                    //};
                }

                return ret;
            }
        }

        public async Task<TrainTaskPdf> GetTrainTaskForPdf(int task_id)
        {
            return (await GetTrainTasksForPdf(new int[] { task_id })).Select(t => t.Value.First()).SingleOrDefault();
        }
        

        /*
         * ид статуса
         * ид типа или название типа, если он не енум
         * имя поезда
         * номер вагона
         * имя модели вагона
         * имя оборудования
         * типовая неисправность
         * фио инициатора
         * дата в формате ГГГГ-ММ-ДД
         */

        public async Task<TasksByInspection[]> GetInspectionTasksForPdf(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var mr = new MeterageRepository(_logger);

                var result = await conn.QueryAsync(TaskCommon.SqlPdf,
                    new[]
                    {
                        typeof(TrainTaskWithRfidLabel), typeof(Carriage), typeof(Train), typeof(EquipmentModel),
                        typeof(EquipmentModel), typeof(Equipment), typeof(Equipment), typeof(Fault), typeof(Inspection),
                        typeof(Train), typeof(User), typeof(Label)
                    },
                    (objects) =>
                    {
                        var task = (TrainTaskWithRfidLabel) objects[0];
                        task.Carriage = (Carriage) objects[1];
                        task.Carriage.Train = (Train) objects[2];
                        if (objects[3] != null)
                        {
                            task.EquipmentModel = (EquipmentModel) objects[3];
                            task.EquipmentModel.Equipment = (Equipment) objects[5];
                        }

                        if (objects[4] != null)
                        {
                            task.EquipmentModel.Parent = (EquipmentModel) objects[4];
                            task.EquipmentModel.Parent.Equipment = (Equipment) objects[6];
                        }

                        task.Fault = (Fault) objects[7];
                        task.Inspection = (Inspection) objects[8];
                        var user = (User) objects[10];
                        user.PasswordHash = null;
                        task.Inspection.User = user;
                        task.Inspection.Train = (Train) objects[9];
                        if (objects[11] != null)
                        {
                            task.Label = (Label) objects[11];
                        }

                        return task;
                    }, new {inspection_id = id}
                );

                var dicTasks = new Dictionary<int, List<TrainTaskPdf>>();
                var dicInspections = new Dictionary<int, Inspection>();

                foreach (var task in result)
                {
                    if (task.Inspection == null) continue;

                    if (!dicInspections.ContainsKey(task.Inspection.Id))
                    {
                        dicInspections.Add(task.Inspection.Id, task.Inspection);
                    }

                    var list = new List<TrainTaskPdf>();
                    if (dicTasks.ContainsKey(task.Inspection.Id))
                    {
                        list = dicTasks[task.Inspection.Id];
                    }
                    list.Add(new TrainTaskPdf
                    {
                        Carriage = task.Carriage.Number.ToString(),
                        Description = task.Description,
                        Equipment = task.EquipmentModel.Equipment.Name,
                        Location = task.EquipmentModel.Parent?.Equipment.Name,
                        Fault = task.Fault?.Name,
                        Label = task.Label?.Rfid,
                        Created = task.CreateDate,
                        Id = task.Id,
                        CarriageSerialNum = task.Carriage.Serial
                    });
                    dicTasks[task.Inspection.Id] = list;
                }

                var ret = new List<TasksByInspection>();
                foreach (var kv in dicInspections)
                {
                    var inspection = kv.Value;
                    var tasks = dicTasks[kv.Key];
                    //
                    var labels = await mr.GetLabels(inspection.Id);
                    var labelslist = new List<JournalService.JournalInspectionLabel>();
                    foreach (var labelFromSql in labels)
                    {
                        labelslist.Add(new JournalService.JournalInspectionLabel
                        {
                            CarriageName = CreateCarriageName(labelFromSql.Label.Carriage.Train.Name,
                               labelFromSql.Label.Carriage.Number),
                            EquipmentName = labelFromSql.Label.EquipmentModel.Equipment.Name,
                            LabelSerial = labelFromSql.Label.Rfid,
                            TimeStamp = labelFromSql.Date
                        });
                    }

                    //
                    var meterages = await mr.GetMeterages(inspection.Id);
                    var temps = new List<JournalService.JournalInspectionTemp>();
                    foreach (var tempFromSql in meterages)
                    {
                        temps.Add(new JournalService.JournalInspectionTemp
                        {
                            Value = tempFromSql.Value,
                            TimeStamp = tempFromSql.Date
                        });
                    }

                    //
                    ret.Add(new TasksByInspection
                    {
                        Type = inspection.CheckListType.ToString(),
                        DateStart = inspection.DateStart,
                        DateEnd = inspection.DateEnd,
                        Train = inspection.Train.Name,
                        User = inspection.User.Name,
                        Tasks = tasks.ToArray(),
                        Labels = labelslist.ToArray(),
                        Temperatures = temps.ToArray()
                    });
                }

                return ret.ToArray();
            }
        }

        //for pdf
        public class TrainTaskWithRfidLabel : TrainTask
        {
            public Label Label { get; set; }
            public Fault Fault { get; set; }
            public Inspection Inspection { get; set; }
        }

        public class TasksByInspection
        {
            public string Type { get; set; }
            public string Train { get; set; }
            public DateTime DateStart { get; set; }
            public DateTime? DateEnd { get; set; }
            public string User { get; set; }
            public TrainTaskPdf[] Tasks { get; set; }
            public JournalService.JournalInspectionLabel[] Labels { get; set; }
            public JournalService.JournalInspectionTemp[] Temperatures { get; set; }

        }

        public class TasksByTrain
        {
            public string Train { get; set; }
            public TrainTaskPdf[] Tasks { get; set; }
        }

        public class TrainTaskPdf
        {
            public string Carriage { get; set; }
            public string CarriageSerial { get; set; }
            public string Location { get; set; }
            public string Equipment { get; set; }
            public string Fault { get; set; }
            public string Description { get; set; }
            //
            public string Label { get; set; }
            public DateTime Created { get; set; }
            public int Id { get; set; }
            public string CarriageSerialNum { get; set; }
            public string TrainName { get; set; }
            public string UserName { get; set; }
        }

        public async Task<List<TrainTaskWithStatus>> GetAllForPdf(int skip, int limit, int traindId)
        {
            //CreateFilter(filter, out string sqlfilter);
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sqlRinpect = new InspectionRepository(_logger);
                var sqlRTrain = new TrainRepository(_logger);
                var inspections = sqlRinpect.GetAll().Result
                    .Where(e => e.TrainId == traindId && e.CheckListType == CheckListType.Inspection).ToList();

                var sqlRUsers = new UserRepository(_logger);
                var sqlRBrigade = new BrigadeRepository(_logger);
                var good = new List<Inspection>();

                foreach (var i in inspections)
                {
                    var user = await sqlRUsers.ById(i.UserId);
                    if (user.BrigadeId != null)
                    {
                        var brig = await sqlRBrigade.ById(user.BrigadeId.Value);
                        if (brig.BrigadeType == BrigadeType.Receiver)
                            good.Add(i);
                    }

                }

                var train = await sqlRTrain.ById(traindId);
                if (!good.Any())
                    throw new ValidationException($"Для состава {train.Name} не найдено инспекций");


                var inspection = inspections.OrderBy(e => e.DateStart).LastOrDefault();



                var sql = $"{TaskCommon.SqlSelect} {TaskCommon.SqlCommon}  and c.TrainId = {train.Id} {TaskCommon.SqlPagingEndSortDate}";

                var result = await conn.QueryAsync<TrainTaskWithStatus>(sql,
                    new[]
                    {
                        typeof(TrainTaskWithStatus), typeof(Carriage), typeof(Train), typeof(EquipmentModel),
                        typeof(Equipment), typeof(Model.Model), typeof(TrainTaskAttribute), typeof(Fault), typeof(Inspection), typeof(User),
                        typeof(TrainTaskStatus), typeof(TrainTaskExecutor)
                    },
                    (objects) =>
                    {
                        var task = (TrainTaskWithStatus) objects[0];
                        task.Carriage = (Carriage) objects[1];
                        task.Carriage.Train = (Train) objects[2];
                        task.EquipmentModel = (EquipmentModel) objects[3];
                        task.EquipmentModel.Equipment = (Equipment) objects[4];
                        task.EquipmentModel.Model = (Model.Model) objects[5];
                        //task.Fault = (Fault) objects[6];
                        //task.Inspection = (Inspection)objects[7];
                        //var inspection = 
                        var user = (User) objects[9];
                        user.PasswordHash = null;
                        task.User = user;
                        task.Status = (TrainTaskStatus) objects[10];
                        task.Executor = (TrainTaskExecutor) objects[11];
                        return task;
                    }, new {skip = skip, limit = limit}
                );

                //Убираем нахуй задачи без инспекций
                //result = result.Where(e => e.InspectionId != null);
                //result.Where(e => e.tr)
                //result = result.Where(e => e.Carriage.Train.Id == traindId).ToList();
                var sqlRts = new TaskStatusRepository(_logger);
                foreach (var task in result)
                {
                    var statuses = await sqlRts.ByTaskId(task.Id);

                    //надо получить последний статус до старта инспекции
                    var status = statuses.Where(e => e.Date < inspection.DateStart).OrderBy(e => e.Date)
                        .LastOrDefault();
                    //если нет подходящего статуса в нужный временной период, то ставим текущий и в рот его ебать
                    if (status == null)
                        task.StatusBeforeInspectionStart = task.Status;
                    else
                        task.StatusBeforeInspectionStart = status;
                }

                //отдаем все задачи у которые на начало инспекции были незакрыты
                return result.Where(e => e.StatusBeforeInspectionStart.Status != TaskStatus.Closed).ToList();
            }
        }

        public async Task<TrainTaskPaging> GetAll(int skip, int limit, string filter)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                CreateFilter(filter, out string sqlfilter);
                var sql =
                    $"{TaskCommon.SqlSelect} {TaskCommon.SqlCommon} {sqlfilter} {TaskCommon.SqlPagingEndSortDate}";


                var result = (await conn.QueryAsync<TrainTaskWithStatus>(sql,
                    new[]
                    {
                        typeof(TrainTaskWithStatus), typeof(Carriage), typeof(Train), typeof(EquipmentModel),
                        typeof(Equipment), typeof(Model.Model), typeof(Fault), typeof(Inspection), typeof(User),
                        typeof(TrainTaskStatus), typeof(TrainTaskExecutor)
                    },
                    (objects) =>
                    {
                        var task = (TrainTaskWithStatus) objects[0];
                        task.Carriage = (Carriage) objects[1];
                        task.Carriage.Train = (Train) objects[2];
                        task.EquipmentModel = (EquipmentModel) objects[3];
                        task.EquipmentModel.Equipment = (Equipment) objects[4];
                        task.EquipmentModel.Model = (Model.Model) objects[5];
                        //task.Fault = (Fault) objects[6];
                        //task.Inspection = (Inspection)objects[7];
                        //var inspection = 
                        var user = (User) objects[8];
                        user.PasswordHash = null;
                        task.User = user;
                        task.Status = (TrainTaskStatus) objects[9];
                        task.Executor = (TrainTaskExecutor) objects[10];
                        return task;
                    }, new {skip = skip, limit = limit}
                )).ToList();
                var result1 = result.Select(t => ConvertTaskToTaskUi(t)).ToArray();
                //
                var sqlc = $"{TaskCommon.SqlCount} {TaskCommon.SqlCommon} {sqlfilter}";
                var count = (await conn.QueryAsync<int>(sqlc)).FirstOrDefault();
                var output = new TrainTaskPaging
                {
                    Data = result1,
                    Total = count
                };

                return output;
            }
        }

        public async Task<List<TrainTask>> GetAll()
        {

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<TrainTask>(_sql.GetAll());
                return result.ToList();
            }
        }

        public async Task UpdateEditDate(int taskId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.QueryAsync<TrainTask>(_sql.UpdateEditDate(taskId));
            }
        }


        public async Task<List<TrainTask>> GetAllSortByProperty(string property, string direction)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<TrainTask>(CommonSql.GetAllSortByProperty(_tableName, property, direction));
                return result.ToList();
            }
        }



        /*
         * интервал дат, типа с Х и по У, дата в формате ГГГГ-ММ-ДД
         *  ид поезда
         *  ид пользователя инициатора
         *  статус задачи
         *  ид бригады
         *  ид вагона
         *  
         *  Carriage.TrainId, CarriageId
         *  UserId, User.BrigadeId,
         *  CreateDate <>, StatusId
         *  ExecutorId  !!!!! TODO
         */
        private static void CreateFilter(string filter, out string sqlfilter)
        {
            sqlfilter = "";
            if (filter != null)
            {
                var filters = JsonConvert.DeserializeObject<TaskCommon.FilterBody[]>(filter);
                sqlfilter = " ";
                for (var index = 0; index < filters.Length; index++)
                {
                    var item = filters[index];
                    switch (item.Filter)
                    {
                        //внезапно блядь имя поезда
                        case "TrainId":
                            var valueI = item.Value;
                            sqlfilter += $" AND t.Name like '%{valueI}%'";
                            break;

                        case "CarriageId":
                            valueI = item.Value;
                            sqlfilter += $" AND c.Number like '%{valueI}%'";
                            break;

                        case "OwnerId":
                            valueI = item.Value;
                            sqlfilter += $" AND u.Name like '%{valueI}%'";
                            break;

                        case "BrigadeId":
                            valueI = item.Value;
                            sqlfilter += $" AND  tte.BrigadeType like '%{valueI}%'";
                            break;

                        case "StatusId":
                            valueI = item.Value;
                            sqlfilter += $" AND st.Status like '%{valueI}%'";
                            break;

                        case "InspectionId":
                            var valueI1 = Int32.Parse(item.Value);
                            sqlfilter += $" AND i.Id={valueI1}";
                            break;

                        case "DateFrom":
                            sqlfilter += $" AND ts.CreateDate >= '{item.Value}'";
                            break;

                        case "DateTo":
                            DateTime.TryParse(item.Value, out var dateParsed);
                            dateParsed = dateParsed.AddDays(1);
                            CultureInfo us = new CultureInfo("en-US");
                            sqlfilter += $" AND ts.CreateDate < '{dateParsed.ToString(dateParsed.ToString(@"yyyy-MM-dd", us))}'";
                            break;
                        case "Custom":
                            sqlfilter += $" AND ({item.Value})";
                            break;
                    }

                    //if (index < (filters.Length - 1))
                    //    sqlfilter += " AND ";
                }
            }
        }




        public class TrainTaskaddUi
        {
            //public DateTime Data { get; set;}
            //public string UserName { get; set; }
            public int? FaultId { get; set; }
            public TaskType TaskType { get; set; }
            public int TrainId { get; set; }
            public int CarriageId { get; set; }
            public int EquipmentId { get; set; }
            public TaskStatus TaskStatus { get; set; }
            public BrigadeType Executor { get; set; }
            public string Text { get; set; }
            public int[] FilesId { get; set; }
            public int TaskLevel { get; set; }

        }

        public class TrainTaskWithUpdateStatusDto 
        {
            public TrainTask TrainTask { get; set;}
            public bool IsUpdated { get; set; }
            public string EquipmentName { get; set;}
        }

        public TrainTaskUi ConvertTaskToTaskUi(TrainTaskWithStatus task)
        {
            var tUi = new TrainTaskUi
            {
                Id = task.Id,
                CarriageName = CreateCarriageName(task.Carriage.Train.Name, task.Carriage.Number),
                CreateDate = task.CreateDate,//.ToString("yyyy-MM-dd"),
                Executor = task.Executor.BrigadeType,
                EquipmentName = task.EquipmentModel.Equipment.Name,
                //FaultName = task.Fault?.Name,
                ModelName = task.EquipmentModel.Model.Name,
                OwnerName = task.User.Name,
                StatusId = task.Status?.Status,// ?? TaskStatus.New,
                TaskTypeId = task.TaskType,
                TrainName = task.Carriage.Train.Name,
                CarriageNum = task.Carriage.Number

            };
            return tUi;
        }

        public TaskTypeUi[] GetAvailableTaskTypes()
        {
            var res = new List<TaskTypeUi>();

            res.Add(new TaskTypeUi { Id = 0, Name = "Техническая" });
            res.Add(new TaskTypeUi { Id = 1, Name = "Санитарная" });
            res.Add(new TaskTypeUi { Id = 2, Name = "Сервисная" });

            return res.ToArray();
        }

        public static string BrigadeTypeToString(BrigadeType? type)
        {
            var name = "";

            if (type != null)
            {
                switch (type)
                {
                    case BrigadeType.Depo:
                        name = "Бригада депо";
                        break;
                    case BrigadeType.Locomotiv:
                        name = "Локомотивная бригада";
                        break;
                    case BrigadeType.Receiver:
                        name = "Приемщики";
                        break;
                    default:
                        name = "неизвестный тип: " + type;
                        break;
                }
            }

            return name;
        }

        public string StatusToString(TaskStatus? st)
        {
            var name = "";

            if (st != null)
            {
                switch (st)
                {
                    case TaskStatus.InWork:
                        name = "В работе";
                        break;
                    case TaskStatus.Log:
                        name = "В журнале";
                        break;
                    case TaskStatus.Closed:
                        name = "Закрыто";
                        break;
                    case TaskStatus.Remake:
                        name = "Переоткрыт";
                        break;
                    case TaskStatus.Confirmation:
                        name = "К подтверждению";
                        break;
                    case TaskStatus.New:
                        name = "Новый";
                        break;
                    default:
                        name = "неизвестный статус: " + st;
                        break;
                }
            }

            return name;
        }



        public class TaskTypeUi
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class TrainTaskPaging
        {
            public TrainTaskUi[] Data { get; set; }
            public int Total { get; set; }
        }

        public class TrainTaskWithStatus : TrainTask
        {
            public TrainTaskStatus Status { get; set; }
            public TrainTaskExecutor Executor { get; set; }

            public TrainTaskStatus StatusBeforeInspectionStart { get; set; }
        }



        public enum TaskHistoryType
        {
            Status = 1,
            Executor = 2,
            Comment = 3
        }




        /*
       * ид статуса
       * ид типа или название типа, если он не енум
       * имя поезда
       * номер вагона
       * имя модели вагона
       * имя оборудования
       * типовая неисправность
       * фио инициатора
       * дата в формате ГГГГ-ММ-ДД
       */
        public class TrainTaskUi
        {
            public int Id { get; set; }
            public Model.Enums.TaskStatus? StatusId { get; set; }
            public TaskType TaskTypeId { get; set; }
            public string TrainName { get; set; }
            public string CarriageName { get; set; }
            public string ModelName { get; set; }
            public string EquipmentName { get; set; }
            public string FaultName { get; set; }
            public string OwnerName { get; set; }
            public DateTime CreateDate { get; set; }
            public BrigadeType? Executor { get; set; }
            public int CarriageNum { get; set; }
        }







        public class DocumentDetail : Document
        {
            public string Base64 { get; set; }
        }

    }
}
