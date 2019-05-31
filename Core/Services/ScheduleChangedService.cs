using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;
using Rzdppk.Model.Raspisanie;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using static Rzdppk.Core.Other.DevExtremeTableData;
using static Rzdppk.Core.Services.ScheduleCycleService;
using static Rzdppk.Core.Services.ScheduleCycleService.ScheduleColors;
using static Rzdppk.Core.Services.ScheludePlanedService;

namespace Rzdppk.Core.Services
{
    public class ScheduleChangedService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly int _userId;

        public ScheduleChangedService(ILogger logger, IMapper mapper, int userId)
        {
            _logger = logger;
            _mapper = mapper;
            _userId = userId;
        }

        public async Task<ChangedRoutesWithTimelinePaging> ChangedRouteTrainsTable(DateTime date)
        {
            var planService = new ScheludePlanedService(_logger, _mapper);
            var planTableData =
                await planService.PlanedRouteTrainsTable(date, date.AddHours(23).AddMinutes(59).AddSeconds(59));


            var result = new List<SheludeChangedRouteTripsTo>();
            foreach (var planTableItem in planTableData)
            {
                if (planTableItem.DaysData == null || planTableItem.DaysData.Count == 0)
                    continue;
                var currentPlanedRouteTrainId = planTableItem.DaysData.First().PlanedRouteTrainId;
                if (currentPlanedRouteTrainId == 0)
                    continue;

                var toAdd = new SheludeChangedRouteTripsTo
                {
                    Mileage = planTableItem.Route.Mileage,
                    RouteName = planTableItem.Route.Name,
                    PlanedRouteId = currentPlanedRouteTrainId,
                    PlanTimeLines = new List<TimeLine>(),
                    ChangeTimeLines = new List<TimeLine>()
                };

                int plannedTrainId = AddTimelineDictionary(planTableItem, toAdd);

                //Плановые
                await AddInspectionTimelines(planTableItem, toAdd, date);

                //Неплановые инспекции
                await AddNotPlanedInspectionTimeLines(planTableItem, toAdd, _logger, date);

                //Критичные инциденты
                await AddCriticalTasksTimeLines(planTableItem, toAdd, date, _logger);

                //Постановка на канал Только на ФАКТЕ
                await AddDepotParking(planTableItem, toAdd, date, _logger);

                //Вход в депо
                await AddEnterToDepot(planTableItem, toAdd, date, _logger);

                //Бригады на таймлайне
                await AddBrigadesTimelines(date, currentPlanedRouteTrainId, toAdd);

                //Рейсы на таймлайне
                await AddTripsTimelines(date, currentPlanedRouteTrainId, toAdd, plannedTrainId);

                result.Add(toAdd);
            }



            var page = new ChangedRoutesWithTimelinePaging
            {
                Data = result,
                Total = result.Count
            };

            return page;
        }

        private static async Task<string> FillNotPlannedInspectionDescription(int inspectionId, CheckListType checkListType, ILogger logger)
        {
            var sqlRInspection = new InspectionRepository(logger);
            var sqlRUser = new UserRepository(logger);
            var sqlRTaskAttribute = new TrainTaskAttributesRepository(logger);
            var inspection = await sqlRInspection.ById(inspectionId);
            var user = await sqlRUser.ById(inspection.UserId);
            var inspectionTasks = await sqlRTaskAttribute.ByInspectionId(inspectionId);
            var sqlBrigades = new BrigadeRepository(logger);
            //Нехуй слать юзера без бригады
            if (user.BrigadeId == null)
                throw Error.CommonError;
            var brigade = await sqlBrigades.ById((int)user.BrigadeId);
            var brigadeTypeString = GetStringBrigadeType(brigade.BrigadeType);
            var inspectionTimeString = inspection.DateStart.ToStringTimeOnly();
            if (inspection.DateEnd != null)
                inspectionTimeString += $" - {((DateTime)inspection.DateEnd).ToStringTimeOnly()}";


            switch (checkListType)
            {
                case CheckListType.TO1:
                    return $"{user.Name} <br /> Количество инцидентов: {inspectionTasks.Count}";
                case CheckListType.Inspection:
                    return $"<b>Приемка поезда</b>" +
                          $"<p>{inspectionTimeString}</p>" +
                          $"{brigadeTypeString} <br />" +
                          $"{user.Name}<br />" +
                          $"Количество инцидентов: {inspectionTasks.Count}";
                case CheckListType.Surrender:
                    return $"<b>Сдача поезда</b>" +
                           $"<p>{inspectionTimeString}</p>" +
                           $"{brigadeTypeString}<br />" +
                           $"{user.Name}<br />" +
                           $"Количество инцидентов: {inspectionTasks.Count}";
                default: return string.Empty;

            }
        }


        private static int AddTimelineDictionary(PlanedRouteTrainDto planTableItem, SheludeChangedRouteTripsTo toAdd)
        {
            toAdd.Trains = new Trains();
            var plannedTrainId = 0;
            if (planTableItem.DaysData.Any())
            {
                toAdd.Trains.Planed = planTableItem.DaysData.First().Train.Name;
                plannedTrainId = planTableItem.DaysData.First().Train.Id;
            }
            toAdd.Trains.Change = new Change { Current = toAdd.Trains.Planed, Previous = new List<string>() };
            return plannedTrainId;
        }

        private static async Task<string> FillBrigadesDescription(PlaneBrigadeTrain plannedBrigade, ILogger logger, bool dropped = false)
        {
            var sqlRUser = new UserRepository(logger);
            var sqlBrigades = new BrigadeRepository(logger);

            var user = await sqlRUser.ById(plannedBrigade.UserId);
            //Нехуй слать юзера без бригады
            if (user.BrigadeId == null)
                throw new Microsoft.Rest.ValidationException($"У пользователя {user.Name} неуказана бригада");
            var brigade = await sqlBrigades.ById((int)user.BrigadeId);
            var brigadeTypeString = GetStringBrigadeType(brigade.BrigadeType);
            if (dropped)
                return $"<i>{user.Name}({brigadeTypeString})</i> <br />";
            return $"{user.Name}({brigadeTypeString}) <br />";
        }


        private static async Task<string> FillChangedBrigadesDescription(ChangePlaneBrigadeTrain changedBrigade, PlaneBrigadeTrain plannedBrigade, ILogger logger)
        {
            var sqlRUser = new UserRepository(logger);
            var sqlBrigades = new BrigadeRepository(logger);

            
            var user = await sqlRUser.ById(changedBrigade.UserId);
            //else 
            //    user = await sqlRUser.ById(plannedBrigade.UserId);
            //Нехуй слать юзера без бригады
            if (user.BrigadeId == null)
                throw new Microsoft.Rest.ValidationException($"У пользователя {user.Name} неуказана бригада");
            var brigade = await sqlBrigades.ById((int)user.BrigadeId);
            var brigadeTypeString = GetStringBrigadeType(brigade.BrigadeType);
            if (changedBrigade.Droped)
                return $"<i>{user.Name}({brigadeTypeString})</i> <br />";
            return $"{user.Name}({brigadeTypeString}) <br />";
        }


