using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Rzdppk.Api;
using Rzdppk.Api.Requests;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Model;
using Rzdppk.Model.Enums;
using Rzdppk.Model.Raspisanie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Rzdppk.Api.ScheludeChangedDtos;
using static Rzdppk.Core.Other.DevExtremeTableData;
using static Rzdppk.Core.Services.ScheduleCycleService;
using TaskStatus = Rzdppk.Model.Enums.TaskStatus;

namespace Rzdppk.Core.Services
{
    public class ScheludeChangedTableService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ScheludeChangedTableService(ILogger logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<ChangedData> ScheduleChangedTableManager(GetRouteInformationTableRequest input)
        {
            ChangedData result = new ChangedData();
            switch (input.TimelineTypeEnum)
            {
                //case (int)TimelineTypeEnum.TimeRangeTo2:
                //    result = await GetTo1Table(input.PlanedRouteTrainId, _logger);
                //    break;
                case (int)TimelineTypeEnum.TimeRangeTo2:
                case (int)TimelineTypeEnum.Cto:
                    result.To2 = await GetTo2OrCto(input, _logger);
                    break;
                case (int)TimelineTypeEnum.TimeRangeTrip:
                case (int)TimelineTypeEnum.TimeRangeTripTransfer:
                    result.TimeRangeTrip = await GetTripData(input, _logger);
                    break;
                //case (int)ScheludeChangedTableEnum.PriemkaPr:
                //    result = await GetPriemshikiInspectionsTable(input, planedRouteTrainId, _logger);
                //    break;
                case (int)TimelineTypeEnum.TimeBrigade:
                    result.TimeBrigade = await GetBrigadeData(input, _logger);
                    break;
                //case (int)ScheludeChangedTableEnum.Incidents:
                //    result = await GetNotClosedTrainTaskTable(input, planedRouteTrainId, _logger);
                //    break;
                //case (int)ScheludeChangedTableEnum.Depo:
                //    result = await GetDepoEventsTable(input, planedRouteTrainId, _logger);
                //    break;
                default:
                    throw new ValidationException("Неизвестный TimelineTypeEnum");
            }

            //PagingFilteringSorting(input, result);
            return result;
        }


        /// <summary>
        ///  Депо – вкладка с информацией о плановом назначении поезда на места постановки 
        /// </summary>
        private static async Task<ReportResponse> GetDepoEventsTable(ReportRequest input, int planedRouteTrainId, ILogger logger)
        {
            var sqlRTrains = new TrainRepository(logger);
            var sqlRTask = new TaskRepository(logger);
            var sqlRPlanedRouteTrains = new PlanedRouteTrainsRepository(logger);
            var sqlRRoute = new RoutesRepository(logger);
            var sqlRInspections = new InspectionRepository(logger);
            var sqlRTaskAttributes = new TrainTaskAttributesRepository(logger);
            var sqlREquipmentModel = new EquipmentModelsRepository(logger);
            var sqlREquipment = new EquipmentRepository(logger);
            var sqlRCarriage = new CarriageRepository(logger);
            var sqlR = new PrimitiveRepository(logger, "Parkings");
            var sqlRDepoEvent = new DepoEventsRepository(logger);



            var result = new ReportResponse { Rows = new List<Row>() };

            result.Columns = new List<Column>
            {
                new Column("col0", "Место постановки", "string"),
                new Column("col1", "Время захода на место постановки", "date"),
                new Column("col2", "Время выхода с места постановки", "date"),
            };

            var planedRouteTrain = await sqlRPlanedRouteTrains.ById(planedRouteTrainId);
            var events = await sqlRDepoEvent.ByTrainId(planedRouteTrain.TrainId);

            foreach (var item in events)
            {
                var parking = await sqlR.ById<Parking>(item.ParkingId);
                string testStopTime = null;
                if (item.TestStopTime != null)
                    testStopTime = ((DateTime)item.TestStopTime).ToStringDateTime();

                var row = new Row
                {
                    Id = new RowId(item.Id, 1),
                    HasItems = false.ToString(),
                    ParentId = null,
                    //Место постановки
                    Col0 = parking.Name,
                    //Время захода на место постановки
                    Col1 = item.InTime.ToStringDateTime(),
                    //Время выхода с места постановки
                    Col2 = testStopTime
                };

                result.Rows.Add(row);

            }

            return result;
        }


        /// <summary>
        ///  Инциденты – вкладка с перечнем открытых инцидентов на составе, с возможностью перехода в форму инцидента при клике по нему. Информация будет выводится в табличном виде, со следующим набором полей:
        /// </summary>
        private static async Task<ReportResponse> GetNotClosedTrainTaskTable(ReportRequest input, int planedRouteTrainId, ILogger logger)
        {
            var sqlRTrains = new TrainRepository(logger);
            var sqlRTask = new TaskRepository(logger);
            var sqlRPlanedRouteTrains = new PlanedRouteTrainsRepository(logger);
            var sqlRRoute = new RoutesRepository(logger);
            var sqlRInspections = new InspectionRepository(logger);
            var sqlRTaskAttributes = new TrainTaskAttributesRepository(logger);
            var sqlREquipmentModel = new EquipmentModelsRepository(logger);
            var sqlREquipment = new EquipmentRepository(logger);
            var sqlRCarriage = new CarriageRepository(logger);
            var sqlRTaskStatus = new TaskStatusRepository(logger);

            var result = new ReportResponse { Rows = new List<Row>() };

            result.Columns = new List<Column>
            {
                new Column("col0", "Номер", "number"),
                new Column("col1", "Статус", "string"),
                new Column("col2", "Тип", "string"),
                new Column("col3", "Вагон", "string"),
                new Column("col4", "Оборудование", "string"),
                new Column("col5", "Дата", "date"),
            };

            var planedRouteTrain = await sqlRPlanedRouteTrains.ById(planedRouteTrainId);
            //var route = await sqlRRoute.ById(planedRouteTrain.RouteId);

            var trainTasks = await sqlRTask.ByTrainId(planedRouteTrain.TrainId);
            var notClosedTasks = new List<TrainTask>();
            foreach (var trainTask in trainTasks)
            {
                var status = await sqlRTaskStatus.ByTaskId(trainTask.Id);
                if (status.Length == 0)
                    continue;
                if (status.Last().Status != TaskStatus.Closed)
                    notClosedTasks.Add(trainTask);
            }

            foreach (var notClosedTask in notClosedTasks)
            {
                var taskAtributes = await sqlRTaskAttributes.ByTaskId(notClosedTask.Id);
                var lastStatus = await sqlRTaskStatus.ByTaskId(notClosedTask.Id);
                var carriage = await sqlRCarriage.GetById(notClosedTask.CarriageId);
                var equipmentModel = await sqlREquipmentModel.ById(notClosedTask.EquipmentModelId);
                var equpment = await sqlREquipment.ById(equipmentModel.EquipmentId);


                foreach (var taskAtribute in taskAtributes)
                {
                    var row = new Row
                    {
                        Id = new RowId(notClosedTask.Id, 1),
                        HasItems = false.ToString(),
                        ParentId = null,
                        //Номер
                        Col0 = $"И{notClosedTask.Id}",
                        //Статус
                        Col1 = GetStringTaskStatus(lastStatus.Last().Status),
                        //Тип
                        Col2 = GetStringTaskType(notClosedTask.TaskType),
                        //Вагон
                        Col3 = $"{carriage.Number} - {carriage.Serial}",
                        //Оборудование
                        Col4 = equpment.Name,
                        //Дата
                        Col5 = notClosedTask.CreateDate.ToStringDateTime(),
                    };

                    result.Rows.Add(row);
                }
            }
            return result;
        }


        /// <summary>
        ///  Приемка/Сдача Пра
        /// </summary>
        private static async Task<ReportResponse> GetPriemshikiInspectionsTable(ReportRequest input, int planedRouteTrainId, ILogger logger)
        {
            var sqlRTrains = new TrainRepository(logger);
            var sqlRPlanedRouteTrains = new PlanedRouteTrainsRepository(logger);
            var sqlRRoute = new RoutesRepository(logger);
            var sqlRInspections = new InspectionRepository(logger);
            var sqlRTaskAttributes = new TrainTaskAttributesRepository(logger);
            var sqlRUser = new UserRepository(logger);
            var sqlRBrigade = new BrigadeRepository(logger);

            var result = new ReportResponse { Rows = new List<Row>() };

            result.Columns = new List<Column>
            {
                new Column("col0", "Состав", "string"),
                new Column("col1", "Тип", "string"),
                new Column("col2", "Время начала", "date"),
                new Column("col3", "Время окончания", "date"),
                new Column("col4", "Меток считано", "number"),
                new Column("col5", "Новых инцидентов заведено (из них критических)", "number"),
                new Column("col6", "Инцидентов на составе всего", "string"),
            };

            var planedRouteTrain = await sqlRPlanedRouteTrains.ById(planedRouteTrainId);
            //var route = await sqlRRoute.ById(planedRouteTrain.RouteId);

            var currentTrainInspections = await sqlRInspections.GetByTrainId(planedRouteTrain.TrainId);
            var currentDayInspections =
                currentTrainInspections.Where(x => x.DateStart.Date.Equals(planedRouteTrain.CreateDate));



            foreach (var currentDayInspection in currentDayInspections)
            {
                var user = await sqlRUser.ById(currentDayInspection.UserId);
                if (user.BrigadeId == null)
                    continue;

                var brigade = await sqlRBrigade.ById((int)user.BrigadeId);
                if (brigade.BrigadeType != BrigadeType.Receiver)
                    continue;

                var row = new Row
                {
                    Id = new RowId(currentDayInspection.Id, 1),
                    HasItems = false.ToString(),
                    ParentId = null,
                    //Состав
                    Col0 = (await sqlRTrains.ById(planedRouteTrain.TrainId)).Name,
                    //Тип
                    Col1 = GetStringInspectionType(currentDayInspection.CheckListType),
                    //Время начала
                    Col2 = currentDayInspection.DateStart.ToStringDateTime(),
                    //Время окончания
                    Col3 = currentDayInspection.DateEnd?.ToStringDateTime(),
                    //Меток считано
                    Col4 = "666",
                    //Новых инцидентов заведено (из них критических)
                    Col5 = "666",
                    //Инцидентов на составе всего
                    Col6 = "666",
                };

                result.Rows.Add(row);
            }

            return result;
        }


        /// <summary>
        ///  Приемка/Сдача ЛБ таблица
        /// </summary>
        private static async Task<ReportResponse> GetLocomotvInspectionsTable(ReportRequest input, int planedRouteTrainId, ILogger logger)
        {
            var sqlRTrains = new TrainRepository(logger);
            var sqlRPlanedRouteTrains = new PlanedRouteTrainsRepository(logger);
            var sqlRRoute = new RoutesRepository(logger);
            var sqlRInspections = new InspectionRepository(logger);
            var sqlRTaskAttributes = new TrainTaskAttributesRepository(logger);
            var sqlRUser = new UserRepository(logger);
            var sqlRBrigade = new BrigadeRepository(logger);

            var result = new ReportResponse { Rows = new List<Row>() };

            result.Columns = new List<Column>
            {
                new Column("col0", "Состав", "string"),
                new Column("col1", "Тип", "string"),
                new Column("col2", "Время начала", "date"),
                new Column("col3", "Время окончания", "date"),
                new Column("col4", "Меток считано", "number"),
                new Column("col5", "Новых инцидентов заведено (из них критических)", "number"),
                new Column("col6", "Инцидентов на составе всего", "string"),
                new Column("col7", "Километров всего", "number"),
                new Column("col8", "Километров за смену", "number"),
                new Column("col9", "КВт*ч (по каждому вагону)", "string"),
            };

            var planedRouteTrain = await sqlRPlanedRouteTrains.ById(planedRouteTrainId);
            //var route = await sqlRRoute.ById(planedRouteTrain.RouteId);

            var currentTrainInspections = await sqlRInspections.GetByTrainId(planedRouteTrain.TrainId);
            var currentDayInspections =
                currentTrainInspections.Where(x => x.DateStart.Date.Equals(planedRouteTrain.CreateDate));



            foreach (var currentDayInspection in currentDayInspections)
            {
                var user = await sqlRUser.ById(currentDayInspection.UserId);
                if (user.BrigadeId == null)
                    continue;

                var brigade = await sqlRBrigade.ById((int)user.BrigadeId);
                if (brigade.BrigadeType != BrigadeType.Locomotiv)
                    continue;

                var row = new Row
                {
                    Id = new RowId(currentDayInspection.Id, 1),
                    HasItems = false.ToString(),
                    ParentId = null,
                    //Состав
                    Col0 = (await sqlRTrains.ById(planedRouteTrain.TrainId)).Name,
                    //Тип
                    Col1 = GetStringInspectionType(currentDayInspection.CheckListType),
                    //Время начала
                    Col2 = currentDayInspection.DateStart.ToStringDateTime(),
                    //Время окончания
                    Col3 = currentDayInspection.DateEnd?.ToStringDateTime(),
                    //Меток считано
                    Col4 = "666",
                    //Новых инцидентов заведено (из них критических)
                    Col5 = "666",
                    //Инцидентов на составе всего
                    Col6 = "666",
                    //Километров всего
                    Col7 = "666",
                    //Километров за смену
                    Col8 = "666",
                    //КВт*ч (по каждому вагону)
                    Col9 = "In Development",
                };

                result.Rows.Add(row);
            }

            return result;
        }

        /// <summary>
        ///  То1 таблица
        /// </summary>
        private static async Task<ReportResponse> GetTo1Table(int planedRouteTrainId, ILogger logger)
        {
            var sqlRTrains = new TrainRepository(logger);
            var sqlRPlanedRouteTrains = new PlanedRouteTrainsRepository(logger);
            var sqlRRoute = new RoutesRepository(logger);
            var sqlRInspections = new InspectionRepository(logger);
            var sqlRTaskAttributes = new TrainTaskAttributesRepository(logger);
            var sqlRUser = new UserRepository(logger);

            var result = new ReportResponse { Rows = new List<Row>() };

            result.Columns = new List<Column>
            {
                new Column("col0", "Номер", "number"),
                new Column("col1", "Состав", "string"),
                new Column("col2", "Рейс", "string"),
                new Column("col3", "Время начала", "date"),
                new Column("col4", "Время окончания", "date"),
                new Column("col5", "Количество созданных инцидентов", "number"),
                new Column("col6", "Исполнитель", "string"),
                new Column("col7", "Меток считано всего (в том числе при обходе и закрытии задач)", "number"),
            };

            var planedRouteTrain = await sqlRPlanedRouteTrains.ById(planedRouteTrainId);
            var route = await sqlRRoute.ById(planedRouteTrain.RouteId);

            //Надо получить ТО 1 на этот день или блядь для этого поезда. хуй его знает
            var currentTrainInspections = await sqlRInspections.GetByTrainId(planedRouteTrain.TrainId);
            //Выбираем за текущий денек
            var currentDayInspections =
                currentTrainInspections.Where(x => x.DateStart.Date.Equals(planedRouteTrain.CreateDate) && x.CheckListType == CheckListType.TO1);

            foreach (var currentDayInspection in currentDayInspections)
            {
                var row = new Row
                {
                    Id = new RowId(currentDayInspection.Id, 1),
                    HasItems = false.ToString(),
                    ParentId = null,
                    //Номер
                    Col0 = currentDayInspection.Id.ToString(),
                    //Состав
                    Col1 = (await sqlRTrains.ById(planedRouteTrain.TrainId)).Name,
                    //Рейс
                    Col2 = route.Name,
                    //Время начала
                    Col3 = currentDayInspection.DateStart.ToStringDateTime(),
                    //Время окончания
                    Col4 = currentDayInspection.DateEnd?.ToStringDateTime(),
                    //Количество созданных инцидентов
                    Col5 = (await sqlRTaskAttributes.ByInspectionId(currentDayInspection.Id))?.Count.ToString(),
                    //Исполнитель
                    Col6 = (await sqlRUser.ById(currentDayInspection.UserId))?.Name,
                    //Меток считано всего (в том числе при обходе и закрытии задач)
                    Col7 = "In Development",
                };

                result.Rows.Add(row);

            }

            return result;
        }

        /// <summary>
        ///  Рейсы со станками таблица
        /// </summary>
        private static async Task<RouteInformationTableTrip> GetTripData(GetRouteInformationTableRequest input, ILogger logger)
        {
            var sqlRStations = new StantionsRepository(logger);
            var sqlRTrips = new TripsRepository(logger);
            var sqlRTrains = new TrainRepository(logger);
            var sqlRChangedStation = new ChangePlaneStantionOnTripsRepository(logger);
            var sqlRPlannedRouteTrains = new PlanedRouteTrainsRepository(logger);
            var sqlRPlannedStations = new PlanedStationOnTripsRepository(logger);

            var result = new RouteInformationTableTrip
            {
                Stantions = new List<RouteInformationTableStantion>(),
                DataSource = new DataSource()
            };

            var plannedRouteTrain = await sqlRPlannedRouteTrains.ById(input.PlanedRouteTrainId);
            var plannedStations = await sqlRPlannedStations.ByPlannedRouteTrainId(input.PlanedRouteTrainId);
            var tripId = plannedStations.Where(x => x.TripId == input.EntityId).Select(x => x.TripId).ToList();
            if (!tripId.Any())
                return result;

            var trip = await sqlRTrips.ById(tripId.First());
            result.Trip = trip.Name;
            var currentTripPlaneStations = plannedStations.Where(x => x.TripId == trip.Id);
            foreach (var currentTripPlaneStation in currentTripPlaneStations)
            {

                var planTrain = await sqlRTrains.ById(plannedRouteTrain.TrainId);

                var changedStationData =
                    await sqlRChangedStation.ByPlaneStantionOnTripId(currentTripPlaneStation.Id);
                //String changedTrainName;
                //if (changedStationData?.TrainId != null)
                //    changedTrainName = (await sqlRTrains.ById(changedStationData.TrainId)).Name;

                var toAdd = new RouteInformationTableStantion
                {
                    Id = currentTripPlaneStation.Id,
                    Name = (await sqlRStations.ById(currentTripPlaneStation.StantionId)).Name,
                    Train = planTrain.Name,
                    StartPlan = currentTripPlaneStation.InTime.ToStringTimeOnly(),
                    EndPlan = currentTripPlaneStation.OutTime.ToStringTimeOnly(),
                    TrainId = planTrain.Id,
                    StartFact = currentTripPlaneStation.InTime.ToFuckingGenaFormat(plannedRouteTrain.Date.Date),
                    EndFact = currentTripPlaneStation.OutTime.ToFuckingGenaFormat(plannedRouteTrain.Date.Date),
                    Canceled = false,
                    PlaneStationOnTripId = currentTripPlaneStation.Id
                };

                if (changedStationData != null)
                {
                    toAdd.StartFact = changedStationData.InTime.ToFuckingGenaFormat(plannedRouteTrain.Date.Date);
                    toAdd.EndFact = changedStationData.OutTime.ToFuckingGenaFormat(plannedRouteTrain.Date.Date);
                    toAdd.Canceled = changedStationData.Droped;
                    if (changedStationData.TrainId != null)
                        toAdd.TrainId = (int)changedStationData.TrainId;
                }
                result.Stantions.Add(toAdd);
            }

            var availableTrains = await GetAvaibleTrains(plannedRouteTrain.TrainId, logger);
            result.DataSource.Trains =
                availableTrains.Select(x => new DataSourceDto { Value = x.TrainId, Text = x.TrainName }).ToList();

            return result;
        }

        /// <summary>
        /// Бригады таблица
        /// </summary>
        private static async Task<ChangeTimeRangeBrigadeDto> GetBrigadeData(GetRouteInformationTableRequest input, ILogger logger)
        {
            var sqlRPlanedRouteTrains = new PlanedRouteTrainsRepository(logger);
            var sqlRTrips = new TripsRepository(logger);
            var sqlRPlanedStations = new PlanedStationOnTripsRepository(logger);
            var sqlRUsers = new UserRepository(logger);
            var sqlRStations = new StantionsRepository(logger);
            var sqlRPlaneBrigadeTrains = new PlaneBrigadeTrainsRepository(logger);
            var sqlRChangedBrigadeTrains = new ChangePlaneBrigadeTrainsRepository(logger);

            //var result = new ReportResponse { Rows = new List<Row>() };
            var stationsSimple = new List<StantionSimple>();

            //Берем всех юзерков
            var users = await sqlRUsers.GetAll();


            //Получим все станки с маршрута
            var planeStations = await sqlRPlanedStations.ByPlannedRouteTrainId(input.PlanedRouteTrainId);
            planeStations.OrderBy(x => x.InTime);

            var currentTripId = 0;
            for (var index = 0; index < planeStations.Count; index++)
            {
                var planeStation = planeStations[index];
                var trip = await sqlRTrips.ById(planeStation.TripId);
                var station = await sqlRStations.ById(planeStation.StantionId);
                var qqStart = planeStation.InTime.ToStringTimeOnly();
                var qqEnd = planeStation.OutTime.ToStringTimeOnly();

                if (currentTripId == 0 || planeStation.TripId != currentTripId)
                    qqStart = "н/д";
                if (index != planeStations.Count - 1)
                {
                    if (planeStations[index + 1].TripId != planeStation.TripId && currentTripId != 0)
                        qqEnd = "н/д";
                }
                else
                {
                    qqEnd = "н/д";
                }
                var qq1 = $"{station.Name} {qqStart}-{qqEnd} ({trip.Name})";
                stationsSimple.Add(new StantionSimple
                {
                    StantionId = planeStation.Id, StantionName = qq1, TripName = trip.Name, InTime = planeStation.InTime
                });
                currentTripId = planeStation.TripId;
            }

            var planeBrigades = await sqlRPlaneBrigadeTrains.ByPlanedRouteTrainId(input.PlanedRouteTrainId);
            planeBrigades = planeBrigades.Where(x => x.StantionStartId == input.EntityId).ToList();


            var result = new ChangeTimeRangeBrigadeDto
            {
                Users = new List<ChangeTimeRangeUserDto>(),
                DataSource = new DataSource
                {
                    Stantions = stationsSimple
                                .OrderBy(x => x.TripName).ThenBy(y => y.InTime)
                                .Select(x => new DataSourceDto { Value = x.StantionId, Text = x.StantionName })
                                .ToList(),

                    Users = users
                            .OrderBy(x => x.Name)
                            .Select(x => new DataSourceDto { Value = x.Id, Text = x.Name })
                            .ToList()
                }
            };

            foreach (var planeBrigade in planeBrigades)
            {
                var changeBrigade = await sqlRChangedBrigadeTrains.ByPlaneBrigadeTrainId(planeBrigade.Id) ?? new ChangePlaneBrigadeTrain { StantionEndId = 0, StantionStartId = 0, Droped = false, UserId = 0 };

                var planeStartStation = await sqlRPlanedStations.ById(planeBrigade.StantionStartId);
                var planeEndStation = await sqlRPlanedStations.ById(planeBrigade.StantionEndId);

                var realStartStation = await sqlRStations.ById(planeStartStation.StantionId);
                var realEndStation = await sqlRStations.ById(planeEndStation.StantionId);

                var planedUser = await sqlRUsers.ById(planeBrigade.UserId);

                var toAdd = new ChangeTimeRangeUserDto
                {
                    StartId = planeStartStation.Id,
                    EndId = planeEndStation.Id,
                    UserId = planedUser.Id,
                    User = planedUser.Name,
                    Start = realStartStation.Name,
                    End = realEndStation.Name,
                    PlaneBrigadeTrainId = planeBrigade.Id,
                    Canseled = changeBrigade.Droped
                };

                if (changeBrigade.UserId != 0)
                    toAdd.UserId = changeBrigade.UserId;
                if (changeBrigade.StantionEndId != 0)
                    toAdd.EndId = changeBrigade.StantionEndId;
                if (changeBrigade.StantionStartId != 0)
                    toAdd.StartId = changeBrigade.StantionStartId;


                result.Users.Add(toAdd);
            }
            return result;
        }

        /// <summary>
        /// То2 таблица
        /// </summary>
        private static async Task<ChangedTo2OrCtoDto> GetTo2OrCto(GetRouteInformationTableRequest input, ILogger logger)
        {
            var sqlRPlaneInspections = new PlanedInspectionRoutesRepository(logger);
            var sqlRChangedInspections = new ChangedPlanedInspectionRoutesRepository(logger);

            var planedInspection = await sqlRPlaneInspections.ById(input.EntityId);
            var changedInspection = await sqlRChangedInspections.ByPlanedInspectionRouteId(planedInspection.Id);

            var sqlRTrains = new TrainRepository(logger);
            var sqlRPlanedRouteTrains = new PlanedRouteTrainsRepository(logger);
            var sqlRRoute = new RoutesRepository(logger);

            var planedRouteTrain = await sqlRPlanedRouteTrains.ById(input.PlanedRouteTrainId);
            var route = await sqlRRoute.ById(planedRouteTrain.RouteId);
            var train = await sqlRTrains.ById(planedRouteTrain.TrainId);


            var result = new ChangedTo2OrCtoDto
            {
                Id = planedInspection.Id,
                RouteName = route.Name,
                TrainName = train.Name,
                Plan = new ChangedTo2OrCtoDto.Planned
                {
                    DateStart = planedInspection.Start.ToFuckingGenaFormat(planedRouteTrain.Date),
                    DateEnd = planedInspection.End.ToFuckingGenaFormat(planedRouteTrain.Date)
                },
                Fact = new ChangedTo2OrCtoDto.Actual
                {
                    DateStart = planedInspection.Start.ToFuckingGenaFormat(planedRouteTrain.Date),
                    DateEnd = planedInspection.End.ToFuckingGenaFormat(planedRouteTrain.Date),
                    Canseled = false
                }

            };
            if (changedInspection != null)
            {
                result.Fact.DateStart = changedInspection.Start.ToFuckingGenaFormat(planedRouteTrain.Date);
                result.Fact.DateEnd = changedInspection.End.ToFuckingGenaFormat(planedRouteTrain.Date);
                if (changedInspection.Droped)
                    result.Fact.Canseled = changedInspection.Droped;

            }

            return result;
        }



        private static async Task<List<TrainSimple>> GetAvaibleTrains(int trainId, ILogger logger)
        {
            var sqlRTrains = new TrainRepository(logger);
            var allTrains = await sqlRTrains.GetAll();
            var result = new List<TrainSimple>();
            foreach (var train in allTrains)
            {
                //if (train.Id == trainId)
                //    continue;
                result.Add(new TrainSimple { TrainName = train.Name, TrainId = train.Id });
            }

            return result;
        }

        /// <summary>
        /// Данные по станкам
        /// </summary>
        private static async Task GetPlaneStationsData(RouteData result, List<PlaneStantionOnTrip> planeStations,
            int tripId, ILogger logger)
        {
            var sqlRStations = new StantionsRepository(logger);
            var sqlRTrips = new TripsRepository(logger);
            var sqlRTrains = new TrainRepository(logger);
            var sqlRChangedStantion = new ChangePlaneStantionOnTripsRepository(logger);
            var sqlRPlanedRouteTrains = new PlanedRouteTrainsRepository(logger);

            var planedTripWithStation = new PlanedTripWithStations
            {
                TripName = (await sqlRTrips.ById(tripId)).Name,
                PlaneStations = new List<PlanedStation>()
            };

            var currentTripPlaneStations = planeStations.Where(x => x.TripId == tripId);
            foreach (var currentTripPlaneStation in currentTripPlaneStations)
            {
                var changedStationData = await sqlRChangedStantion.ByPlaneStantionOnTripId(currentTripPlaneStation.Id);
                var toAdd = new PlanedStation
                {
                    PlanedStationId = currentTripPlaneStation.Id,
                    StationName = (await sqlRStations.ById(currentTripPlaneStation.StantionId)).Name,
                    InTime = currentTripPlaneStation.InTime,
                    OutTime = currentTripPlaneStation.OutTime,
                    ChangeStationsId = changedStationData?.Id,
                    ChangedInTime = changedStationData?.InTime,
                    ChangedOutTime = changedStationData?.OutTime,
                    ChangeDroped = changedStationData?.Droped,
                    ChangeTrainId = changedStationData?.TrainId
                };

                //TODO надо уточнить стоит ли так делать. Возвращать нулл если поезд нихуя непоменялся
                var planedRouteTrain = await sqlRPlanedRouteTrains.ById(currentTripPlaneStation.PlanedRouteTrainId);
                if (planedRouteTrain.TrainId == toAdd.ChangeTrainId)
                    toAdd.ChangeTrainId = null;

                if (toAdd.ChangeTrainId != null)
                    toAdd.ChangeTranName = (await sqlRTrains.ById(toAdd.ChangeTrainId)).Name;

                planedTripWithStation.PlaneStations.Add(toAdd);
            }

            result.PlaneRoute.TripsWithStations.Add(planedTripWithStation);
        }


        /// <summary>
        /// Данные по ебучим юзерам меж станками
        /// </summary>
        private static async Task GetPlanedBrigadesData(int planedRouteTrainId, RouteData result, ILogger logger)
        {
            var sqlRPlanedStations = new PlanedStationOnTripsRepository(logger);
            var sqlRUsers = new UserRepository(logger);
            var sqlRStations = new StantionsRepository(logger);
            var sqlRPlaneBrigadeTrains = new PlaneBrigadeTrainsRepository(logger);
            var sqlRChangedBrigadeTrains = new ChangePlaneBrigadeTrainsRepository(logger);

            result.PlaneRoute.UserOnStations = new List<PlanedUserOnStations>();
            var planeBrigades = await sqlRPlaneBrigadeTrains.ByPlanedRouteTrainId(planedRouteTrainId);
            foreach (var planeBrigade in planeBrigades)
            {
                var stationStartId = (await sqlRPlanedStations.ById(planeBrigade.StantionStartId)).StantionId;
                var stationEndId = (await sqlRPlanedStations.ById(planeBrigade.StantionEndId)).StantionId;

                var changedBrigadeData = await sqlRChangedBrigadeTrains.ByPlaneBrigadeTrainId(planeBrigade.Id);

                var toAdd =
                    new PlanedUserOnStations
                    {
                        PlaneBrigadeTrainId = planeBrigade.Id,
                        UserName = (await sqlRUsers.ById(planeBrigade.UserId)).Name,
                        StationStartName = (await sqlRStations.ById(stationStartId)).Name,
                        StationEndName = (await sqlRStations.ById(stationEndId)).Name,
                        ChangeBrigadeTrainId = changedBrigadeData?.Id
                    };

                if (changedBrigadeData != null)
                {
                    toAdd.ChangedUserName = (await sqlRUsers.ById(planeBrigade.UserId)).Name;
                    toAdd.ChangedStationStartName = (await sqlRStations.ById(changedBrigadeData.StantionEndId)).Name;
                    toAdd.ChangedStationEndName = (await sqlRStations.ById(changedBrigadeData.StantionStartId)).Name;
                }


                result.PlaneRoute.UserOnStations.Add(toAdd);
            }
        }

        public class RouteData
        {
            public PlaneRouteData PlaneRoute { get; set; }
        }

        public class PlaneRouteData
        {
            public int PlanedRouteId { get; set; }
            public string RouteName { get; set; }
            public double Mileage { get; set; }
            public string TurnoverName { get; set; }
            public int PlanedTrainId { get; set; }
            public string PlanedTrainName { get; set; }
            public List<PlanedTripWithStations> TripsWithStations { get; set; }
            public List<PlanedUserOnStations> UserOnStations { get; set; }
        }

        public class PlanedTripWithStations
        {
            public string TripName { get; set; }
            public List<PlanedStation> PlaneStations { get; set; }
        }

        public class PlanedUserOnStations
        {
            public int PlaneBrigadeTrainId { get; set; }
            public string UserName { get; set; }
            public string StationStartName { get; set; }
            public string StationEndName { get; set; }
            public int? ChangeBrigadeTrainId { get; set; }
            public string ChangedUserName { get; set; }
            public string ChangedStationStartName { get; set; }
            public string ChangedStationEndName { get; set; }
        }

        public class PlanedStation
        {
            public int PlanedStationId { get; set; }
            public string StationName { get; set; }
            public DateTime InTime { get; set; }
            public DateTime OutTime { get; set; }
            public int? ChangeStationsId { get; set; }
            public bool? ChangeDroped { get; set; }
            public int? ChangeTrainId { get; set; }
            public string ChangeTranName { get; set; }
            public DateTime? ChangedInTime { get; set; }
            public DateTime? ChangedOutTime { get; set; }
        }
    }
}