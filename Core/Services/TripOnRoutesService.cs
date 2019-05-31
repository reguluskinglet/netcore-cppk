using AutoMapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Error = Rzdppk.Core.Other.Error;


namespace Rzdppk.Core.Services
{
    public class TripOnRoutesService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public TripOnRoutesService(ILogger logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<TripOnRouteWithStationsDto> TripWithStationsByRouteIdAndTripId(int routeId, int tripId)
        {
            var sqlRTripsOnRoute = new TripsOnRouteRepository(_logger);
            var sqlRTrips = new TripsRepository(_logger);
            var sqlRStationOnTrip = new StantionOnTripsRepository(_logger);
            var sqlRStation = new StantionsRepository(_logger);
            var result = new TripOnRouteWithStationsDto();

            var tripOnRoute = await sqlRTripsOnRoute.ByRouteIdAndTripId(routeId, tripId);
            if (tripOnRoute == null)
                throw new ValidationException("Элемент не найден");
            result.Id = tripOnRoute.Id;
            result.RouteId = tripOnRoute.Id;
            result.TripId = tripId;

            //result.TripWithDateTimeStations = new TripWithDateTimeStations();
            result.TripWithDateTimeStations = _mapper.Map<Trip, TripWithDateTimeStations>(await sqlRTrips.ById(tripId));
            var stations = await sqlRStationOnTrip.ByTripId(tripId);
            result.TripWithDateTimeStations.StantionOnTripsWithStringTime = new List<StationOnTripWithStringDateTime>();
            foreach (var stantion in stations)
            {
                var toAdd = _mapper.Map<StantionOnTrip, StationOnTripWithStringDateTime>(stantion);
                toAdd.InTimeString = stantion.InTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                toAdd.OutTimeString = stantion.OutTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                toAdd.Stantion = await sqlRStation.ById(toAdd.StantionId);
                result.TripWithDateTimeStations.StantionOnTripsWithStringTime.Add(toAdd);
            }

            return result;
        }

        /// <summary>
        /// Обновить Хуету
        /// </summary>
        public async Task<TripOnRouteWithStationsDto> UpdateTripOnRoute(TripOnRouteWithStationsDto input)
        {
            await CompareDaysFromTurnover(input);
            //Начинаем транзакцию
            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                var sqlRStationOnTrip = new StantionOnTripsRepository(_logger);
                var sqlRDaysOfTrip = new DayOfTripsRepoisitory(_logger);
                //var sqlRStationOnTripNoTransaction = new StantionOnTripsRepository(_logger);
                //Удаляем лишние станции нахуй, если есть
                var stations = await sqlRStationOnTrip.ByTripId(input.TripWithDateTimeStations.Id);
                foreach (var station in stations)
                {
                    if (input.TripWithDateTimeStations.StantionOnTripsWithStringTime.Any(e => e.Id == station.Id))
                        continue;
                    sqlRStationOnTrip.Delete(station.Id);
                }

                var currentDaysOfTrip = await sqlRDaysOfTrip.DaysByTripId(input.TripId);
                var currentDays = currentDaysOfTrip.Select(x => x.Day).ToList();
                foreach (var day in input.Days)
                {
                    if (currentDays.Contains(day))
                    {
                        await sqlRDaysOfTrip.Add(new DayOfTrip { TripId = input.TripId, Day = day });
                        currentDays.Remove(day);
                    }
                }
                if (currentDays.Count > 0)
                    foreach (var currentDay in currentDays)
                    {
                        var toRemove = currentDaysOfTrip.FirstOrDefault(x => x.Day.Equals(currentDay));
                        await sqlRDaysOfTrip.Delete(toRemove.Id);
                    }

                var sqlRTripOnRoute = new TripsOnRouteRepository(_logger);

                var newTripOnRoute =
                    await sqlRTripOnRoute.Update(_mapper.Map<TripOnRouteWithStationsDto, TripOnRoute>(input));
                var result = _mapper.Map<TripOnRoute, TripOnRouteWithStationsDto>(newTripOnRoute);
                result.TripWithDateTimeStations = new TripWithDateTimeStations
                {
                    StantionOnTripsWithStringTime = new List<StationOnTripWithStringDateTime>()
                };

                foreach (var item in input.TripWithDateTimeStations.StantionOnTripsWithStringTime)
                {
                    var stationOnTrip =
                        await sqlRStationOnTrip.Update(
                            _mapper.Map<StationOnTripWithStringDateTime, StantionOnTrip>(item));
                    result.TripWithDateTimeStations.StantionOnTripsWithStringTime.Add(
                        _mapper.Map<StantionOnTrip, StationOnTripWithStringDateTime>(item));
                }

                transaction.Complete();
                return result;
            }
        }


