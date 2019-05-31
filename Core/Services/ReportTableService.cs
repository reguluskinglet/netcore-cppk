using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using static Rzdppk.Core.Other.DevExtremeTableData;
using static Rzdppk.Core.Other.DevExtremeTableUtils;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using static Rzdppk.Core.Other.Other;


namespace Rzdppk.Core.Services
{
    public class ReportTableService
    {

        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ReportTableService(ILogger logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        //В эксельку кто когда сваливает с депо
        public async Task<ReportResponse> EscapeFromDepoReport(ExcelDataRequest input)
        {
            var sqlRPlaneTrain = new PlanedRouteTrainsRepository(_logger);
            var sqlRRoute = new RoutesRepository(_logger);
            var sqlRTrain = new TrainRepository(_logger);
            var sqlRTrip = new TripsRepository(_logger);
            var sqlRUser = new UserRepository(_logger);
            var sqlRPlaneStation = new PlanedStationOnTripsRepository(_logger);
            var sqlRPlaneBrigade = new PlaneBrigadeTrainsRepository(_logger);

            if (input.Date == null)
                throw new ValidationException("Не задан StartDate");
            
            var result = new ReportResponse { Rows = new List<Row>() };
            result.Columns = new List<Column>
            {
                new Column("col0", "Маршрут", "string"),
                //№ поезда(Походу рейс https://alcodevelop.atlassian.net/browse/CPPK-4)
                new Column("col1", "№ поезда",  "string"),
                new Column("col2", "Машинист", "string"),
                new Column("col3", "Состав", "string"),
                new Column("col4", "КП",  "string"),
            };

            //Бля ну получим все поезда наверно
            var planeTrains = await sqlRPlaneTrain.GetAll();
            //За указанные сутки
            planeTrains = planeTrains.Where(x => x.Date.Date == input.Date.Date).ToList();
            foreach (var planeTrain in planeTrains)
            {
                var route = await sqlRRoute.ById(planeTrain.RouteId);
                var train = await sqlRTrain.ById(planeTrain.TrainId);
                //Берем все станки маршрута
                var planeStations = await sqlRPlaneStation.ByPlannedRouteTrainId(planeTrain.Id);
                var startStation = planeStations.OrderBy(x => x.OutTime).FirstOrDefault();

                var trip = new Trip();
                if (startStation != null)
                    trip = await sqlRTrip.ById(startStation.TripId);
                var planeBrigades = await sqlRPlaneBrigade.ByPlanedRouteTrainId(planeTrain.Id);

                //TODO брать 1-го юзера и делать машинистом, както неправильно) хДД
                User motorman = null;
                if (planeBrigades.Count != 0)
                    motorman = await sqlRUser.ById(planeBrigades.First().UserId);

                var toadd = new Row
                {
                    Id = new RowId(DateTime.Now.Ticks.GetHashCode(), 2),
                    HasItems = false.ToString(),
                    ParentId = null,
                    //Маршрут
                    Col0 = route.Name,
                    //№ поезда(Походу рейс https://alcodevelop.atlassian.net/browse/CPPK-4)
                    Col1 = trip.Name,
                    //Машинист
                    Col2 = motorman?.Name,
                    //Состав
                    Col3 = train.Name,
                    //КП
                    Col4 = startStation?.OutTime.ToStringTimeOnly()
                };

                result.Rows.Add(toadd);
            }


            return result;
        }


        public async Task<ReportResponse> GetReportTasks(ReportRequest input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                if (!input.ReportFilter.StartDate.HasValue || !input.ReportFilter.EndDate.HasValue)
                    throw new Exception("не указан диапазон дат");

                var startDate = input.ReportFilter.StartDate.Value;
                var endDate = input.ReportFilter.EndDate.Value;

                const string sql = @"
                                    select ts.*,(select count(*) from TrainTaskStatuses tss where tss.TrainTaskId=ts.Id and tss.Status=8) as Repeats,
                                    (select count(*) from TrainTaskAttributes ta where ta.TrainTaskId=ts.Id) as Attributes,
                                    c.*,t.*,em.*,e.*,u.*,st.*
                                    from TrainTasks ts
                                    left join Carriages c on c.Id=ts.CarriageId
                                    left join Trains t on t.Id=c.TrainId
                                    left join EquipmentModels em on em.Id=ts.EquipmentModelId
                                    left join Equipments e on e.Id=em.EquipmentId
                                    left join auth_users u on u.Id=ts.UserId
                                    outer apply (select top 1 * from TrainTaskStatuses tss where tss.TrainTaskId=ts.Id order by tss.Date desc) as st
                                    where (select count(*) from TrainTaskStatuses tss where tss.TrainTaskId=ts.Id and tss.Status=8) > 0 and ts.CreateDate between @StartDate and @EndDate";
                var result = (await conn.QueryAsync(
                    sql,
                    new[]
                    {
                        typeof(ReportRepository.TrainTaskReport1), typeof(Carriage), typeof(Train), typeof(EquipmentModel),
                        typeof(Equipment), typeof(User), typeof(TrainTaskStatus)
                    },
                    objects =>
                    {
                        var task = (ReportRepository.TrainTaskReport1)objects[0];
                        task.Carriage = (Carriage)objects[1];
                        task.Carriage.Train = (Train)objects[2];
                        task.EquipmentModel = (EquipmentModel)objects[3];
                        task.EquipmentModel.Equipment = (Equipment)objects[4];
                        var user = (User)objects[5];
                        user.PasswordHash = null;
                        task.User = user;
                        task.Status = (TrainTaskStatus)objects[6];
                        return task;
                    }, new { StartDate = startDate, EndDate = endDate, repeat_task_status = Model.Enums.TaskStatus.Remake }
                )).ToArray();

                var sqlRmodel = new ModelRepository(_logger);
                foreach (var item in result)
                {
                    var carriageTypeId = (int)(await sqlRmodel.GetById(item.Carriage.ModelId)).ModelType;
                    switch (carriageTypeId)
                    {
                        case 0:
                            item.CarriageTypeString = CarriageTypeString.HeadVagon;
                            break;
                        case 1:
                            item.CarriageTypeString = CarriageTypeString.TractionVagon;
                            break;
                        case 2:
                            item.CarriageTypeString = CarriageTypeString.TrailerVagon;
                            break;
                    }
                }

                var dataToTable = new ReportResponse { Rows = new List<Row>() };
                CreateMainTableColumbs(dataToTable);

                foreach (var value in result)
                {
                    var toadd = new Row
                    {
                        Id = new RowId(value.Id, 2),
                        HasItems = false.ToString(),
                        ParentId = null,
                        //Номер Задачи
                        Col0 = value.Id.ToString(),
                        //Статус
                        Col1 = GetStringTaskStatus(value.Status?.Status ?? Model.Enums.TaskStatus.New),
                        //Тип
                        Col2 = GetStringTaskType(value.TaskType),
                        //Состав
                        Col3 = value.Carriage.Train.Name,
                        //Вагон
                        Col4 = value.Carriage.Serial + " (" + value.Carriage.Number + ", " + value.CarriageTypeString + ")",
                        //Оборудование
                        Col5 = value.EquipmentModel.Equipment.Name,
                        //Кол-во неисправностей
                        Col6 = value.Attributes.ToString(),
                        ////Инициатор
                        Col7 = value.User.Name,
                        //Дата
                        Col8 = value.CreateDate.ToStringDateTime(),
                        ////Переоткрыто раз
                        Col9 = value.Repeats.ToString()
                    };

                    dataToTable.Rows.Add(toadd);
                }

                return dataToTable;
            }
        }