        private async Task AddBrigadesTimelines(DateTime date, int currentPlanedRouteTrainId, SheludeChangedRouteTripsTo toAdd)
        {
            var sqlRPlanedStationOnTrips = new PlanedStationOnTripsRepository(_logger);
            var sqlRPlaneBrigadeTrain = new PlaneBrigadeTrainsRepository(_logger);
            var sqlRChangePlaneBrigadeTrains = new ChangePlaneBrigadeTrainsRepository(_logger);

            var planeBrigadeTrains =
                await sqlRPlaneBrigadeTrain.ByPlanedRouteTrainId(currentPlanedRouteTrainId);
            foreach (var planeBrigadeTrain in planeBrigadeTrains)
            {
                //Нужно получить время когда хуесос содится и слазиет с поезда
                var inputPlaneStation = await sqlRPlanedStationOnTrips.ById(planeBrigadeTrain.StantionStartId);
                var outputPlaneStation = await sqlRPlanedStationOnTrips.ById(planeBrigadeTrain.StantionEndId);

                var description = await FillBrigadesDescription(planeBrigadeTrain, _logger);

                var brigadesToAddInput = new TimeLine
                {
                    StarTime = inputPlaneStation.OutTime.ToFuckingGenaFormat(date),
                    Color = ColorEntryBrigadeToTrain,
                    EnumType = TimelineTypeEnum.TimeBrigade,
                    //Ебаный стыд.
                    Id = inputPlaneStation.Id,
                    Description = "ЛБ",
                    AdditionalTimeLineData = new AdditionalTimeLineData { Description = description }
                };

                //var brigadesToAddOutput = new TimeLine
                //{
                //    StarTime = outputPlaneStation.InTime.ToFuckingGenaFormat(date),
                //    Color = ColorEntryBrigadeToTrain,
                //    EnumType = TimelineTypeEnum.TimeBrigade,
                //    Id = outputPlaneStation.Id,
                //    Description = "Б"
                //};
                toAdd.PlanTimeLines.Add(brigadesToAddInput);
                //toAdd.PlanTimeLines.Add(brigadesToAddOutput);

                //Надо проверить есть ли в измененных данные.
                var changeBrigade = await sqlRChangePlaneBrigadeTrains.ByPlaneBrigadeTrainId(planeBrigadeTrain.Id);
                if (changeBrigade == null)
                {
                    toAdd.ChangeTimeLines.Add(brigadesToAddInput);
                    //toAdd.ChangeTimeLines.Add(brigadesToAddOutput);
                }
                else
                {
                    var inputCPlaneStation = await sqlRPlanedStationOnTrips.ById(changeBrigade.StantionStartId);
                    //var outputCPlaneStation = await sqlRPlanedStationOnTrips.ById(changeBrigades.StantionEndId);
                    //if (changeBrigade.Droped)
                    description = await FillChangedBrigadesDescription(changeBrigade, planeBrigadeTrain, _logger);

                    var brigadesChangedAddInput = new TimeLine
                    {
                        StarTime = inputCPlaneStation.OutTime.ToFuckingGenaFormat(date),
                        Color = ColorEntryBrigadeToTrain,
                        EnumType = TimelineTypeEnum.TimeBrigade,
                        Id = inputPlaneStation.Id,
                        //Description = LegendTimeLine.EntryBrigadeToTrain.Symbol,
                        Description = "ЛБ",
                        AdditionalTimeLineData = new AdditionalTimeLineData { Description = description }
                    };
                    //var brigadesChangedToAddOutput = new TimeLine
                    //{
                    //    StarTime = outputCPlaneStation.InTime.ToFuckingGenaFormat(date),
                    //    Color = ColorEntryBrigadeToTrain,
                    //    EnumType = TimelineTypeEnum.TimeBrigade,
                    //    Id = outputPlaneStation.Id,
                    //    Description = LegendTimeLine.EscapeBrigadeFromTrain.Symbol
                    //};

                    //Если были отменены все сотрудники то TimeLine бригад не показывать на ФАКТЕ (Каждая запись это 1 сотрудник. так что нахуй с пляжа)
                    if (!changeBrigade.Droped)
                        toAdd.ChangeTimeLines.Add(brigadesChangedAddInput);


                    //toAdd.ChangeTimeLines.Add(brigadesChangedToAddOutput);
                }
            }
        }