        /// <summary>
        /// Добавить хуету
        /// </summary>
        public async Task<TripOnRouteWithStationsDto> AddTripOnRoute(TripOnRouteWithStationsDto input)
        {
            if (input.TripWithDateTimeStations.Id != 0)
                if (!await CompareDaysFromTurnover(input))
                    throw new ValidationException("Не совпадают дни графика оборота и рейса");

            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {

                //var sqlRTripOnRoute = new TripsOnRouteRepository(_logger);
                var sqlRTrip = new TripsRepository(_logger);
                var sqlRStationOnTrip = new StantionOnTripsRepository(_logger);
                var sqlRDaysOfTrip = new DayOfTripsRepoisitory(_logger);
                var all = await sqlRTrip.GetAll();
                if (input.TripWithDateTimeStations.Id == 0 &&
                    all.Any(x => x.Name.Equals(input.TripWithDateTimeStations.Name)))
                    throw new ValidationException(Error.AlreadyAddWithThisName);
                var result = new TripOnRouteWithStationsDto
                {
                    Days = new List<DayOfWeek>(),
                    TripWithDateTimeStations = new TripWithDateTimeStations
                    {
                        StantionOnTrips = new List<StantionOnTrip>(),
                        StantionOnTripsWithStringTime = new List<StationOnTripWithStringDateTime>()
                    }
                };

                if (input.TripWithDateTimeStations.Id == 0)
                {
                    var newTrip =
                        await sqlRTrip.Add(_mapper.Map<TripWithDateTimeStations, Trip>(input.TripWithDateTimeStations));
                    result.TripWithDateTimeStations.Id = newTrip.Id;
                    result.TripWithDateTimeStations.Name = newTrip.Name;
                    result.TripWithDateTimeStations.Description = newTrip.Description;
                    result.TripId = newTrip.Id;
                }

                foreach (var day in input.Days)
                {
                    var addedDay = await sqlRDaysOfTrip.Add(new DayOfTrip { TripId = result.TripId, Day = day });
                    result.Days.Add(addedDay.Day);
                }

                foreach (var item in input.TripWithDateTimeStations.StantionOnTripsWithStringTime)
                {
                    var toAdd = _mapper.Map<StationOnTripWithStringDateTime, StantionOnTrip>(item);
                    if (toAdd.InTime > toAdd.OutTime)
                        throw new ValidationException(Error.IncorrectTimeRange);
                    toAdd.TripId = result.TripWithDateTimeStations.Id;
                    var stationOnTrip = await sqlRStationOnTrip.Add(toAdd);
                    result.TripWithDateTimeStations.StantionOnTripsWithStringTime.Add(
                        _mapper.Map<StantionOnTrip, StationOnTripWithStringDateTime>(stationOnTrip));
                }

                //await CheckTimelineUsing(input.RouteId, input.TripId);

                transaction.Complete();
                return result;
            }
        }

        private async Task<bool> CompareDaysFromTurnover(int routeId, int tripId)
        {

            var sqlRDaysOnTrip = new DayOfTripsRepoisitory(_logger);
            var daysOfTrip = await sqlRDaysOnTrip.DaysByTripId(tripId);
            var wc = new TripOnRouteWithStationsDto { RouteId = routeId, Days = new List<DayOfWeek>() };
            wc.Days.AddRange(daysOfTrip.Select(x => x.Day));

            return await CompareDaysFromTurnover(wc);


        }

        private async Task<bool> CompareDaysFromTurnover(TripOnRouteWithStationsDto input)
        {

            var sqlRRoute = new RoutesRepository(_logger);
            var route = await sqlRRoute.ById(input.RouteId);
            if (route.TurnoverId == null)
                throw new ValidationException("У данного маршрута отсутсвует цикловой график");
            var sqlRTurnover = new TurnoversRepoisitory(_logger);
            var turnover = await sqlRTurnover.ById((int)route.TurnoverId);
            var sqlRDaysOnRoute = new DayOfRoutesRepoisitory(_logger);
            var daysOfRouteFromTurnover = await sqlRDaysOnRoute.DaysByTurnoverId(turnover.Id);
            var daysFromTurnover = daysOfRouteFromTurnover.Select(x => x.Day).ToList();
            daysFromTurnover.Sort();
            input.Days.Sort();

            var isgoodTrip = true;
            foreach (var d in daysFromTurnover)
            {
                if (!input.Days.Any(x => x.Equals(d)))
                    isgoodTrip = false;
            }
            if (isgoodTrip) return true;
            return false;



        }