        private static void CreateMainTableColumbs(ReportResponse result)
        {
            result.Columns = new List<Column>
            {
                new Column("col0", "Номер Задачи", "number"),
                new Column("col1", "Статус", "string"),
                new Column("col2", "Тип", "string"),
                new Column("col3", "Состав", "string"),
                new Column("col4", "Вагон", "string"),
                new Column("col5", "Оборудование", "string"),
                new Column("col6", "Кол-во неисправностей", "number"),
                new Column("col7", "Инициатор", "string"),
                new Column("col8", "Дата", "date"),
                new Column("col9", "Переоткрыто раз", "number")
            };
        }





        //public async Task<ReportResponse> GetTaskAndInspections(ReportRequest input)
        //{

        //    var sqlRInspections = new InspectionRepository(_logger);
        //    var sqlRTask = new TaskRepository(_logger);
        //    var sqlRTrainTaskAttributes = new TrainTaskAttributesRepository(_logger);

        //    var result = new ReportResponse {Rows = new List<Row>()};

        //    if (input.ParentId != null)
        //    {
        //        if (input.ParentId.Type == (int)EventTableEnum.Inspection)
        //        {
        //            CreateTaskAttributesChildTableColumbs(result);
        //            var traintTaskAttributesByInspectionId = await sqlRTrainTaskAttributes.ByInspectionId(input.ParentId.Id);
        //            var taskAttributesData =
        //                GetTaskAttributesData(traintTaskAttributesByInspectionId, input.ParentId, _logger).Result;
        //            result.Rows.AddRange(taskAttributesData);
        //        }