        private async Task AddTripsTimelines(DateTime date, int currentPlanedRouteTrainId, SheludeChangedRouteTripsTo toAdd, int plannedTrainId)
        {

            var sqlRPlanedStationOnTrips = new PlanedStationOnTripsRepository(_logger);
            var sqlRChangedStationOnTrips = new ChangePlaneStantionOnTripsRepository(_logger);
            var sqlRPlannedRouteTrain = new PlanedRouteTrainsRepository(_logger);
            var sqlRTrips = new TripsRepository(_logger);
            var sqlRTrains = new TrainRepository(_logger);
            var sqlRStations = new StantionsRepository(_logger);

            var planedStationsOnTrips =
                await sqlRPlanedStationOnTrips.ByPlannedRouteTrainId(currentPlanedRouteTrainId);
            var planedTripsIds = planedStationsOnTrips.DistinctBy(e => e.TripId).Select(x => x.TripId).ToList();
            foreach (var planedTripId in planedTripsIds)
            {
                var toAddPlanedTrip = new TimeLine();
                var stationsOnTrip = planedStationsOnTrips.Where(x => x.TripId == planedTripId).ToList();
                stationsOnTrip = stationsOnTrip.OrderBy(e => e.InTime).ToList();
                if (stationsOnTrip.Count > 0)
                {
                    toAddPlanedTrip.StarTime = stationsOnTrip.First().OutTime.ToFuckingGenaFormat(date);
                    toAddPlanedTrip.EndTime = stationsOnTrip.Last().InTime.ToFuckingGenaFormat(date);
                }
                else
                    continue;

                toAddPlanedTrip.Color = ColorTrip;
                toAddPlanedTrip.EnumType = TimelineTypeEnum.TimeRangeTrip;
                var trip = await sqlRTrips.ById(planedTripId);
                //Костыль для перегонного рейса
                if (trip.TripType == TripType.Transfer)
                {
                    toAddPlanedTrip.Color = ColorTransfer;
                    toAddPlanedTrip.EnumType = TimelineTypeEnum.TimeRangeTripTransfer;
                }


                toAddPlanedTrip.Id = trip.Id;
                toAddPlanedTrip.Description = trip.Name;

                toAddPlanedTrip.AdditionalTimeLineData = await FillAdditionalDataPlanned(stationsOnTrip);

                var changedStationList = new List<ChangePlaneStantionOnTrip>();
                var toAddChangedTrip = toAddPlanedTrip.Clone();


                var stationsWithTrainChanged = new List<ChangePlaneStantionOnTrip>();

                foreach (var stationOnTrip in stationsOnTrip)
                {
                    var changedStation = await sqlRChangedStationOnTrips.ByPlaneStantionOnTripId(stationOnTrip.Id);
                    if (changedStation != null)
                    {
                        changedStationList.Add(changedStation);
                        if (changedStation.TrainId.HasValue && plannedTrainId != 0)
                        //&&changedStation.TrainId != plannedTrainId)
                        {
                            //toAdd.Trains.Change.Previous.Add((await sqlRTrains.ById(changedStation.TrainId))?.Name);
                            stationsWithTrainChanged.Add(changedStation);
                        }

                    }
                    else
                    {
                        var clonePlanedStation =
                            _mapper.Map<PlaneStantionOnTrip, ChangePlaneStantionOnTrip>(stationOnTrip);
                        clonePlanedStation.PlaneStantionOnTripId = stationOnTrip.Id;

                        changedStationList.Add(clonePlanedStation);
                    }
                }

                //заполняем таймлайн с измененными
                changedStationList = changedStationList.OrderBy(e => e.InTime).ToList();

                //Тут потихому присунемся и заполним на факте изменение поезда. Ну если есть.
                await AddTrainChange(toAdd, plannedTrainId, sqlRPlanedStationOnTrips, sqlRTrains, sqlRStations, stationsWithTrainChanged);

                var notDropped = new List<ChangePlaneStantionOnTrip>();
                var dropped = new List<ChangePlaneStantionOnTrip>();
                //ChangePlaneStantionOnTrip lastDropped = null;
                //ChangePlaneStantionOnTrip lastNotDropped = null;
                var currentTripChangedTimeLines = new List<TimeLine>();
                var lastIndex = changedStationList.Count - 1;

                toAddChangedTrip.Changed = false;

                for (var index = 0; index < changedStationList.Count; index++)
                {
                    var changeStation = changedStationList[index];
                    var changedStationDb = await sqlRChangedStationOnTrips.ByPlaneStantionOnTripId(changeStation.PlaneStantionOnTripId);

                    if (changedStationDb != null)
                    {
                        var plannedSt = await sqlRPlanedStationOnTrips.ById(changeStation.PlaneStantionOnTripId);
                        var planRoute = await sqlRPlannedRouteTrain.ById(currentPlanedRouteTrainId);
                        if (!(changeStation.InTime == plannedSt.InTime &&
                            changeStation.OutTime == plannedSt.OutTime &&
                            changeStation.Droped == false &&
                            changeStation.TrainId == planRoute.TrainId
                        ))
                            toAddChangedTrip.Changed = true;
                    }

                    if (!changeStation.Droped)
                    {
                        if (dropped.Any())
                        {
                            if (dropped.First().PlaneStantionOnTripId == changedStationList[0].PlaneStantionOnTripId)
                                toAddChangedTrip.StarTime = dropped.First().OutTime.ToFuckingGenaFormat(date);
                            else
                                toAddChangedTrip.StarTime = dropped.First().InTime.ToFuckingGenaFormat(date);
                            //if (index == lastIndex)
                            //    toAddChangedTrip.EndTime = changeStation.InTime.ToFuckingGenaFormat(date);
                            //else
                            if (index != lastIndex)
                                toAddChangedTrip.EndTime = changedStationList[index].OutTime.ToFuckingGenaFormat(date);
                            else
                                toAddChangedTrip.EndTime = changedStationList[index].InTime.ToFuckingGenaFormat(date);
                            toAddChangedTrip.EnumType = TimelineTypeEnum.TimeRangeCancelTrip;
                            toAddChangedTrip.Color = null;

                            toAddChangedTrip.AdditionalTimeLineData = await FillAdditionalDataChanged(stationsOnTrip, dropped);
                            currentTripChangedTimeLines.Add(toAddChangedTrip.Clone());
                            dropped = new List<ChangePlaneStantionOnTrip>();
                            toAddChangedTrip = toAddPlanedTrip.Clone();
                        }

                        notDropped.Add(changeStation);
                        //astNotDropped = changeStation;
                    }

                    if (changeStation.Droped)
                    {
                        if (notDropped.Any())
                        {
                            if (index == lastIndex)
                            {
                                toAddChangedTrip.StarTime = notDropped.First().OutTime.ToFuckingGenaFormat(date);
                                toAddChangedTrip.EndTime = notDropped.Last().InTime.ToFuckingGenaFormat(date);
                                if (notDropped.Count == 1)
                                {
                                    toAddChangedTrip.StarTime = notDropped.First().OutTime.ToFuckingGenaFormat(date);
                                    toAddChangedTrip.EndTime = toAddChangedTrip.StarTime;
                                }
                            }
                            else
                            {
                                toAddChangedTrip.StarTime = notDropped.First().OutTime.ToFuckingGenaFormat(date);
                                toAddChangedTrip.EndTime = changeStation.InTime.ToFuckingGenaFormat(date);
                            }

                            toAddChangedTrip.AdditionalTimeLineData = await FillAdditionalDataChanged(stationsOnTrip, notDropped);
                            currentTripChangedTimeLines.Add(toAddChangedTrip);
                            toAddChangedTrip = toAddPlanedTrip.Clone();
                            notDropped = new List<ChangePlaneStantionOnTrip>();
                        }

                        dropped.Add(changeStation);
                    }
                }



                if (notDropped.Any())
                {
                    if (notDropped.Count == 1)
                    {
                        toAddChangedTrip.StarTime =
                            notDropped.FirstOrDefault().InTime.ToFuckingGenaFormat(date);
                        toAddChangedTrip.EndTime = notDropped.FirstOrDefault().InTime.ToFuckingGenaFormat(date);
                    }
                    else
                    {
                        toAddChangedTrip.StarTime =
                            notDropped.FirstOrDefault().OutTime.ToFuckingGenaFormat(date);
                        toAddChangedTrip.EndTime = notDropped.LastOrDefault().InTime.ToFuckingGenaFormat(date);
                    }
                    toAddChangedTrip.AdditionalTimeLineData = await FillAdditionalDataChanged(stationsOnTrip, notDropped);
                    currentTripChangedTimeLines.Add(toAddChangedTrip);
                    toAddChangedTrip = toAddPlanedTrip.Clone();
                }

                if (dropped.Any())
                {
                    toAddChangedTrip.EnumType = TimelineTypeEnum.TimeRangeCancelTrip;
                    toAddChangedTrip.Color = null;
                    if (currentTripChangedTimeLines.Any(x => x.EnumType == TimelineTypeEnum.TimeRangeTrip || x.EnumType == TimelineTypeEnum.TimeRangeTripTransfer))
                        toAddChangedTrip.StarTime = (DateTime)currentTripChangedTimeLines.Last().EndTime;
                    else
                    if (dropped.First().PlaneStantionOnTripId == changedStationList[0].PlaneStantionOnTripId)
                        toAddChangedTrip.StarTime = dropped.First().OutTime.ToFuckingGenaFormat(date);
                    else
                        toAddChangedTrip.StarTime = dropped.FirstOrDefault().InTime.ToFuckingGenaFormat(date);
                    toAddChangedTrip.EndTime = dropped.LastOrDefault().InTime.ToFuckingGenaFormat(date);
                    toAddChangedTrip.AdditionalTimeLineData = await FillAdditionalDataChanged(stationsOnTrip, dropped);
                    currentTripChangedTimeLines.Add(toAddChangedTrip.Clone());
                }

                var planRoute1 = await sqlRPlannedRouteTrain.ById(currentPlanedRouteTrainId);
                var train = await sqlRTrains.ById(planRoute1.TrainId);

                if (string.IsNullOrEmpty(toAdd.Trains.Change.Current))
                    toAdd.Trains.Change.Current = train.Name;

                toAdd.ChangeTimeLines.AddRange(currentTripChangedTimeLines);

                toAdd.PlanTimeLines.Add(toAddPlanedTrip);
            }
        }

