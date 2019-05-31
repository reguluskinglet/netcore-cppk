using AutoMapper;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using static Rzdppk.Api.ScheludePlanedDtos;

namespace Rzdppk.Core.Services
{
    public class ScheludePlanedService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ScheludePlanedService(ILogger logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<List<PlanedRouteTrainDto>> PlanedRouteTrainsTable(DateTime startTime, DateTime EndTime)
        {
            var sqlRPlanedRouteTrains = new PlanedRouteTrainsRepository(_logger);
            var sqlRRoutes = new RoutesRepository(_logger);
            var sqlRTrains = new TrainRepository(_logger);
            var sqlRUsers = new UserRepository(_logger);
            var sqlRDayOfROutes = new DayOfRoutesRepoisitory(_logger);
            var sqlRStations = new StantionsRepository(_logger);
            var sqlRPlaneStationsOnTrips = new PlanedStationOnTripsRepository(_logger);
            var sqlRPlaneBrigadeTrains = new PlaneBrigadeTrainsRepository(_logger);
            var planedRouteTrains = await sqlRPlanedRouteTrains.GetAll();
            planedRouteTrains = planedRouteTrains.Where(x => x.Date >= startTime && x.Date < EndTime).ToList();
            //var result = new List<PlanedRouteTrainDto>();

            var dictionary = new Dictionary<int, PlanedRouteTrainDto>();
            //var routeDictionary = planedRouteTrains.ToDictionary(e => e.RouteId);

            //Собственно надо набить словарик доступными роутами
            var routes = await sqlRRoutes.GetAll(0, Int32.MaxValue, null);
            routes.Data = routes.Data.Where(x => x.TurnoverId != null).ToList();

            foreach (var route in routes.Data)
            {
                var planedRouteTrainDto = new PlanedRouteTrainDto
                {
                    Route = route,
                    RouteDays = new List<DayOfWeek>(),
                    DaysData = new List<DayData>()
                };


                if (route.TurnoverId != null)
                {
                    var daysOfRoutes = await sqlRDayOfROutes.DaysByTurnoverId((int) route.TurnoverId);
                    var days = daysOfRoutes.Select(x => x.Day);
                    planedRouteTrainDto.RouteDays.AddRange(days);
                    var proccesDate = startTime;

                    while (proccesDate <= EndTime)
                    {
                        var currentDay = proccesDate.DayOfWeek;
                        if (days.Contains(currentDay))
                            planedRouteTrainDto.DaysData.Add(new DayData
                                {Date = proccesDate.Date, DateString = proccesDate.Date.ToString()});
                        proccesDate = proccesDate.AddDays(1);
                    }
                }

                if (dictionary.ContainsKey(route.Id))
                    dictionary[route.Id].DaysData.AddRange(planedRouteTrainDto.DaysData);
                else
                    dictionary.Add(route.Id, planedRouteTrainDto);
            }

            foreach (var planedRouteTrain in planedRouteTrains)
            {
                //достаем роут
                var route = await sqlRRoutes.ById(planedRouteTrain.RouteId);
                //var currentRouteItems = planedRouteTrains.Where(e => e.RouteId == route.Id);
                var planedRouteTrainDto = new PlanedRouteTrainDto
                {
                    Route = route,
                    DaysData = new List<DayData>()
                };

                var toAdd = new DayData
                {
                    Train = await sqlRTrains.ById(planedRouteTrain.TrainId),
                    PlanedRouteTrainId = planedRouteTrain.Id,
                    Date = planedRouteTrain.Date
                };
                toAdd.DateString = toAdd.Date.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var planeBrigadeTrains = await sqlRPlaneBrigadeTrains.ByPlanedRouteTrainId(planedRouteTrain.Id);

                toAdd.Users = new List<DaysUser>();
                foreach (var planeBrigadeTrain in planeBrigadeTrains)
                {
                    var user = await sqlRUsers.ById(planeBrigadeTrain.UserId);
                    var userToAdd = new DaysUser
                        {UserId = user.Id, Name = user.Name, PlaneBrigadeTrainsId = planeBrigadeTrain.Id};
                    userToAdd.UserStations = new UserStations();
                    var planedStationInput = await sqlRPlaneStationsOnTrips.ById(planeBrigadeTrain.StantionStartId);

                    userToAdd.UserStations.InputTime = planedStationInput.OutTime;
                    userToAdd.UserStations.InputName = (await sqlRStations.ById(planedStationInput.StantionId)).Name;

                    var planedStationOutput = await sqlRPlaneStationsOnTrips.ById(planeBrigadeTrain.StantionEndId);
                    userToAdd.UserStations.OutputTime = planedStationOutput.InTime;
                    userToAdd.UserStations.OutputName = (await sqlRStations.ById(planedStationOutput.StantionId)).Name;
                    //userToAdd.UserStations.InputName = (await sqlRStations.ById(planeBrigadeTrain.StantionStartId)).Name;
                    //userToAdd.UserStations.OutputName = (await sqlRStations.ById(planeBrigadeTrain.StantionEndId)).Name;
                    toAdd.Users.Add(userToAdd);
                }

                planedRouteTrainDto.DaysData.Add(toAdd);


                if (dictionary.ContainsKey(route.Id))
                {
                    //надо взять текущие деньки роута и проверить что такого еще нет. кстати если его нет надо нахуй послать, а если есть заменить.
                    var currentDays = dictionary[route.Id].DaysData;
                    var day = currentDays.FirstOrDefault(x =>
                        x.Date.Date.Equals(planedRouteTrainDto.DaysData.First().Date.Date));
                    if (day == null)
                    {
                        dictionary[route.Id].DaysData.AddRange(planedRouteTrainDto.DaysData);
                        continue;
                    }

                    //throw new ValidationException("На этот день нельзя добавить поезд");
                    var index = dictionary[route.Id].DaysData.IndexOf(day);
                    dictionary[route.Id].DaysData[index] = planedRouteTrainDto.DaysData.First();
                    //dictionary[route.Id].DaysData.AddRange(planedRouteTrainDto.DaysData);
                }
                else
                    dictionary.Add(route.Id, planedRouteTrainDto);
            }


            //var train = await sqlRTrains.ByIdWithStations(planedRouteTrain.TrainId);

            var result = new List<PlanedRouteTrainDto>();
            result.AddRange(dictionary.Values);

            return result;
        }

        public class PlanedRouteTrainDto
        {
            //public int PlanedRouteTrainsId { get; set; }
            public Route Route { get; set; }
            public List<DayOfWeek> RouteDays { get; set; }
            public List<DayData> DaysData { get; set; }
        }

        public class DaysUser
        {
            public int UserId { get; set; }
            public string Name { get; set; }
            public int PlaneBrigadeTrainsId { get; set; }
            public UserStations UserStations { get; set; }
        }

        public class UserStations
        {
            public string InputName { get; set; }
            public string OutputName { get; set; }
            public DateTime InputTime { get; set; }
            public DateTime OutputTime { get; set; }
        }

        public class DayData
        {
            public DateTime Date { get; set; }
            public string DateString { get; set; }
            public Train Train { get; set; }
            public List<DaysUser> Users { get; set; }
            public int PlanedRouteTrainId { get; set; }
        }


        public async Task<PlanedRouteTrain> AddTrainToPlanedRouteTrains(PlanedRouteTrain input)
        {
            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                var sqlRTrips = new TripsRepository(_logger);
                var sqlRStations = new StantionOnTripsRepository(_logger);
                var sqlRInspectionOnRoutes = new InspectionRoutesRepository(_logger);
                var sqlRPlanedInspectionOnRoutes = new PlanedInspectionRoutesRepository(_logger);

                var sqlRPlaneStations = new PlanedStationOnTripsRepository(_logger);
                var sqlRPlanedRouteTrains = new PlanedRouteTrainsRepository(_logger);

                //Валидация Хуяция.
                //TODO поменять на sql
                var trainPlaneRoutes = await sqlRPlanedRouteTrains.ByTrainId(input.TrainId);
                if (trainPlaneRoutes.Any(x => x.Date.Date.Equals(input.Date.Date)))
                    throw new ValidationException(Error.AlreadyAdd);

                var result = await sqlRPlanedRouteTrains.Add(input);

                var trips = await sqlRTrips.GetTripsByRouteId(input.RouteId);
                //var tripsStationsDictionary = new Dictionary<Trip, List<StantionOnTrip>>();
                foreach (var trip in trips)
                {
                    var stations = await sqlRStations.ByTripId(trip.Id);
                    foreach (var station in stations)
                    {
                        var toAdd = _mapper.Map<StantionOnTrip, PlaneStantionOnTrip>(station);
                        toAdd.PlanedRouteTrainId = result.Id;
                        var newInTime = toAdd.InTime - DateTime.MinValue;
                        toAdd.InTime = input.Date.Date.Add(newInTime);

                        var newOutTime = toAdd.OutTime - DateTime.MinValue;
                        toAdd.OutTime = input.Date.Date.Add(newOutTime);

                        await sqlRPlaneStations.Add(toAdd);
                    }

                    //tripsStationsDictionary.Add(trip, stations);
                }

                //И тут я узнал что надо инспекции скопировать ебать их в рот
                var inspections = await sqlRInspectionOnRoutes.GetByRouteId(input.RouteId);
                foreach (var inspection in inspections)
                {
                    var toAdd = new PlanedInspectionRoute
                    {
                        CheckListType = inspection.CheckListType,
                        PlanedRouteTrainId = result.Id,
                    };
                    var newInTime = inspection.Start - inspection.Start.Date;
                    toAdd.Start = input.Date.Date.Add(newInTime);
                    var newOutTime = inspection.End - inspection.End.Date;
                    toAdd.End = input.Date.Date.Add(newOutTime);
                    await sqlRPlanedInspectionOnRoutes.Add(toAdd);
                }


                transaction.Complete();
                return result;
            }
        }