        //        if (input.ParentId.Type == (int)EventTableEnum.TrainTask)
        //        {
        //            CreateTaskAttributesChildTableColumbs(result);
        //            var traintTaskAttributesByTaskId = await sqlRTrainTaskAttributes.ByTaskId(input.ParentId.Id);
        //            var taskAttributesData =
        //                GetTaskAttributesData(traintTaskAttributesByTaskId, input.ParentId, _logger).Result;
        //            result.Rows.AddRange(taskAttributesData);
        //        }


        //        PagingFilteringSorting(input, result);
        //        return result;
        //    }

        //    CreateMainTableColumbs(result);


        //    var inspections = await sqlRInspections.GetAll();
        //    result.Rows.AddRange(GetInspectionsData(inspections, _logger));

        //    var tasks = await sqlRTask.GetAll();
        //    result.Rows.AddRange(GetTaskData
        //        (tasks, _logger).Result);

        //    PagingFilteringSorting(input, result);

        //    return result;
        //}

        //private static void PagingFilteringSorting(ReportRequest input, ReportResponse result)
        //{
        //    result.Rows = DevExtremeTableFiltering(result.Rows, input.Filters);
        //    result.Rows = DevExtremeTableSorting(result.Rows, input.Sortings);
        //    result.Total = result.Rows.Count.ToString();
        //    result.Paging(input.Paging);
        //}

        //private static async Task<List<Row>> GetTaskAttributesData(List<TrainTaskAttribute> attributes, RowId parentId, ILogger logger)
        //{


        //    var sqlRUsers = new UserRepository(logger);
        //    var sqlRTaskStatus = new TaskStatusRepository(logger);
        //    var sqlRTaskExecutors = new ExecutorRepository(logger);
        //    var sqlRCarriages = new CarriageRepository(logger);
        //    var sqlREquipmentModel = new EquipmentModelsRepository(logger);
        //    var sqlREquipment = new EquipmentRepository(logger);
        //    var sqlRTrains = new TrainRepository(logger);
        //    var sqlRTask = new TaskRepository(logger);

        //    var rows = new List<Row>();

        //    foreach (var attribute in attributes)
        //    {

        //        var task = await sqlRTask.ById(attribute.TrainTaskId);

        //        var currentTaskStatus = (await sqlRTaskStatus.ByTaskId(attribute.TrainTaskId)).Last();
        //        var carriage = await sqlRCarriages.ById(task.CarriageId);
        //        var user = GetUserById(sqlRUsers, task.UserId);
        //        var taskExecutor = (await sqlRTaskExecutors.ByTaskId(task.Id)).Last();
        //        var equipmentModel = await sqlREquipmentModel.ById(task.EquipmentModelId);
        //        var equipment = await sqlREquipment.ById(equipmentModel.EquipmentId);