        private static async Task AddTrainChange(SheludeChangedRouteTripsTo toAdd, int plannedTrainId, PlanedStationOnTripsRepository sqlRPlanedStationOnTrips, TrainRepository sqlRTrains, StantionsRepository sqlRStations, List<ChangePlaneStantionOnTrip> stationsWithTrainChanged)
        {
            var currentTrainId = plannedTrainId;

            foreach (var stationWithTrainChanged in stationsWithTrainChanged)
            {
                if (stationWithTrainChanged.TrainId == currentTrainId)
                    continue;
                var plannedStation = await sqlRPlanedStationOnTrips.ById(stationWithTrainChanged.PlaneStantionOnTripId);
                var station = await sqlRStations.ById(plannedStation.StantionId);
                var train = await sqlRTrains.ById(stationWithTrainChanged.TrainId);

                var changedTrain = new TimeLine
                {
                    StarTime = stationWithTrainChanged.OutTime,
                    EndTime = null,
                    Id = stationWithTrainChanged.PlaneStantionOnTripId,
                    EnumType = TimelineTypeEnum.ChangeTrain,
                    Color = null,
                    BorderColor = null,
                    Description = String.Empty,
                    AdditionalTimeLineData = new AdditionalTimeLineData
                    {
                        TripStartStationName = station.Name,
                        TripEndStationName = null,
                        Stantions = null,
                        Description = train.Name
                    }
                };

                currentTrainId = train.Id;

                if (toAdd.Trains.Change.Previous.LastOrDefault() != toAdd.Trains.Change.Current)
                    toAdd.Trains.Change.Previous.Add(toAdd.Trains.Change.Current);
                toAdd.Trains.Change.Current = train.Name;

                //= new Change { Current = string.Empty, Previous = new List<string>() };
                toAdd.ChangeTimeLines.Add(changedTrain);
            }
        }

        private async Task<AdditionalTimeLineData> FillAdditionalDataChanged(List<PlaneStantionOnTrip> stationsOnTrip, List<ChangePlaneStantionOnTrip> changedStations)
        {
            var sqlRStations = new StantionsRepository(_logger);
            var sqlRPlanedStationOnTrips = new PlanedStationOnTripsRepository(_logger);

            var filteredStationsOnTrip = new List<PlaneStantionOnTrip>();
            foreach (var item in changedStations)
            {
                var plannedSt = await sqlRPlanedStationOnTrips.ById(item.PlaneStantionOnTripId);
                //Если в бд какоет гавно
                if (plannedSt == null)
                    continue;
                filteredStationsOnTrip.Add(stationsOnTrip.FirstOrDefault(x => x.StantionId == plannedSt.StantionId));
            }

            var result = new AdditionalTimeLineData
            {
                Stantions = new List<TimeLineStantion>()
            };

            if (filteredStationsOnTrip.Any())
            {
                var startStation = await sqlRStations.ById(filteredStationsOnTrip.First().StantionId);
                var endStation = await sqlRStations.ById(filteredStationsOnTrip.Last().StantionId);
                result = new AdditionalTimeLineData
                {
                    TripStartStationName = startStation.Name,
                    TripEndStationName = endStation.Name,
                    TripStartStationShortName = startStation.ShortName,
                    TripEndStationShortName = endStation.ShortName,
                    Stantions = new List<TimeLineStantion>()
                };

            }

            foreach (var stationOnTrip in filteredStationsOnTrip)
            {
                var toAddSt = new TimeLineStantion();
                var st = await sqlRStations.ById(stationOnTrip.StantionId);
                toAddSt.Name = st.Name;
                toAddSt.Time = stationOnTrip.InTime.ToStringTimeOnly() + "-" +
                               stationOnTrip.OutTime.ToStringTimeOnly();
                //if (stationOnTrip.Id == stationsOnTrip.First().Id)
                //    toAddSt.Time = stationOnTrip.OutTime.ToStringTimeOnly();

                //if (stationOnTrip.Id == stationsOnTrip.Last().Id)
                //    toAddSt.Time = stationOnTrip.InTime.ToStringTimeOnly();

                toAddSt.Name = st.Name;
                result.Stantions.Add(toAddSt);
            }


            return result;
        }

        private async Task<AdditionalTimeLineData> FillAdditionalDataPlanned(List<PlaneStantionOnTrip> stationsOnTrip)
        {
            var sqlRStations = new StantionsRepository(_logger);
            var startStation = await sqlRStations.ById(stationsOnTrip.First().StantionId);
            var endStation = await sqlRStations.ById(stationsOnTrip.Last().StantionId);
            var result = new AdditionalTimeLineData
            {
                TripStartStationName = startStation.Name,
                TripEndStationName = endStation.Name,
                TripStartStationShortName = startStation.ShortName,
                TripEndStationShortName = endStation.ShortName,
                Stantions = new List<TimeLineStantion>()
            };

            foreach (var stationOnTrip in stationsOnTrip)
            {
                var toAddSt = new TimeLineStantion();
                var st = await sqlRStations.ById(stationOnTrip.StantionId);
                toAddSt.Name = st.Name;
                toAddSt.Time = stationOnTrip.InTime.ToStringTimeOnly() + "-" + stationOnTrip.OutTime.ToStringTimeOnly();
                if (stationOnTrip.Id == stationsOnTrip.First().Id)
                    toAddSt.Time = stationOnTrip.OutTime.ToStringTimeOnly();

                if (stationOnTrip.Id == stationsOnTrip.Last().Id)
                    toAddSt.Time = stationOnTrip.InTime.ToStringTimeOnly();

                toAddSt.Name = st.Name;
                result.Stantions.Add(toAddSt);
            }

            return result;
        }