        public async Task<List<TripOnRoute>> AddExistingTripsOnRouteToRoute(int routeId, List<int> tripIds)
        {
            //Начинаем блядь КВН
            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                var result = new List<TripOnRoute>();
                foreach (var tripId in tripIds)
                {
                    if (!await CompareDaysFromTurnover(routeId, tripId))
                        throw new ValidationException("Не совпадают дни графика оборота и рейса");

                    var sqlRTripOnRoutes = new TripsOnRouteRepository(_logger);
                    if (await sqlRTripOnRoutes.ByRouteIdAndTripId(routeId, tripId) != null)
                        throw new ValidationException("Данный рейс уже добавлен на этот маршрут");

                    await CheckTimelineUsing(routeId, tripId);

                    var res = await sqlRTripOnRoutes.Add(new TripOnRoute { RouteId = routeId, TripId = tripId });
                    result.Add(res);
                }
                transaction.Complete();
                return result;
            }
        }

        private async Task CheckTimelineUsing(int routeId, int tripId)
        {
            var sqlRStationsOnTrip = new StantionOnTripsRepository(_logger);
            //Берем станки трипа для добавления
            var tripToAddStations = await sqlRStationsOnTrip.ByTripId(tripId);

            tripToAddStations = tripToAddStations.OrderBy(x => x.UpdateDate).ToList();
            var tripToAddStartTime = tripToAddStations.First().OutTime;
            var tripToAddEndTime = tripToAddStations.Last().InTime;
            if (tripToAddStartTime > tripToAddEndTime)
            {
                tripToAddEndTime = tripToAddEndTime.AddDays(1);
            }
            var service = new ScheduleCycleService(_logger, _mapper);
            var sqlRoute = new RoutesRepository(_logger);
            var route = await sqlRoute.ById(routeId);
            if (route?.TurnoverId == null)
                throw Error.CommonError;
            var routesWithTimelinePaging = await service.GetRoutesWithTimeline((int)route.TurnoverId, 0, int.MaxValue, route.Id);
            var timeLines = routesWithTimelinePaging.Data.First().TimeLines;

            foreach (var timeline in timeLines)
            {
                if ((tripToAddStartTime >= timeline.StarTime || tripToAddEndTime >= timeline.StarTime) &&
                    (tripToAddStartTime <= timeline.EndTime || tripToAddEndTime <= timeline.EndTime))
                    throw new ValidationException(Error.UsedTimeRange);
            }

            //var trips = (await sqlRTripOnRoutes.ByRouteId(routeId)).Where(x => x.TripId != tripId);
            //foreach (var trip in trips)
            //{
            //    var stations = await sqlRStations.ByTripId(trip.TripId);
            //    stations = stations.OrderBy(x => x.InTime).ToList();

            //    var tripStartTime = stations.First().InTime;
            //    var tripEndTime = stations.Last().OutTime;

            //    if (!(tripToAddStartTime < tripStartTime && tripToAddEndTime < tripStartTime || tripToAddStartTime > tripEndTime && tripToAddEndTime > tripEndTime))
            //        throw new ValidationException("Пересекается время рейса с текущими рейсами на маршруте");
            //}
        }

        public async Task RemoveTripsOnRouteFromRoute(int tripOnRouteId)
        {
            var sqlRTripOnRoutes = new TripsOnRouteRepository(_logger);
            await sqlRTripOnRoutes.Delete(tripOnRouteId);
        }

        public class TripOnRouteWithStationsDto : TripOnRoute
        {
            public TripWithDateTimeStations TripWithDateTimeStations { get; set; }
            public List<DayOfWeek> Days { get; set; }
            public List<int> TripIds { get; set; }
        }


        public class TripWithDateTimeStations : Trip
        {
            public List<StationOnTripWithStringDateTime> StantionOnTripsWithStringTime { get; set; }
        }

        public class StationOnTripWithStringDateTime : StantionOnTrip
        {
            public string InTimeString { get; set; }
            public string OutTimeString { get; set; }
        }

    }
}