        public async Task DeleteFromPlanedRouteTrains(int planedRouteTrainsId)
        {
            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                var sqlRPlanedInspectionOnRoutes = new PlanedInspectionRoutesRepository(_logger);
                var sqlRPlaneStations = new PlanedStationOnTripsRepository(_logger);
                var sqlRPlaneBrigades = new PlaneBrigadeTrainsRepository(_logger);
                var sqlRPlanedRouteTrains = new PlanedRouteTrainsRepository(_logger);

                var sqlRChangedInspections = new ChangedPlanedInspectionRoutesRepository(_logger);
                var sqlRChangedBrigades = new ChangePlaneBrigadeTrainsRepository(_logger);
                var sqlRChangedStations = new ChangePlaneStantionOnTripsRepository(_logger);

                //ебаем нахуй инспекции
                var planedInspections = await sqlRPlanedInspectionOnRoutes.ByPlanedRouteTrainId(planedRouteTrainsId);
                foreach (var planedInspection in planedInspections)
                {
                    //Блядь чистим изменненые к хуям.
                    var changed = await sqlRChangedInspections.ByPlanedInspectionRouteId(planedInspection.Id);
                    if (changed != null)
                        await sqlRChangedInspections.Delete(changed.Id);
                    await sqlRPlanedInspectionOnRoutes.Delete(planedInspection.Id);
                }

                //Ебаем юзеров к хуям
                var planeBrigades = await sqlRPlaneBrigades.ByPlanedRouteTrainId(planedRouteTrainsId);
                foreach (var planeBrigade in planeBrigades)
                {
                    //Блядь чистим изменненые к хуям.
                    var changed = await sqlRChangedBrigades.ByPlaneBrigadeTrainId(planeBrigade.Id);
                    if (changed != null)
                        await sqlRChangedBrigades.Delete(changed.Id);
                    await sqlRPlaneBrigades.Delete(planeBrigade.Id);
                }

                //ебаем станки
                var planeStations = await sqlRPlaneStations.ByPlannedRouteTrainId(planedRouteTrainsId);
                foreach (var planeStation in planeStations)
                {
                    //Блядь чистим изменненые к хуям.
                    var changed = await sqlRChangedStations.ByPlaneStantionOnTripId(planeStation.Id);
                    if (changed != null)
                        await sqlRChangedStations.Delete(changed.Id);
                    await sqlRPlaneStations.Delete(planeStation.Id);
                }


                //Ну и главную хуету нахуй
                await sqlRPlanedRouteTrains.Delete(planedRouteTrainsId);
                transaction.Complete();
            }
        }

        public async Task<List<Stantion>> GetInputStation(int planedRouteTrainId, DateTime day, int userId)
        {
            var result = await GetStationsFromPlanedRouteByIdAndDay(planedRouteTrainId, day, userId);
            if (result.Count == 0 || result.Count == 1)
                throw new Exception(Error.NoStationsAvailable);
            //return new List<Stantion>();
            //if (result.Count == 1)
            //    return result;
            var stations = result.GetRange(0, result.Count - 1);
            stations = stations.OrderBy(y => y.InTime).ToList();
            return stations.Select(x => _mapper.Map<StantionQq, Stantion>(x)).ToList();
        }

        public async Task<List<Stantion>> GetOutputStation(int planedRouteTrainId, int? inputStationId, DateTime day,
            int userId)
        {
            var result = await GetStationsFromPlanedRouteByIdAndDay(planedRouteTrainId, day, userId, inputStationId);
            if (result == null)
                throw new Exception("Не удалось найти станции на маршруте за указанную дату");

            var inputStationOnList = result.FirstOrDefault(e => e.Id == inputStationId);
            var index = result.IndexOf(inputStationOnList);
            if (index == -1)
                throw new Exception(Error.NoStationsAvailable);
            if (index == result.Count - 1)
                return new List<Stantion>();
            if (index + 1 == result.Count - 1)
                return new List<Stantion> { _mapper.Map<StantionQq, Stantion>(result.Last())};
            var stations = result.GetRange(index + 1, result.Count - 1 - index);
            stations = stations.OrderBy(y => y.InTime).ToList();
            return stations.Select(x => _mapper.Map<StantionQq, Stantion>(x)).ToList();
        }

        private async Task<List<StantionQq>> GetStationsFromPlanedRouteByIdAndDay(int planedRouteTrainId, DateTime day,
            int userId, int? inputStationId = 0)
        {
            var result = new List<Stantion>();
            var sqlRStations = new StantionsRepository(_logger);
            var sqlRPlanedStOnTrips = new PlanedStationOnTripsRepository(_logger);
            var sqlRTrip = new TripsRepository(_logger);
            if (userId == 0)
                throw new ValidationException(Error.NotFilledOptionalField);

            PlaneStantionOnTrip inputSt = null;
            if (inputStationId != 0)
                inputSt = await sqlRPlanedStOnTrips.ById(inputStationId.Value);

            var currentDayPlaneBrigadeTrain = await sqlRPlanedStOnTrips.ByUserIdAndTimeRange(userId,
                day.Date.AddHours(3), day.Date.AddDays(1).AddMilliseconds(-1));

            //busyPlanedSt = busyPlanedSt.Where(x => x.PlanedRouteTrainId != planedRouteTrainId).ToList();
            //var curPlanedRoute = await sqlPlanedBr.ByPlannedRouteTrainId(planedRouteTrainId);
            var currentPlanedRSt = await sqlRPlanedStOnTrips.ByPlannedRouteTrainId(planedRouteTrainId);
            currentPlanedRSt = currentPlanedRSt.OrderBy(x => x.OutTime).ToList();

            //var curPlanedRouteBr = await sqlPlanedBr.ByPlannedRouteTrainId(planedRouteTrainId);
            var usedPlaneSt = new List<PlaneStantionOnTrip>();
            if (currentPlanedRSt.Any())
            {
                foreach (var item in currentDayPlaneBrigadeTrain)
                {
                    //все станки маршрута, на который назначена данная запись
                    var planSts = (await sqlRPlanedStOnTrips.ByPlannedRouteTrainId(item.PlanedRouteTrainId))
                        .OrderBy(x => x.OutTime).ToList();
                    var start = planSts.IndexOf(planSts.First(x => x.Id == item.StantionStartId)) + 1;
                    var end = planSts.IndexOf(planSts.First(x => x.Id == item.StantionEndId)) + 1;
                    usedPlaneSt.AddRange(planSts.GetRange(start, end - start));
                }
            }

            usedPlaneSt = usedPlaneSt.DistinctBy(x => x.Id).ToList();
            DateTime? minUsedTime = null; 
            DateTime? maxUsedTime = null;
            if (usedPlaneSt.Count > 1)
            {
                minUsedTime = usedPlaneSt.First().OutTime;
                maxUsedTime = usedPlaneSt.Last().InTime;
            }
            if (usedPlaneSt.Count == 1)
            {
                minUsedTime = usedPlaneSt.First().OutTime;
                maxUsedTime = usedPlaneSt.First().InTime;
            }

            if (minUsedTime != null && maxUsedTime != null)
            {
                if (inputSt != null)
                {
                    if (inputSt.OutTime <= minUsedTime)
                        maxUsedTime = ((DateTime) maxUsedTime).AddDays(1);
                    if (inputSt.OutTime >= maxUsedTime)
                        minUsedTime = ((DateTime)minUsedTime).AddDays(-1);
                }
            }

            var res1 = new List<StantionQq>();

            var currentTripId = 0;
            for (var index = 0; index < currentPlanedRSt.Count; index++)
            {
                var item = currentPlanedRSt[index];
                var trip = await sqlRTrip.ById(item.TripId);


                if (minUsedTime != null && maxUsedTime != null)
                {
                    if (item.InTime >= minUsedTime && item.InTime <= maxUsedTime)
                        continue;
                    if (item.OutTime >= minUsedTime && item.OutTime <= maxUsedTime)
                        continue;
                }

                //foreach (var x in usedPlaneSt)
                //{
                //    if (item.InTime >= x.InTime && item.InTime <= x.OutTime)
                //        isUsed = true;
                //    if (item.OutTime >= x.InTime && item.OutTime <= x.OutTime)
                //        isUsed = true;
                //}
                //if (isUsed)
                //    continue;
                    var station = await sqlRStations.ById(item.StantionId);
                station.Id = item.Id;
                //station.Name += $" ({item.InTime.ToStringTimeOnly()})";
                var sqq = _mapper.Map<Stantion, StantionQq>(station);
                sqq.InTime = item.InTime;
                sqq.TripName = trip.Name;

                var qqStart = item.InTime.ToStringTimeOnly();
                var qqEnd = item.OutTime.ToStringTimeOnly();
                if (currentTripId == 0 || item.TripId != currentTripId)
                    qqStart = "н/д";
                if (index != currentPlanedRSt.Count - 1)
                {
                    if (currentPlanedRSt[index + 1].TripId != item.TripId && currentTripId != 0)
                        qqEnd = "н/д";
                }
                else
                {
                    qqEnd = "н/д";
                }

                var qq1 = $"{station.Name} {qqStart}-{qqEnd} ({trip.Name})";
                sqq.Name += $" ({qq1})";
                res1.Add(sqq);
                currentTripId = item.TripId;
                //result.Add(station);
            }

            return res1;
        }

        public class StantionQq : Stantion
        {
            public string TripName { get; set; }
            public DateTime InTime { get; set; }
        }

        /// <summary>
        /// Обновить Хуету
        /// </summary>
        public async Task<List<PlaneBrigadeTrain>> AddUserToTrain(PlaneBrigadeTrainDto input)
        {
            ////Начинаем блядь квн
            //using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            //{
            var sqlR = new PlaneBrigadeTrainsRepository(_logger);
            var sqlRPlanedStations = new PlanedStationOnTripsRepository(_logger);

            var result = new List<PlaneBrigadeTrain>();

            foreach (var userId in input.UserIds)
            {
                var data = await sqlR.ByPlanedRouteTrainId(input.PlanedRouteTrainId);
                //Если добавлен на этот плановый рейс уже
                if (data.Any(x => x.UserId == userId)) throw new ValidationException(Error.AlreadyAdd);

                var requestPlanedStations =
                    await sqlRPlanedStations.ByPlannedRouteTrainId(input.PlanedRouteTrainId);
                if (requestPlanedStations.Count < 2) throw new ValidationException(Error.MiniTripStations);

                //TODO вынести маппер отдельно
                var entity = new PlaneBrigadeTrain
                {
                    StantionStartId = input.StantionStartId,
                    StantionEndId = input.StantionEndId,
                    UserId = userId,
                    PlanedRouteTrainId = input.PlanedRouteTrainId
                };

                result.Add(await sqlR.Add(entity));
            }

            //transaction.Complete();
            return result;
            //}
        }
    }
}