        private async Task AddInspectionTimelines(PlanedRouteTrainDto planTableItem, SheludeChangedRouteTripsTo toAdd, DateTime date)
        {
            var sqlRChangedInspection = new ChangedPlanedInspectionRoutesRepository(_logger);
            var sqlRPlanedInspection = new PlanedInspectionRoutesRepository(_logger);

            var plannedInspections =
                await sqlRPlanedInspection.ByPlanedRouteTrainId(planTableItem.DaysData.First().PlanedRouteTrainId);
            foreach (var plannedInspection in plannedInspections)
            {
                var toAddInspection = new TimeLine
                {
                    StarTime = plannedInspection.Start.ToFuckingGenaFormat(date),
                    EndTime = plannedInspection.End.ToFuckingGenaFormat(date),
                    EnumType = TimelineTypeEnum.TimeRangeTo2,
                    Id = plannedInspection.Id,
                };
                toAdd.PlanTimeLines.Add(toAddInspection);
                if (plannedInspection.CheckListType == CheckListType.TO2)
                {
                    toAddInspection.Description = "TO2";
                    toAddInspection.Color = ColorTo2;
                }

                if (plannedInspection.CheckListType == CheckListType.CTO)
                {
                    toAddInspection.Description = "CTO";
                    toAddInspection.Color = ColorCto;
                }

                //смотрим есть ли для этой няшки изменения
                var changedInspection = await sqlRChangedInspection.ByPlanedInspectionRouteId(plannedInspection.Id);
                if (changedInspection == null)
                {
                    toAdd.ChangeTimeLines.Add(toAddInspection);
                    continue;
                }
                var toAddChangedInspection = new TimeLine
                {
                    StarTime = changedInspection.Start.ToFuckingGenaFormat(date),
                    EndTime = changedInspection.End.ToFuckingGenaFormat(date),
                    Color = toAddInspection.Color,
                    BorderColor = ColorRedBorder,
                    EnumType = TimelineTypeEnum.TimeRangeTo2,
                    Id = plannedInspection.Id,
                    Description = toAddInspection.Description,
                    Changed = true
                };
                if (changedInspection.Start == plannedInspection.Start.ToFuckingGenaFormat(date) &&
                    changedInspection.End == plannedInspection.End.ToFuckingGenaFormat(date))
                    toAddChangedInspection.Changed = false;

                if (changedInspection.Droped)
                    toAddChangedInspection.Color = ColorToDropped;
                if (!changedInspection.Droped)
                    toAdd.ChangeTimeLines.Add(toAddChangedInspection);
            }
        }


        /// <summary>
        /// Непланируемые инспекции и прочее
        /// </summary>
        private static async Task AddNotPlanedInspectionTimeLines(
            PlanedRouteTrainDto planTableItem, SheludeChangedRouteTripsTo toAdd, ILogger logger, DateTime date)
        {
            var sqlRPlanedRouteTrains = new PlanedRouteTrainsRepository(logger);
            var sqlRInspections = new InspectionRepository(logger);

            var planedRouteTrain = await sqlRPlanedRouteTrains.ById(planTableItem.DaysData.First().PlanedRouteTrainId);
            var currentTrainInspections = await sqlRInspections.GetByTrainId(planedRouteTrain.TrainId);
            var currentDayInspections =
                currentTrainInspections.Where(x => x.DateStart.Date.Equals(planedRouteTrain.CreateDate)).ToList();

            foreach (var currentDayInspection in currentDayInspections)
            {
                var description = await FillNotPlannedInspectionDescription(currentDayInspection.Id, currentDayInspection.CheckListType, logger);

                if (currentDayInspection.CheckListType == CheckListType.TO1)
                {
                    //ТО1
                    var toAddInspection = new TimeLine
                    {
                        StarTime = currentDayInspection.DateStart.ToFuckingGenaFormat(date),
                        EndTime = currentDayInspection.DateEnd,
                        Color = ColorTo1,
                        EnumType = TimelineTypeEnum.TimeRangeTo1,
                        Id = currentDayInspection.Id,
                        Description = "ТО-1",
                        AdditionalTimeLineData = new AdditionalTimeLineData { Description = description }
                    };
                    if (toAddInspection.EndTime != null)
                        toAddInspection.EndTime = ((DateTime)toAddInspection.EndTime).ToFuckingGenaFormat(date);
                    toAdd.ChangeTimeLines.Add(toAddInspection);
                }



                if (currentDayInspection.CheckListType == CheckListType.Inspection)
                {
                    var sqlRUser = new UserRepository(logger);
                    var sqlRBrigade = new BrigadeRepository(logger);
                    var user = await sqlRUser.ById(currentDayInspection.UserId);
                    if (user.BrigadeId != null)
                    {
                        //Приемка
                        var toAddInspection = new TimeLine
                        {
                            StarTime = currentDayInspection.DateStart.ToFuckingGenaFormat(date),
                            EndTime = currentDayInspection.DateEnd,
                            Color = ColorPriemka,
                            EnumType = TimelineTypeEnum.Inspection,
                            Id = currentDayInspection.Id,
                        };

                        var brigade = await sqlRBrigade.ById((int)user.BrigadeId);
                        switch (brigade.BrigadeType)
                        {
                            case BrigadeType.Locomotiv:
                                toAddInspection.Description = "ЛБ";
                                toAddInspection.AdditionalTimeLineData = new AdditionalTimeLineData { Description = description };
                                break;
                            case BrigadeType.Receiver:
                                toAddInspection.Description = "ПР";
                                toAddInspection.AdditionalTimeLineData = new AdditionalTimeLineData { Description = description };
                                break;
                        }

                        if (toAddInspection.EndTime != null)
                            toAddInspection.EndTime = ((DateTime)toAddInspection.EndTime).ToFuckingGenaFormat(date);
                        toAdd.ChangeTimeLines.Add(toAddInspection);
                    }
                }


                //Сдача поезда
                if (currentDayInspection.CheckListType == CheckListType.Surrender)
                {
                    var sqlRUser = new UserRepository(logger);
                    var sqlRBrigade = new BrigadeRepository(logger);
                    var user = await sqlRUser.ById(currentDayInspection.UserId);
                    if (user.BrigadeId != null)
                    {
                        //Сдача
                        var toAddInspection = new TimeLine
                        {
                            StarTime = currentDayInspection.DateStart.ToFuckingGenaFormat(date),
                            EndTime = currentDayInspection.DateEnd,
                            Color = ColorSur,
                            EnumType = TimelineTypeEnum.Surrender,
                            Id = currentDayInspection.Id,
                        };

                        var brigade = await sqlRBrigade.ById((int)user.BrigadeId);
                        if (brigade.BrigadeType == BrigadeType.Locomotiv)
                        {
                            toAddInspection.Description = "ЛБ";
                            toAddInspection.AdditionalTimeLineData = new AdditionalTimeLineData { Description = description };
                        }

                        if (brigade.BrigadeType == BrigadeType.Receiver)
                        {
                            toAddInspection.Description = "ПР";
                            toAddInspection.AdditionalTimeLineData = new AdditionalTimeLineData { Description = description };
                        }
                        if (toAddInspection.EndTime != null)
                            toAddInspection.EndTime = ((DateTime)toAddInspection.EndTime).ToFuckingGenaFormat(date);
                        toAdd.ChangeTimeLines.Add(toAddInspection);
                    }
                }

            }
        }



        ///// <summary>
        ///// Вход в депо
        ///// </summary>
        private static async Task AddEnterToDepot(PlanedRouteTrainDto planTableItem, SheludeChangedRouteTripsTo toAdd, DateTime date, ILogger logger)
        {
            var sqlRDepoEvents = new DepoEventsRepository(logger);
            var allTrainEvents = await sqlRDepoEvents.ByTrainId(planTableItem.DaysData.First().Train.Id);
            var currentDateEvents = allTrainEvents.Where(x =>
                x.InTime >= date.AddHours(3) && x.InTime <= date.AddHours(26).AddMinutes(59).AddSeconds(59)).ToList();

            foreach (var currentDateEvent in currentDateEvents)
            {
                var toAddTask = new TimeLine
                {
                    StarTime = currentDateEvent.InTime.ToFuckingGenaFormat(date),
                    Color = ColorEnterToDepot,
                    EnumType = TimelineTypeEnum.EnterToDepot,
                    Id = currentDateEvent.Id,
                    Description = string.Empty,
                };
                toAdd.ChangeTimeLines.Add(toAddTask);
            }
        }