        //        var toadd = new Row
        //        {
        //            Id = new RowId(DateTime.Now.Ticks.GetHashCode(), 2),
        //            HasItems = false.ToString(),
        //            ParentId = parentId,
        //            //Ид
        //            Col0 = task.Id.ToString(),
        //            //Статус
        //            Col1 = GetStringTaskStatus(currentTaskStatus.Status),
        //            //Тип
        //            Col2 = GetStringTaskType(task.TaskType),
        //            //Состав
        //            Col3 = GetTrainById(sqlRTrains, carriage.TrainId).Name,
        //            //Инициатор
        //            Col4 = user.Name,
        //            //Исполнитель
        //            Col5 = GetStringBrigadeType(taskExecutor.BrigadeType),
        //            //Дата
        //            Col6 = task.CreateDate.ToStringDateTime(),
        //            ////Вагон
        //            Col7 = carriage.Number.ToString(),
        //            //Оборудование
        //            Col8 = equipment.Name,
        //            ////Типовая Неисправность
        //            Col9 = null

        //        };
        //        rows.Add(toadd);

        //    }

        //    return rows;
        //}

        //private static async Task<List<Row>> GetTaskData(List<TrainTask> tasks, ILogger logger)
        //{


        //    var sqlRUsers = new UserRepository(logger);
        //    var sqlRTaskStatus = new TaskStatusRepository(logger);
        //    var sqlRTaskExecutors = new ExecutorRepository(logger);
        //    var sqlRCarriages = new CarriageRepository(logger);
        //    var sqlREquipmentModel = new EquipmentModelsRepository(logger);
        //    var sqlREquipment = new EquipmentRepository(logger);
        //    var sqlRTrains = new TrainRepository(logger);
        //    var sqlRTrainTaskAttributes = new TrainTaskAttributesRepository(logger);

        //    var rows = new List<Row>();
        //    foreach (var task in tasks)
        //    {


        //        var currentTaskStatus = (await sqlRTaskStatus.ByTaskId(task.Id)).Last();
        //        var carriage = await sqlRCarriages.ById(task.CarriageId);
        //        var user = GetUserById(sqlRUsers, task.UserId);
        //        var taskExecutor = (await sqlRTaskExecutors.ByTaskId(task.Id)).Last();
        //        var equipmentModel = await sqlREquipmentModel.ById(task.EquipmentModelId);
        //        var equipment = await sqlREquipment.ById(equipmentModel.EquipmentId);
        //        var taskAttributes = await sqlRTrainTaskAttributes.ByTaskId(task.Id);

        //        var toadd = new Row
        //        {
        //            Id = new RowId(task.Id, 2),
        //            HasItems = false.ToString(),
        //            ParentId = null,
        //            //Ид
        //            Col0 = task.Id.ToString(),
        //            //Статус
        //            Col1 = GetStringTaskStatus(currentTaskStatus.Status),
        //            //Тип
        //            Col2 = GetStringTaskType(task.TaskType),
        //            //Состав
        //            Col3 = GetTrainById(sqlRTrains, carriage.TrainId).Name,
        //            //Инициатор
        //            Col4 = user.Name,
        //            //Исполнитель
        //            Col5 = GetStringBrigadeType(taskExecutor.BrigadeType),
        //            //Дата
        //            Col6 = task.CreateDate.ToStringDateTime(),
        //            ////Вагон
        //            Col7 = carriage.Number.ToString(),
        //            //Оборудование
        //            Col8 = equipment.Name,
        //            ////Типовая Неисправность
        //            Col9 = null

        //        };
        //        if (taskAttributes.Count > 1)
        //            toadd.HasItems = true.ToString();

