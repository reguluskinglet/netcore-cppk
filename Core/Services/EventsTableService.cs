using AutoMapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Api.Dto.EventTable;
using Rzdppk.Core.Helpers;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static Rzdppk.Api.Dto.EventTable.InspectionByIdDto;
using static Rzdppk.Core.Other.DevExtremeTableData;
using static Rzdppk.Core.Other.Other;


namespace Rzdppk.Core.Services
{
    public class EventsTableService
    {

        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public EventsTableService(ILogger logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<InspectionByIdDto> InspectionByIdForEventTable(int inspectionId)
        {
            var sqlRInspection = new InspectionRepository(_logger);
            var inspectionDto = await sqlRInspection.ByIdForEventTable(inspectionId);
            var sqlRTrainTaskAttributes = new TrainTaskAttributesRepository(_logger);

            if (inspectionDto == null)
                throw new NotFoundException();
            inspectionDto.Id = "M" + inspectionDto.InspectionId;

            inspectionDto.Type = GetStringInspectionType((CheckListType)inspectionDto.TypeInt);
            inspectionDto.Status = GetStringInspectionStatus((InspectionStatus)inspectionDto.StatusInt);
            inspectionDto.BrigadeName = GetStringBrigadeType((BrigadeType)inspectionDto.BrigadeTypeInt);

            var endDateString = string.Empty;
            if (inspectionDto.DateEnd.HasValue)
                endDateString = $" - {inspectionDto.DateEnd:g}";
            inspectionDto.Date = $"{inspectionDto.DateStart:g}" + endDateString;


            //TaskCount
            var taskAttributesByInspectionCount = await sqlRTrainTaskAttributes.ByInspectionId(inspectionId);
            inspectionDto.TaskCount = taskAttributesByInspectionCount.Count;

            //Метки
            inspectionDto.Labels = await GetInspectionLabels(inspectionId);

            //Счетчики киловат и прочей хери
            inspectionDto.InspectionDataCarriages = await GetInspectionCounters(inspectionId);

            //Остальные данные по инспекции
            inspectionDto.InspectionDataUis = await GetInspectionOtherData(inspectionId);

            return inspectionDto;
        }

        private async Task<List<InspectionLabelDto>> GetInspectionLabels(int inspectionId)
        {
            var sqlRMeterage = new MeterageRepository(_logger);
            var labelsFromSql = await sqlRMeterage.GetLabels(inspectionId);
            var labels = labelsFromSql.Select(x =>
                new InspectionLabelDto
                {
                    CarriageName = CreateCarriageName(x.Label.Carriage.Train.Name, x.Label.Carriage.Number),
                    EquipmentName = x.Label.EquipmentModel.Equipment.Name,
                    Label = x.Label.Rfid,
                    Date = x.Date.ToString("g")
                }).ToList();
            return labels;
        }


        private async Task<List<InspectionDataCarriageDto>> GetInspectionCounters(int inspectionId)
        {
            var sqlR = new InspectionDataRepository(_logger);

            var countersFromSql = await sqlR.GetByInspectionId(inspectionId);
            var result = countersFromSql
                .Where(x => x.Type == InspectionDataType.KwHours)
                .Select(x =>
                new InspectionDataCarriageDto
                {
                    CarriageName = "Вагон " + x.Carriage.Serial,
                    Value = x.Value
                }).ToList();
            return result;
        }

        private async Task<List<InspectionDataDto>> GetInspectionOtherData(int inspectionId)
        {
            var sqlR = new InspectionDataRepository(_logger);

            var countersFromSql = await sqlR.GetByInspectionId(inspectionId);
            var result = countersFromSql
                .Where(q => q.Type == InspectionDataType.KmPerShift)
                .Select(x =>
                    new InspectionDataDto
                    {
                        Type = x.Type.GetDescription(),
                        Value = x.Value.ToString()
                    }).ToList();
            result.AddRange(
                    countersFromSql
                        .Where(q => q.Type == InspectionDataType.KmTotal)
                        .Select(x =>
                            new InspectionDataDto
                            {
                                Type = x.Type.GetDescription(),
                                Value = x.Value.ToString()
                            }).ToList()
                );
            result.AddRange(
                countersFromSql
                    .Where(q => q.Type == InspectionDataType.BrakeShoes)
                    .Select(x =>
                        new InspectionDataDto
                        {
                            Type = x.Type.GetDescription(),
                            Value = string.Join(", ", countersFromSql
                                .Where(o => o.Type == InspectionDataType.BrakeShoes)
                                .Select(o => o.Text))
                        }).ToList()
            );

            return result;
        }


        public async Task<ReportResponse> GetTaskAndInspections(ReportRequest input)
        {

            var sqlRTrainTaskAttributes = new TrainTaskAttributesRepository(_logger);

            var result = new ReportResponse { Rows = new List<Row>() };

            if (input.ReportId == null)
                throw new ValidationException("reportId null");

            //Отчет всех тасок 
            if (int.Parse(input.ReportId) == (int)ReportsTableEnum.Tasks)
            {

                //атрибуты на 2-й уровень
                if (input.ParentId?.Type == (int)EventTableEnum.TrainTask)
                {
                    CreateTaskAttributesChildTableColumbs(result);
                    var trainTaskAttributesByTaskId = await sqlRTrainTaskAttributes.ByTaskId(input.ParentId.Id);
                    var taskAttributesData =
                        GetTaskAttributesData(trainTaskAttributesByTaskId, input.ParentId, _logger).Result;
                    result.Rows.AddRange(taskAttributesData);

                    return result;

                }

                //Все таски
                CreateMainTableColumbs(result);

                var taskReportData = await FilterPaginate(input, _logger, isFillInspection: false);
                var reportTasks = new List<TrainTask>();
                foreach (var task in taskReportData.TrainTask)
                {
                    var attributes = await sqlRTrainTaskAttributes.ByTaskId(task.Id);
                    if (attributes.Any())
                        reportTasks.Add(task);
                }

                result.Rows.AddRange(GetTaskData(reportTasks, _logger).Result);

                result.Total = taskReportData.Total.ToString();
                return result;

            }


            switch (input.ParentId?.Type)
            {
                //Судя по всему задачи без инспекций для списка с инспекциями
                case (int)EventTableEnum.Inspection:
                    CreateTaskAttributesChildTableColumbs(result);

                    var taskWihoutInspForInspList = (await FilterPaginate(input, _logger)).TrainTask;
                    var taskWihoutIns = new List<TrainTask>();
                    foreach (var task in taskWihoutInspForInspList)
                    {
                        var attributes = await sqlRTrainTaskAttributes.ByTaskId(task.Id);
                        var attributesWithOutInspections = attributes.Where(x => x.InspectionId == input.ParentId.Id).ToList();
                        if (attributesWithOutInspections.Any())
                            taskWihoutIns.Add(task);
                    }
                    result.Rows.AddRange(GetTaskDataChildInspection(taskWihoutIns, input.ParentId, _logger).Result);
                    return result;

                //Атрибуты для задачи
                case (int)EventTableEnum.TrainTask:
                    CreateTaskAttributesChildTableColumbs(result);
                    var traintTaskAttributesByTaskId = await sqlRTrainTaskAttributes.ByTaskId(input.ParentId.Id);
                    var taskAttributesData =
                        GetTaskAttributesData(traintTaskAttributesByTaskId, input.ParentId, _logger).Result;
                    result.Rows.AddRange(taskAttributesData);
                    return result;
            }

            //Дефолтный отчет вроде как с инспекциями
            CreateMainTableColumbs(result);
            var afterFilterPaginate = await FilterPaginate(input, _logger);
            var inspections = afterFilterPaginate.Inspections;
            var tasks = afterFilterPaginate.TrainTask;

            //Инспекции
            result.Rows.AddRange(GetInspectionsData(inspections, _logger));

            var tasksWithOutInspections = new List<TrainTask>();
            foreach (var item in tasks)
            {
                var attributes = await sqlRTrainTaskAttributes.ByTaskId(item.Id);
                var attributesWithOutInspections = attributes.Where(x => x.InspectionId == null).ToList();
                if (attributesWithOutInspections.Any())
                    tasksWithOutInspections.Add(item);
            }
            result.Rows.AddRange(GetTaskData
                (tasksWithOutInspections, _logger).Result);


            result.Total = afterFilterPaginate.Total.ToString();
            return result;
        }

        private static async Task<FilteredTaskInspection> FilterPaginate(ReportRequest input, ILogger logger, bool isFillTask = true, bool isFillInspection = true)
        {
            var sqlRInspections = new InspectionRepository(logger);
            var sqlRTask = new TaskRepository(logger);
            var sqlRTaskStatus = new TaskStatusRepository(logger);
            var sharedTaskInspections = new List<SharedTaskInspection>();

            if (!int.TryParse(input.Paging.Skip, out var skip))
                throw new ValidationException(Error.ParserErrorInt);

            if (!int.TryParse(input.Paging.Limit, out var limit))
                throw new ValidationException(Error.ParserErrorInt);



            List<Inspection> inspections;
            if (isFillInspection)
            {
                inspections = await sqlRInspections.GetAllSortByProperty("DateStart", "DESC");
                foreach (var inspection in inspections)
                {
                    sharedTaskInspections.Add(new SharedTaskInspection { Inspection = inspection });
                }
            }

            List<TrainTask> tasks;
            if (isFillTask)
            {
                tasks = await sqlRTask.GetAllSortByProperty("CreateDate", "DESC");
                foreach (var item in tasks)
                {
                    sharedTaskInspections.Add(new SharedTaskInspection { TrainTask = item });
                }
            }

            var sharedTaskInspectionFiltered = new List<SharedTaskInspection>();

            if (input.Filters.Any())
            {
                foreach (var filter in input.Filters)
                {
                    foreach (var sharedTaskInspection in sharedTaskInspections)
                    {


                        switch (filter.ColumnName)
                        {
                            //Статус
                            case "col1":
                                string filterField = null;
                                if (sharedTaskInspection.Inspection != null)
                                {
                                    filterField = GetStringInspectionStatus(sharedTaskInspection.Inspection.Status);
                                }
                                if (sharedTaskInspection.TrainTask != null)
                                {
                                    var currentTaskStatus = (await sqlRTaskStatus.ByTaskId(sharedTaskInspection.TrainTask.Id)).Last();
                                    filterField = GetStringTaskStatus(currentTaskStatus.Status);
                                }

                                if (StringFilter(filterField, filter))
                                    sharedTaskInspectionFiltered.Add(sharedTaskInspection);
                                break;
                            default:
                                throw new NotImplementedException(Error.InDevelopment);
                        }
                    }

                }

                sharedTaskInspections = sharedTaskInspectionFiltered;
            }

            if (skip > sharedTaskInspections.Count)
                sharedTaskInspections = new List<SharedTaskInspection>();
            if (skip == sharedTaskInspections.Count)
                limit = 1;
            if (skip + limit > sharedTaskInspections.Count)
                limit = sharedTaskInspections.Count - skip;
            var total = sharedTaskInspections.Count;
            sharedTaskInspections = sharedTaskInspections.GetRange(skip, limit);

            inspections = sharedTaskInspections.Where(x => x.Inspection != null).Select(q => q.Inspection).ToList();
            tasks = sharedTaskInspections.Where(x => x.TrainTask != null).Select(q => q.TrainTask).ToList();


            return new FilteredTaskInspection { Inspections = inspections, TrainTask = tasks, Total = total };
        }

        private static bool StringFilter(string filterField, Filter filter)
        {
            switch (filter.Operation)
            {
                case "contains":
                    if (filterField.Contains(filter.Value))
                        return true;
                    break;
                case "notContains":
                    if (!filterField.Contains(filter.Value))
                        return true;
                    break;
            }
            return false;
        }

        public class FilteredTaskInspection
        {
            public List<Inspection> Inspections { get; set; }
            public List<TrainTask> TrainTask { get; set; }
            public int Total { get; set; }
        }

        public class SharedTaskInspection
        {
            public Inspection Inspection { get; set; }
            public TrainTask TrainTask { get; set; }
        }



        private static async Task<List<Row>> GetTaskAttributesData(List<TrainTaskAttribute> attributes, RowId parentId, ILogger logger)
        {


            var sqlRUsers = new UserRepository(logger);
            var sqlRTaskStatus = new TaskStatusRepository(logger);
            var sqlRTaskExecutors = new ExecutorRepository(logger);
            var sqlRTaskComment = new CommentRepository(logger);
            var sqlRCarriages = new CarriageRepository(logger);
            var sqlREquipmentModel = new EquipmentModelsRepository(logger);
            var sqlREquipment = new EquipmentRepository(logger);
            var sqlRTrains = new TrainRepository(logger);
            var sqlRTask = new TaskRepository(logger);
            var sqlRFault = new FaultsRepository(logger);

            var rows = new List<Row>();

            foreach (var attribute in attributes)
            {



                var task = await sqlRTask.ById(attribute.TrainTaskId);

                var currentTaskStatus = (await sqlRTaskStatus.ByTaskId(attribute.TrainTaskId)).Last();
                var carriage = await sqlRCarriages.ById(task.CarriageId);
                var user = GetUserById(sqlRUsers, task.UserId);
                var taskExecutor = (await sqlRTaskExecutors.GetByTaskId(task.Id)).Last();
                var equipmentModel = await sqlREquipmentModel.ById(task.EquipmentModelId);
                var equipment = await sqlREquipment.ById(equipmentModel.EquipmentId);
                var lastComment = (await sqlRTaskComment.GetByTaskId(task.Id)).LastOrDefault();

                var fault = new Fault(); ;
                if (attribute.FaultId != null)
                {
                    fault = await sqlRFault.ById(attribute.FaultId.Value);
                }

                var updateData = GetTaskLastUpdate(lastComment, taskExecutor, currentTaskStatus);

                var toadd = new Row
                {
                    Id = new RowId(attribute.Id, 3),
                    HasItems = false.ToString(),
                    ParentId = parentId,
                    //Ид
                    Col0 = task.Id.ToString(),
                    //Статус
                    Col1 = GetStringTaskStatus(currentTaskStatus.Status),
                    //Тип
                    Col2 = GetStringTaskType(task.TaskType),
                    //Состав
                    Col3 = GetTrainById(sqlRTrains, carriage.TrainId).Name,
                    //Инициатор
                    Col4 = user.Name,
                    //Исполнитель
                    Col5 = GetStringBrigadeType(taskExecutor.BrigadeType),
                    //Дата
                    Col6 = task.CreateDate.ToStringDateTime(),
                    ////Вагон
                    Col7 = carriage.Number.ToString(),
                    //Оборудование
                    Col8 = equipment.Name,
                    ////Типовая Неисправность
                    Col9 = fault?.Name,
                    //Обновлено
                    Col10 = updateData?.Date.ToStringDateTime(),
                    //Обновил
                    Col11 = updateData?.User.Name

                };
                toadd.AdditionalProperty = new AdditionalProperty { TaskId = task.Id };
                rows.Add(toadd);

            }

            return rows;
        }

        private static async Task<List<Row>> GetTaskDataChildInspection(List<TrainTask> tasks, RowId parentId, ILogger logger)
        {


            var sqlRUsers = new UserRepository(logger);
            var sqlRTaskStatus = new TaskStatusRepository(logger);
            var sqlRTaskExecutors = new ExecutorRepository(logger);
            var sqlRTaskComment = new CommentRepository(logger);
            var sqlRCarriages = new CarriageRepository(logger);
            var sqlREquipmentModel = new EquipmentModelsRepository(logger);
            var sqlREquipment = new EquipmentRepository(logger);
            var sqlRTrains = new TrainRepository(logger);
            var sqlRTrainTaskAttributes = new TrainTaskAttributesRepository(logger);
            var sqlRFault = new FaultsRepository(logger);

            var rows = new List<Row>();
            foreach (var task in tasks)
            {


                var currentTaskStatus = (await sqlRTaskStatus.ByTaskId(task.Id)).Last();
                var carriage = await sqlRCarriages.ById(task.CarriageId);
                var user = GetUserById(sqlRUsers, task.UserId);
                var taskExecutor = (await sqlRTaskExecutors.GetByTaskId(task.Id)).Last();
                var equipmentModel = await sqlREquipmentModel.ById(task.EquipmentModelId);
                var equipment = await sqlREquipment.ById(equipmentModel.EquipmentId);
                var taskAttributes = await sqlRTrainTaskAttributes.ByTaskId(task.Id);
                var lastComment = (await sqlRTaskComment.GetByTaskId(task.Id)).LastOrDefault();
                var updateData = GetTaskLastUpdate(lastComment, taskExecutor, currentTaskStatus);
                var fault = new Fault();
                if (taskAttributes.Count == 1 && taskAttributes.First().FaultId.HasValue)
                    fault = await sqlRFault.ById(taskAttributes.First().FaultId.Value);

                var toadd = new Row
                {
                    Id = new RowId(task.Id, 2),
                    HasItems = false.ToString(),
                    ParentId = parentId,
                    //Ид
                    Col0 = "И" + task.Id,
                    //Статус
                    Col1 = GetStringTaskStatus(currentTaskStatus.Status),
                    //Тип
                    Col2 = GetStringTaskType(task.TaskType),
                    //Состав
                    Col3 = GetTrainById(sqlRTrains, carriage.TrainId).Name,
                    //Инициатор
                    Col4 = user.Name,
                    //Исполнитель
                    Col5 = GetStringBrigadeType(taskExecutor.BrigadeType),
                    //Дата
                    Col6 = task.CreateDate.ToStringDateTime(),
                    ////Вагон
                    Col7 = carriage.Number.ToString(),
                    //Оборудование
                    Col8 = equipment.Name,
                    ////Типовая Неисправность
                    Col9 = fault?.Name,
                    //Обновлено
                    Col10 = updateData?.Date.ToStringDateTime(),
                    //Обновил
                    Col11 = updateData?.User.Name

                };
                if (taskAttributes.Count > 1)
                    toadd.HasItems = true.ToString();

                rows.Add(toadd);
            }

            return rows;
        }

        private static async Task<List<Row>> GetTaskData(List<TrainTask> tasks, ILogger logger)
        {


            var sqlRUsers = new UserRepository(logger);
            var sqlRTaskStatus = new TaskStatusRepository(logger);
            var sqlRTaskExecutors = new ExecutorRepository(logger);
            var sqlRTaskComment = new CommentRepository(logger);
            var sqlRCarriages = new CarriageRepository(logger);
            var sqlREquipmentModel = new EquipmentModelsRepository(logger);
            var sqlREquipment = new EquipmentRepository(logger);
            var sqlRTrains = new TrainRepository(logger);
            var sqlRTrainTaskAttributes = new TrainTaskAttributesRepository(logger);
            var sqlRFault = new FaultsRepository(logger);

            var rows = new List<Row>();
            foreach (var task in tasks)
            {


                var currentTaskStatus = (await sqlRTaskStatus.ByTaskId(task.Id)).Last();
                var carriage = await sqlRCarriages.ById(task.CarriageId);
                var user = GetUserById(sqlRUsers, task.UserId);

                var taskExecutor = (await sqlRTaskExecutors.GetByTaskId(task.Id)).Last();
                var equipmentModel = await sqlREquipmentModel.ById(task.EquipmentModelId);
                var equipment = await sqlREquipment.ById(equipmentModel.EquipmentId);
                var taskAttributes = await sqlRTrainTaskAttributes.ByTaskId(task.Id);
                var lastComment = (await sqlRTaskComment.GetByTaskId(task.Id)).LastOrDefault();
                var fault = new Fault();
                if (taskAttributes.Count == 1 && taskAttributes.First().FaultId.HasValue)
                    fault = await sqlRFault.ById(taskAttributes.First().FaultId.Value);


                var updateData = GetTaskLastUpdate(lastComment, taskExecutor, currentTaskStatus);

                var toadd = new Row
                {
                    Id = new RowId(task.Id, 2),
                    HasItems = false.ToString(),
                    ParentId = null,
                    //Ид
                    Col0 = "И" + task.Id,
                    //Статус
                    Col1 = GetStringTaskStatus(currentTaskStatus.Status),
                    //Тип
                    Col2 = GetStringTaskType(task.TaskType),
                    //Состав
                    Col3 = GetTrainById(sqlRTrains, carriage.TrainId).Name,
                    //Инициатор
                    Col4 = user.Name,
                    //Исполнитель
                    Col5 = GetStringBrigadeType(taskExecutor.BrigadeType),
                    //Дата
                    Col6 = task.CreateDate.ToStringDateTime(),
                    ////Вагон
                    Col7 = carriage.Number.ToString(),
                    //Оборудование
                    Col8 = equipment.Name,
                    ////Типовая Неисправность
                    Col9 = fault?.Name,
                    //Обновлено
                    Col10 = updateData?.Date.ToStringDateTime(),
                    //Обновил
                    Col11 = updateData?.User.Name
                };
                if (taskAttributes.Count > 1)
                    toadd.HasItems = true.ToString();

                rows.Add(toadd);
            }

            return rows;
        }

        private static List<Row> GetInspectionsData(List<Inspection> inspections, ILogger logger)
        {
            var sqlRTrains = new TrainRepository(logger);
            var sqlRUsers = new UserRepository(logger);
            var sqlRTrainTaskAttributes = new TrainTaskAttributesRepository(logger);

            var rows = new List<Row>();

            foreach (var inspection in inspections)
            {
                var user = GetUserById(sqlRUsers, inspection.UserId);
                var taskAttributesByInspectionCount =
                    GetTrainTaskAttributesCountByInspectionId(sqlRTrainTaskAttributes, inspection.Id).Count;
                var toadd = new Row
                {
                    Id = new RowId(inspection.Id, 1),
                    HasItems = false.ToString(),
                    ParentId = null,
                    //Ид
                    Col0 = "М" + inspection.Id,
                    //Статус
                    Col1 = GetStringInspectionStatus(inspection.Status),
                    //Тип
                    Col2 = GetStringInspectionType(inspection.CheckListType),
                    //Состав
                    Col3 = GetTrainById(sqlRTrains, inspection.TrainId).Name,
                    //Инициатор
                    Col4 = user.Name,
                    //Исполнитель
                    Col5 = null,
                    //Дата
                    Col6 = inspection.DateStart.ToStringDateTime(),
                    //Вагон
                    Col7 = null,
                    //Оборудование
                    Col8 = $"{taskAttributesByInspectionCount} инц.",
                    //Типовая Неисправность
                    Col9 = null
                };

                if (taskAttributesByInspectionCount != 0)
                    toadd.HasItems = true.ToString();

                rows.Add(toadd);
            }

            return rows;
        }

        private static void CreateTaskAttributesChildTableColumbs(ReportResponse result)
        {
            result.Columns = new List<Column>
            {
                new Column("col0", "Номер", "number"),
                new Column("col1", "Статус", "string"),
                new Column("col2", "Тип", "string"),
                new Column("col3", "Состав", "string"),
                new Column("col4", "Инициатор", "string"),
                new Column("col5", "Исполнитель", "string"),
                new Column("col6", "Дата", "date"),
                new Column("col7", "Вагон", "string"),
                new Column("col8", "Оборудование", "string"),
                new Column("col9", "Типовая неисправность", "string"),
                new Column("col10", "Обновлено", "date"),
                new Column("col11", "Обновил", "string"),
            };
        }

        private static void CreateMainTableColumbs(ReportResponse result)
        {
            result.Columns = new List<Column>
            {
                new Column("col0", "Номер", "number"),
                new Column("col1", "Статус", "string"),
                new Column("col2", "Тип", "string"),
                new Column("col3", "Состав", "string"),
                new Column("col4", "Инициатор", "string"),
                new Column("col5", "Исполнитель", "string"),
                new Column("col6", "Дата", "date"),
                new Column("col7", "Вагон", "string"),
                new Column("col8", "Оборудование", "string"),
                new Column("col9", "Типовая неисправность", "string"),
                new Column("col10", "Обновлено", "date"),
                new Column("col11", "Обновил", "string"),
            };
        }

        private static Brigade GetBrigadeById(BrigadeRepository sqlR, int id)
        {
            return sqlR.ById(id).Result;
        }

        private static User GetUserById(UserRepository sqlR, int id)
        {
            return sqlR.ById(id).Result;
        }

        private static Train GetTrainById(TrainRepository sqlR, int? id)
        {
            return sqlR.ById(id).Result;
        }


        private static List<TrainTaskAttribute> GetTrainTaskAttributesCountByInspectionId(TrainTaskAttributesRepository sqlR, int id)
        {
            return sqlR.ByInspectionId(id).Result;
        }


        private class UpdateUserDateDto
        {
            public DateTime Date { get; set; }

            public User User { get; set; }
        }

        private static UpdateUserDateDto GetTaskLastUpdate(TrainTaskComment lastComment, TrainTaskExecutor lastExecutor,
            TrainTaskStatus lastStatus)
        {
            var items = new List<UpdateUserDateDto>();

            if (lastComment != null)
            {
                items.Add(new UpdateUserDateDto
                {
                    Date = lastComment.Date,
                    User = lastComment.User
                });
            }

            if (lastExecutor != null)
            {
                items.Add(new UpdateUserDateDto
                {
                    Date = lastExecutor.Date,
                    User = lastExecutor.User
                });
            }

            if (lastStatus != null)
            {
                items.Add(new UpdateUserDateDto
                {
                    Date = lastStatus.Date,
                    User = lastStatus.User
                });
            }

            return items.Any() ? items.OrderBy(o => o.Date).Last() : null;
        }

    }
}