        ///// <summary>
        ///// Постановка на канал. Или посадка на бутылку или еще какаят хуйня. Время постановки на позицию = Постановка на канал
        ///// </summary>
        private static async Task AddDepotParking(PlanedRouteTrainDto planTableItem, SheludeChangedRouteTripsTo toAdd, DateTime date, ILogger logger)
        {
            var sqlRDepoEvents = new DepoEventsRepository(logger);
            var allTrainEvents = await sqlRDepoEvents.ByTrainId(planTableItem.DaysData.First().Train.Id);
            var currentDateEvents = allTrainEvents.Where(x =>
                x.ParkingTime >= date.AddHours(3) && x.ParkingTime <= date.AddHours(26).AddMinutes(59).AddSeconds(59));

            foreach (var currentDateEvent in currentDateEvents)
            {
                if (!currentDateEvent.ParkingTime.HasValue)
                    continue;
                var toAddTask = new TimeLine
                {
                    StarTime = ((DateTime)currentDateEvent.ParkingTime).ToFuckingGenaFormat(date),
                    Color = ColorPakringInDepot,
                    EnumType = TimelineTypeEnum.Channeling,
                    Id = currentDateEvent.Id,
                    Description = string.Empty,
                };
                toAdd.ChangeTimeLines.Add(toAddTask);
            }
        }

        private static async Task AddCriticalTasksTimeLines(PlanedRouteTrainDto planTableItem,
            SheludeChangedRouteTripsTo toAdd, DateTime date, ILogger logger)
        {
            var sqlRTask = new TaskRepository(logger);
            var sqlRPlanedRouteTrains = new PlanedRouteTrainsRepository(logger);

            var planedRouteTrain = await sqlRPlanedRouteTrains.ById(planTableItem.DaysData.First().PlanedRouteTrainId);
            var tasks = await sqlRTask.CriticalTasksByTrainIdAndDate(planedRouteTrain.TrainId, date);



            for (int i = 3; i < 26; i++)
            {
                var i1 = i;
                var filteredTask = tasks.Where(x =>
                    x.AttribUpdateDate.ToFuckingGenaFormat(date) >= DateTime.MinValue.AddHours(i1) && x.AttribUpdateDate.ToFuckingGenaFormat(date) < DateTime.MinValue.AddHours(i1 + 1)).ToList();
                if (!filteredTask.Any())
                    continue;
                var toAddTask = new TimeLine
                {
                    StarTime = DateTime.MinValue.AddHours(i1).AddMinutes(30),
                    EndTime = null,
                    Color = null,
                    EnumType = TimelineTypeEnum.CriticalTask,
                    Id = 666,
                    Description = string.Empty,
                    AdditionalTimeLineData = new AdditionalTimeLineData { Description = $"Количество инцидентов: {filteredTask.Count()}" }
                };
                toAdd.ChangeTimeLines.Add(toAddTask);
            }
        }


        //Круд для ебучих плановых инспекций
        public async Task<ChangedPlanedInspectionRoute> AddOrUpdateChangeInspection(ChangedPlanedInspectionRoute input)
        {
            if (input.PlanedInspectionRouteId == 0)
                throw new ValidationException("PlanedInspectionRouteId 0");

            var sqlRPlaneInspections = new PlanedInspectionRoutesRepository(_logger);

            var planeInspection = await sqlRPlaneInspections.ById(input.PlanedInspectionRouteId);

            if (planeInspection == null)
                throw new ValidationException($"Не существует плановой инспекции с Id {input.PlanedInspectionRouteId}");

            //Ну типа не надо все поля сразу менять
            if (input.Start == DateTime.MinValue)
                input.Start = planeInspection.Start;
            if (input.End == DateTime.MinValue)
                input.End = planeInspection.End;
            if (input.ChangeUserId == 0)
                input.ChangeUserId = 1;

            input.ChangeDate = DateTime.Now;
            input.CheckListType = CheckListType.TO2;

            var sqlRChangedInspections = new ChangedPlanedInspectionRoutesRepository(_logger);

            var checkChangedInspectionExist =
                await sqlRChangedInspections.ByPlanedInspectionRouteId(input.PlanedInspectionRouteId);
            if (checkChangedInspectionExist != null)
                input.Id = checkChangedInspectionExist.Id;

            if (input.Id != 0)
                return await sqlRChangedInspections.Update(input);
            return await sqlRChangedInspections.Add(input);
        }