        //        rows.Add(toadd);
        //    }

        //    return rows;
        //}

        //private static List<Row> GetInspectionsData(List<Inspection> inspections, ILogger logger)
        //{
        //    var sqlRTrains = new TrainRepository(logger);
        //    var sqlRUsers = new UserRepository(logger);
        //    var sqlRTrainTaskAttributes = new TrainTaskAttributesRepository(logger);

        //    var rows = new List<Row>();

        //    foreach (var inspection in inspections)
        //    {
        //        var user = GetUserById(sqlRUsers, inspection.UserId);
        //        var taskAttributesByInspectionCount =
        //            GetTrainTaskAttributesCountByInspectionId(sqlRTrainTaskAttributes, inspection.Id).Count;
        //        var toadd = new Row
        //        {
        //            Id = new RowId(inspection.Id, 1),
        //            HasItems = false.ToString(),
        //            ParentId = null,
        //            //Ид
        //            Col0 = inspection.Id.ToString(),
        //            //Статус
        //            Col1 = GetStringInspectionStatus(inspection.Status),
        //            //Тип
        //            Col2 = GetStringInspectionType(inspection.CheckListType),
        //            //Состав
        //            Col3 = GetTrainById(sqlRTrains, inspection.TrainId).Name,
        //            //Инициатор
        //            Col4 = user.Name,
        //            //Исполнитель
        //            Col5 = null,
        //            //Дата
        //            Col6 = inspection.DateStart.ToStringDateTime(),
        //            //Вагон
        //            Col7 = null,
        //            //Оборудование
        //            Col8 = $"{taskAttributesByInspectionCount} инц.",
        //            //Типовая Неисправность
        //            Col9 = null

        //        };

        //        if (taskAttributesByInspectionCount != 0)
        //            toadd.HasItems = true.ToString();

        //        rows.Add(toadd);
        //    }

        //    return rows;
        //}

        //private static void CreateTaskAttributesChildTableColumbs(ReportResponse result)
        //{
        //    result.Columns = new List<Column>
        //    {
        //        new Column("col0", "Номер", "number"),
        //        new Column("col1", "Статус", "string"),
        //        new Column("col2", "Тип", "string"),
        //        new Column("col3", "Состав", "string"),
        //        new Column("col4", "Инициатор", "string"),
        //        new Column("col5", "Исполнитель", "string"),
        //        new Column("col6", "Дата", "date"),
        //        new Column("col7", "Вагон", "string"),
        //        new Column("col8", "Оборудование", "string"),
        //        new Column("col9", "Типовая неисправность", "string"),
        //    };
        //}

        //private static void CreateMainTableColumbs(ReportResponse result)
        //{
        //    result.Columns = new List<Column>
        //    {
        //        new Column("col0", "Номер", "number"),
        //        new Column("col1", "Статус", "string"),
        //        new Column("col2", "Тип", "string"),
        //        new Column("col3", "Состав", "string"),
        //        new Column("col4", "Инициатор", "string"),
        //        new Column("col5", "Исполнитель", "string"),
        //        new Column("col6", "Дата", "date"),
        //        new Column("col7", "Вагон", "string"),
        //        new Column("col8", "Оборудование", "string"),
        //        new Column("col9", "Типовая неисправность", "string"),
        //    };
        //}

        //private static Brigade GetBrigadeById(BrigadeRepository sqlR, int id)
        //{
        //    return sqlR.ById(id).Result;
        //}

        //private static User GetUserById(UserRepository sqlR, int id)
        //{
        //    return sqlR.ById(id).Result;
        //}

        //private static Train GetTrainById(TrainRepository sqlR, int? id)
        //{
        //    return sqlR.ById(id).Result;
        //}


        //private static List<TrainTaskAttribute> GetTrainTaskAttributesCountByInspectionId(TrainTaskAttributesRepository sqlR, int id)
        //{
        //    return sqlR.ByInspectionId(id).Result;
        //}




    }
}