        //Круд для высадки/посадки долбоящеров
        public async Task<List<ChangePlaneBrigadeTrain>> AddOrUpdateChangeBrigadeOnTrip(List<ChangePlaneBrigadeTrain> entitiesToChange)
        {
            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                var result = new List<ChangePlaneBrigadeTrain>();
                foreach (var input in entitiesToChange)
                {

                    if (input.PlaneBrigadeTrainId == 0)
                        throw new ValidationException("PlaneBrigadeTrainId 0");
                    var sqlRPlaneBrigade = new PlaneBrigadeTrainsRepository(_logger);
                    var planeBrigade = await sqlRPlaneBrigade.ById(input.PlaneBrigadeTrainId);
                    if (planeBrigade == null)
                        throw new ValidationException(
                            $"Не существует плановой бригады с Id {input.PlaneBrigadeTrainId}");

                    //Ну типа не надо все поля сразу менять
                    if (input.StantionStartId == 0)
                        input.StantionStartId = planeBrigade.StantionStartId;
                    if (input.StantionEndId == 0)
                        input.StantionEndId = planeBrigade.StantionEndId;
                    if (input.UserId == 0)
                        input.UserId = planeBrigade.UserId;
                    if (input.ChangeUserId == 0)
                        input.ChangeUserId = 1;

                    input.ChangeDate = DateTime.Now;
                    var sqlRChangedBridages = new ChangePlaneBrigadeTrainsRepository(_logger);
                    var checkChangedBridagesExist =
                        await sqlRChangedBridages.ByPlaneBrigadeTrainId(input.PlaneBrigadeTrainId);
                    if (checkChangedBridagesExist != null)
                        input.Id = checkChangedBridagesExist.Id;

                    input.ChangeUserId = _userId;
                    if (input.Id != 0)
                        result.Add(await sqlRChangedBridages.Update(input));
                    else
                        result.Add(await sqlRChangedBridages.Add(input));
                }
                transaction.Complete();
                return result;
            }
        }

        //Круд для станций ебать их в рот

        public async Task<List<ChangePlaneStantionOnTrip>> AddOrUpdateChangePlaneStationOnTrip(List<ChangePlaneStantionOnTrip> input)
        {
            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                var sqlRdPlanedStations = new PlanedStationOnTripsRepository(_logger);
                var sqlRdPlanedRouteTrains = new PlanedRouteTrainsRepository(_logger);
                var planeStation = await sqlRdPlanedStations.ById(input.First().PlaneStantionOnTripId);
                var planedRoute = await sqlRdPlanedRouteTrains.ById(planeStation.PlanedRouteTrainId);

                var result = new List<ChangePlaneStantionOnTrip>();
                foreach (var item in input)
                {
                    item.InTime = item.InTime.Add(planedRoute.Date - DateTime.MinValue);
                    item.OutTime = item.OutTime.Add(planedRoute.Date - DateTime.MinValue);
                    item.ChangeUserId = _userId;
                    var res = await AddOrUpdateChangePlaneStationOnTrip(item);
                    result.Add(res);
                }

                transaction.Complete();
                return result;
            }
        }

        public async Task<ChangePlaneStantionOnTrip> AddOrUpdateChangePlaneStationOnTrip(
            ChangePlaneStantionOnTrip input)
        {
            var sqlRPlanedStationOnTrips = new PlanedStationOnTripsRepository(_logger);
            var sqlRPlanedRouteTrains = new PlanedRouteTrainsRepository(_logger);
            var sqlRChangedStationOnTrips = new ChangePlaneStantionOnTripsRepository(_logger);
            var sqlRTrains = new TrainRepository(_logger);


            //Берем плановую станку
            var planeStation = await sqlRPlanedStationOnTrips.ById(input.PlaneStantionOnTripId);
            if (planeStation == null)
                throw new ValidationException($"Не существует плановой станции с Id {input.PlaneStantionOnTripId}");

            //Ну типа не надо все поля сразу менять
            if (input.InTime == DateTime.MinValue)
                input.InTime = planeStation.InTime;
            if (input.OutTime == DateTime.MinValue)
                input.OutTime = planeStation.OutTime;
            if (input.TrainId == null)
                input.TrainId = await sqlRTrains.TrainIdByPlaneStationId(planeStation.Id);


            //Проверяем что дата не выходит за границы текущего планового роута
            var planedRoute = await sqlRPlanedRouteTrains.ById(planeStation.PlanedRouteTrainId);
            if (input.InTime.Date != planedRoute.Date || input.OutTime.Date != planedRoute.Date)
                throw new ValidationException($"Временной диапазон {input.InTime} - {input.OutTime} выходит за пределы суток текущего маршрута {planedRoute.Date}");

            //А терь берем нахуй все плановые станки с этого ебучего маршрута
            var planeStations = await sqlRPlanedStationOnTrips.ByPlannedRouteTrainId(planeStation.PlanedRouteTrainId);

            //Сортируем это гавно по времени прибытия
            planeStations = planeStations.OrderBy(x => x.InTime).ToList();

            //Ищем в этом ебучем массиве индекс нашей ахуенной станки
            var planeStationIndex = planeStations.IndexOf(planeStations.First(x => x.InTime == planeStation.InTime && x.OutTime == planeStation.OutTime));
            if (planeStationIndex == -1)
                throw new SystemException("Не удалось найти плановую станцию в массиве станций маршрута");

            DateTime? minInTime = null;
            DateTime? maxOutTime = null;

            //Проверяем что эта сука не 1-я и не последняя.
            if (planeStationIndex == 0)
                minInTime = DateTime.MinValue;
            if (planeStationIndex == planeStations.Count - 1)
                maxOutTime = DateTime.MaxValue;

            //Берем время отхуития со станки перед ней
            if (minInTime == null)
            {
                var q = 1;
                if (planeStations[planeStationIndex - 1].StantionId == planeStation.StantionId)
                    q = 2;
                minInTime = planeStations[planeStationIndex - q].OutTime;
            }

            if (maxOutTime == null)
                //Берем время прихуяривания на станку после нашей ахуитеельной
            {
                var w = 1;
                if (planeStations[planeStationIndex + 1].StantionId == planeStation.StantionId)
                    w = 2;
                maxOutTime = planeStations[planeStationIndex + w].InTime;
            }

            //Думаеш мой молодой прыщавый дружок все? А хуй там. Ищем в изменненых соседей нашей ебучей станки
            //Получаем массивчек изиененных
            var changedStationsList = new List<ChangePlaneStantionOnTrip>();
            foreach (var value in planeStations)
            {
                var changedStation = await sqlRChangedStationOnTrips.ByPlaneStantionOnTripId(value.Id);
                //Проверяем это гавно на нулл или на то что она отменена
                if (changedStation == null || changedStation.Droped)
                    continue;
                //Если это апдейт то нахуй с пляжа ее из массива, а если адд то ее там и небудет) хДД
                if (input.PlaneStantionOnTripId == changedStation.PlaneStantionOnTripId && value.Id == planeStation.Id)
                    continue;
                changedStationsList.Add(changedStation);
            }


            if (changedStationsList.Count > 0)
            {
                //надо проверить что гавно от UI не попадает в измененные станки(Помним что нашей тут нет нихуя)
                foreach (var changedStation in changedStationsList)
                {
                    var ps = await sqlRPlanedStationOnTrips.ById(changedStation.PlaneStantionOnTripId);
                    var psL = await sqlRPlanedStationOnTrips.ByPlannedRouteTrainId(ps.PlanedRouteTrainId);
                    psL = psL.Where(x => x.TripId == ps.TripId).OrderBy(y => y.InTime).ToList();
                    var stt = psL.First(x => x.Id == ps.Id);
                    var index = psL.IndexOf(stt);
                    var lastIndex = psL.Count - 1;

                    if (index == 0)
                        if (!(input.OutTime >= changedStation.OutTime || input.OutTime <= changedStation.OutTime))
                            throw new ValidationException("Временной диапазон пересекается с измененными станциями");

                    if (index == lastIndex)
                        if (!(input.OutTime >= changedStation.OutTime || input.OutTime <= changedStation.OutTime))
                            throw new ValidationException("Временной диапазон пересекается с измененными станциями");

                    if (index != 0 && index != lastIndex)
                        if (!(input.InTime >= changedStation.InTime && input.OutTime >= changedStation.OutTime ||
                              input.InTime <= changedStation.InTime && input.OutTime <= changedStation.OutTime))
                            throw new ValidationException("Временной диапазон пересекается с измененными станциями");
                }
            }

            //Проверяем что оно непопадает в ТО2 или СТО или измененные ТО2 или СТО
            var sqlRPlannedInspectionRoutes = new PlanedInspectionRoutesRepository(_logger);
            var sqlRChangedPlannedInspectionRoutes = new ChangedPlanedInspectionRoutesRepository(_logger);
            var currentRoutePlannedInspection = await sqlRPlannedInspectionRoutes.ByPlanedRouteTrainId(planedRoute.Id);
            if (currentRoutePlannedInspection.ToList().Any())
            {
                foreach (var plannedInspection in currentRoutePlannedInspection)
                {
                    var changed = await sqlRChangedPlannedInspectionRoutes.ByPlanedInspectionRouteId(plannedInspection.Id);
                    if (changed != null)
                    {
                        plannedInspection.Start = changed.Start;
                        plannedInspection.End = changed.End;
                    }

                    if (input.OutTime >= plannedInspection.Start && input.OutTime <= plannedInspection.End)
                        throw new ValidationException($"Временной диапазон пересекается с плановой инспекцией: {plannedInspection.Start.ToStringTimeOnly()} - {plannedInspection.End.ToStringTimeOnly()}");
                }
            }

            //Проверяем то гавно, что прислал Ui и шлем его нахуй
            var ps1 = await sqlRPlanedStationOnTrips.ById(input.PlaneStantionOnTripId);
            var psL1 = await sqlRPlanedStationOnTrips.ByPlannedRouteTrainId(ps1.PlanedRouteTrainId);
            psL1 = psL1.Where(x => x.TripId == ps1.TripId).OrderBy(y => y.InTime).ToList();
            var stt1 = psL1.First(x => x.Id == ps1.Id);
            var index1 = psL1.IndexOf(stt1);
            var lastIndex1 = psL1.Count - 1;

            if (index1 == 0)
                if (input.OutTime >= maxOutTime)
                    throw new ValidationException("Временной диапазон пересекается с остальными станциями");

            if (index1 == lastIndex1)
                if (input.InTime <= minInTime)
                    throw new ValidationException("Временной диапазон пересекается с остальными станциями");

            if (index1 != 0 && index1 != lastIndex1)
                if (input.InTime <= minInTime || input.OutTime >= maxOutTime)
                    throw new ValidationException("Временной диапазон пересекается с остальными станциями");

            //если внезапно все то говно отработало нормально то таки добавим нашу ахуенную станку
            var changeStation = await sqlRChangedStationOnTrips.ByPlaneStantionOnTripId(input.PlaneStantionOnTripId);
            if (changeStation != null)
            {
                input.Id = changeStation.Id;
                if (input.TrainId == 0)
                    input.TrainId = changeStation.TrainId;
                return await sqlRChangedStationOnTrips.Update(input);
            }

            if (input.TrainId == 0)
                input.TrainId = await sqlRTrains.TrainIdByPlaneStationId(planeStation.Id);
            return await sqlRChangedStationOnTrips.Add(input);
        }

        public async Task<RouteData> GetRouteInformation(int planedRouteTrainId)
        {
            var sqlRPlanedStations = new PlanedStationOnTripsRepository(_logger);

            var sqlRPlanedRouteTrains = new PlanedRouteTrainsRepository(_logger);


            var planedRouteTrain = await sqlRPlanedRouteTrains.ById(planedRouteTrainId);

            var result = new RouteData();
            result.PlaneRoute = new PlaneRouteData { TripsWithStations = new List<PlanedTripWithStations>() };

            //Берем данные по плановым.
            var planeStations = await sqlRPlanedStations.ByPlannedRouteTrainId(planedRouteTrainId);

            var tripIds = planeStations.Select(x => x.TripId).Distinct();
            result.PlaneRoute.TripsWithStations = new List<PlanedTripWithStations>();
            foreach (var tripId in tripIds)
            {
                //Станции с группировкой по трипам
                await GetPlaneStationsData(result, planeStations, tripId, _logger);

                //Плановые юзерки
                await GetPlanedBrigadesData(planedRouteTrainId, result, _logger);

                //Инфа по маршруту
                await GetRouteData(planedRouteTrain, result, _logger);
            }


            return result;
        }


        /// <summary>
        /// данные по маршруту
        /// </summary>
        private static async Task GetRouteData(PlanedRouteTrain planedRouteTrain, RouteData result, ILogger logger)
        {
            var sqlRRoutes = new RoutesRepository(logger);
            var sqlRTrains = new TrainRepository(logger);
            var sqlRTurnovers = new TurnoversRepoisitory(logger);


            var route = await sqlRRoutes.ById(planedRouteTrain.RouteId);
            result.PlaneRoute.PlanedRouteId = planedRouteTrain.Id;
            result.PlaneRoute.RouteName = route.Name;
            result.PlaneRoute.Mileage = route.Mileage;
            result.PlaneRoute.TurnoverName = (await sqlRTurnovers.ById((int)route.TurnoverId))?.Name;
            result.PlaneRoute.PlanedTrainId = planedRouteTrain.TrainId;
            result.PlaneRoute.PlanedTrainName = (await sqlRTrains.ById(planedRouteTrain.TrainId))?.Name;
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

        public class ChangedTripWithStations
        {
            //public int TripName { get; set; }
            //public string TripName { get; set; }
        }


        public class SheludeChangedRouteTripsTo
        {
            public double Mileage { get; set; }
            public string RouteName { get; set; }
            public int PlanedRouteId { get; set; }
            //Какаят генина хуйня
            //В корне маршрута необходимо добавить параметры:
            public Trains Trains { get; set; }
            public List<TimeLine> PlanTimeLines { get; set; }
            public List<TimeLine> ChangeTimeLines { get; set; }
        }

        //Какаят генина хуйня
        public class Change
        {
            //          "current": "ЭП2Д-010", - текущий поезд по факту
            public string Current { get; set; }
            //          "previous": ["ЭП2Д-008"] – измененные поезда
            public List<string> Previous { get; set; }
        }

        //Какаят генина хуйня
        public class Trains
        {
            //        "planed": "ЭП2Д-008", - поезд по плану
            public string Planed { get; set; }
            // - по факту
            public Change Change { get; set; }
        }

        public class TimeLine
        {
            public DateTime StarTime { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public DateTime? EndTime { get; set; }
            public int Id { get; set; }
            public TimelineTypeEnum EnumType { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Color { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string BorderColor { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Description { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public AdditionalTimeLineData AdditionalTimeLineData { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool Changed { get; set; }
        }

        public class ChangedRoutesWithTimelinePaging
        {
            public List<SheludeChangedRouteTripsTo> Data { get; set; }
            public int Total { get; set; }
        }


        public class ChangeRouteTrainDto : PlanedRouteTrainDto
        {
            public List<ChangedDayData> ChangedDaysData { get; set; }
        }

        public class ChangedDayData
        {
            public DateTime Date { get; set; }
            public string DateString { get; set; }
            public Train Train { get; set; }
            public List<User> Users { get; set; }
        }

        public class LegendEntry
        {
            public TimelineTypeEnum Type { get; set; }
            public string Description { get; set; }
            public string Color { get; set; }
            public string Symbol { get; set; }


        }

        public static class LegendTimeLine
        {
            public static LegendEntry To1 = new LegendEntry { Type = TimelineTypeEnum.TimeRangeTo2, Description = "ТО1", Color = ColorTo2 };
            public static LegendEntry To2 = new LegendEntry { Type = TimelineTypeEnum.TimeRangeTo2, Description = "ТО2", Color = ColorTo2 };
            public static LegendEntry Trip = new LegendEntry { Type = TimelineTypeEnum.TimeRangeTrip, Description = "Рейс", Color = ColorTrip };
            public static LegendEntry EntryBrigadeToTrain = new LegendEntry { Type = TimelineTypeEnum.CriticalTask, Description = "Заступление бригады", Color = ColorEntryBrigadeToTrain, Symbol = "⎏" };
            public static LegendEntry EscapeBrigadeFromTrain = new LegendEntry { Type = TimelineTypeEnum.CriticalTask, Description = "Сход бригады", Color = ColorEscapeBrigadeFromTrain, Symbol = "⎐" };
            public static LegendEntry CriticalIncident = new LegendEntry { Type = TimelineTypeEnum.CriticalTask, Description = "Критический Инцидент", Color = ColorCriticalTask, Symbol = "💩" };
        }

    }
}