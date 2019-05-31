using AutoMapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using Newtonsoft.Json;
using Rzdppk.Model.Enums;
using static Rzdppk.Core.Other.DevExtremeTableData;

namespace Rzdppk.Core.Services
{
    public class ScheduleCycleService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ScheduleCycleService(ILogger logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<InspectionRoute> AddOrUpdateInspectionOnRoute(InspectionRoute input)
        {
            var sqlRoute = new RoutesRepository(_logger);
            var route = await sqlRoute.ById(input.RouteId);
            if (route?.TurnoverId == null)
                throw Error.CommonError;
            var routesWithTimelinePaging = await GetRoutesWithTimeline((int)route.TurnoverId, 0, int.MaxValue, route.Id);
            var timeLines = routesWithTimelinePaging.Data.First().TimeLines;

            foreach (var timeline in timeLines)
            {
                if ((input.Start >= timeline.StarTime || input.End >= timeline.StarTime) &&
                    (input.Start <= timeline.EndTime || input.End <= timeline.EndTime))
                    throw new ValidationException(Error.UsedTimeRange);
            }

            var sqlR = new InspectionRoutesRepository(_logger);
            if (input.Id != 0)
                return await sqlR.Update(input);
            return await sqlR.Add(input);
        }


        public async Task<TurnoversWithDaysPaging> GetTurnoversWithDays(int skip, int limit)
        {
            var sqlR = new TurnoversRepoisitory(_logger);
            var turnovers = await sqlR.GetAll(skip, limit, null);

            var result = new TurnoversWithDaysPaging { Data = new List<TurnoverWithDays>(), Total = turnovers.Data.Count };
            var sqlRDays = new DayOfRoutesRepoisitory(_logger);
            foreach (var turnover in turnovers.Data)
            {
                result.Data.Add(new TurnoverWithDays
                {
                    Name = turnover.Name,
                    DirectionId = turnover.DirectionId,
                    Id = turnover.Id,
                    Days = (await sqlRDays.DaysByTurnoverId(turnover.Id)).Select(x => x.Day).ToList()
                });
            }

            return result;

        }




        #region UiClasses

        public class TurnoverWithDays : Turnover
        {
            public List<DayOfWeek> Days { get; set; }
        }

        public class TurnoversWithDaysPaging
        {
            public List<TurnoverWithDays> Data { get; set; }
            public int Total { get; set; }
        }

        #endregion



        public async Task<RoutesWithTripsAndToPaging> GetRoutesWithTripsAndToByTurnoverId(int turnoverId, int skip, int limit, int? routeId = null)
        {
            var sqlRRoute = new RoutesRepository(_logger);
            var sqlRTripsOnRoute = new TripsOnRouteRepository(_logger);
            var sqlRTrips = new TripsRepository(_logger);
            var sqlRStationOnTrips = new StantionOnTripsRepository(_logger);
            var sqlRInspectionRoutes = new InspectionRoutesRepository(_logger);

            var routes = new List<Route>();
            var total = 0;
            if (routeId == null)
            {
                var paging = await sqlRRoute.GetByTurnoverIdPaging(turnoverId, skip, limit);
                routes.AddRange(paging.Data);
                total = paging.Total;
            }
            else
                routes.Add(await sqlRRoute.ById(routeId.Value));

            var result = new RoutesWithTripsAndToPaging { Data = new List<RoutesWithTripsAndTo>(), Total = total};


            foreach (var route in routes)
            {
                var data = new RoutesWithTripsAndTo
                {
                    Id = route.Id,
                    Name = route.Name,
                    Mileage = route.Mileage,
                    Description = route.Description,
                    TurnoverId = route.TurnoverId,
                    InspectionRoutes = await sqlRInspectionRoutes.GetByRouteId(route.Id)

                };
                var tripsOnRoute = await sqlRTripsOnRoute.ByRouteId(route.Id);
                var trips = new List<TripWithTripOnRouteId>();
                foreach (var tripOnRoute in tripsOnRoute)
                {
                    var trip = await sqlRTrips.ById(tripOnRoute.TripId);
                    var toAdd = _mapper.Map<Trip, TripWithTripOnRouteId>(trip);
                    toAdd.TripOnRouteId = tripOnRoute.Id;
                    trips.Add(toAdd);
                }

                data.Trips = trips;


                foreach (var trip in data.Trips)
                {
                    trip.StantionOnTrips = new List<StantionOnTrip>();
                    var stationsOnTrip = await sqlRStationOnTrips.ByTripId(trip.Id);
                    foreach (var stationOnTrip in stationsOnTrip)
                    {
                        trip.StantionOnTrips.Add(stationOnTrip);
                    }

                }

                result.Data.Add(data);
            }

            return result;
        }


        public class RoutesWithTripsAndTo : Route
        {

            public List<InspectionRoute> InspectionRoutes { get; set; }
            public List<TripWithTripOnRouteId> Trips { get; set; }
        }

        public class TripWithTripOnRouteId : Trip
        {
            public int TripOnRouteId { get; set; }
        }

        public class RoutesWithTripsAndToPaging
        {

            public List<RoutesWithTripsAndTo> Data { get; set; }
            public int Total { get; set; }
        }

        /// <summary>
        /// Отдает маршруты с временными промежутками то и рейсов для ебучего расписания
        /// </summary>
        public async Task<RoutesWithTimelinePaging> GetRoutesWithTimeline(int turnoverId, int skip, int limit, int? routeId = null)
        {
            RoutesWithTripsAndToPaging data;
            if (routeId != null)
                data = await GetRoutesWithTripsAndToByTurnoverId(turnoverId, skip, limit, routeId);
            else
                data = await GetRoutesWithTripsAndToByTurnoverId(turnoverId, skip, limit);

            var result = new RoutesWithTimelinePaging { Total = data.Total, Data = new List<ScheduleRouteTripsTo>() };

            foreach (var item in data.Data.OrderBy(x=>int.Parse("0"+Regex.Match(x.Name,"(\\d+)").Value)))
            {
                var toAdd = new ScheduleRouteTripsTo();
                toAdd.RouteId = item.Id;
                toAdd.Mileage = item.Mileage;
                toAdd.RouteName = item.Name;
                toAdd.TimeLines = await TimelineFromTrips(item.Trips);
                var timelineFromInspectionRoutes = TimelineFromInspectionRoutes(item.InspectionRoutes);
                if (timelineFromInspectionRoutes.Count > 0)
                    toAdd.TimeLines.AddRange(timelineFromInspectionRoutes);
                result.Data.Add(toAdd);
            }

            var sqlRTurnover = new TurnoversRepoisitory(_logger);
            var sqlRDirection = new DirectionsRepository(_logger);
            var sqlRDayOfRoutes = new DayOfRoutesRepoisitory(_logger);

            var turnover = await sqlRTurnover.ById(turnoverId);

            result.TurnoverName = turnover.Name;
            var direction = await sqlRDirection.ById(turnover.DirectionId);
            result.DirectionName = direction?.Name;

            var turnoversDays = await sqlRDayOfRoutes.DaysByTurnoverId(turnoverId);
            result.Days = turnoversDays.Select(x => x.Day).ToList();

            return result;
        }

        private async Task<List<TimeLine>> TimelineFromTrips(List<TripWithTripOnRouteId> trips)
        {
            var sqlRStations = new StantionsRepository(_logger);
            var result = new List<TimeLine>();
            if (trips.Count == 0)
                return result;
            foreach (var trip in trips)
            {
                var toAdd = new TimeLine();
                if (trip.TripOnRouteId != 0)
                    toAdd.TripOnRouteId = trip.TripOnRouteId;
                var stationsOnTrips = trip.StantionOnTrips.ToList();
                stationsOnTrips = stationsOnTrips.OrderBy(e => e.UpdateDate).ToList();
                if (stationsOnTrips.Count > 1)
                {
                    var startStationOnTrip = stationsOnTrips.First();
                    var endStationOnTrip = stationsOnTrips.Last();
                    var startStation = await sqlRStations.ById(startStationOnTrip.StantionId);
                    var endStation = await sqlRStations.ById(endStationOnTrip.StantionId);
                    toAdd.AdditionalTimeLineData = new AdditionalTimeLineData
                    {
                        TripStartStationName = startStation.Name,
                        TripStartStationShortName = startStation.ShortName,
                        TripEndStationName = endStation.Name,
                        TripEndStationShortName = startStation.ShortName
                    };
                    toAdd.StarTime = startStationOnTrip.OutTime;//.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    toAdd.EndTime = endStationOnTrip.InTime; //.ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
                else
                    continue;

                toAdd.Color = trip.TripType == TripType.Transfer
                                  ? ScheduleColors.ColorTransfer
                                  : ScheduleColors.ColorTrip;
                toAdd.EnumType = trip.TripType == TripType.Transfer
                                     ? TimelineTypeEnum.TimeRangeTripTransfer
                                     : TimelineTypeEnum.TimeRangeTrip;
                toAdd.Id = trip.Id;
                toAdd.Description = trip.Name;
                result.Add(toAdd);
            }

            return result;
        }

        private List<TimeLine> TimelineFromInspectionRoutes(List<InspectionRoute> inspectionsRoute)
        {
            var result = new List<TimeLine>();
            if (inspectionsRoute.Count == 0)
                return result;
            foreach (var inspectionRoute in inspectionsRoute)
            {
                if (inspectionRoute?.CheckListType != null)
                {
                    var toAdd = new TimeLine();
                    toAdd.StarTime = inspectionRoute.Start; //.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    toAdd.EndTime = inspectionRoute.End; //.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    toAdd.Id = inspectionRoute.Id;
                    toAdd.InspectionRouteId = inspectionRoute.Id;
                    if (inspectionRoute.CheckListType == CheckListType.CTO)

                    {
                        toAdd.EnumType = TimelineTypeEnum.Cto;
                        toAdd.Color = ScheduleColors.ColorCto;
                        toAdd.Description = "CTO";
                        result.Add(toAdd);
                    }

                    if (inspectionRoute.CheckListType == CheckListType.TO2)
                    {
                        toAdd.EnumType = TimelineTypeEnum.TimeRangeTo2;
                        toAdd.Color = ScheduleColors.ColorTo2;
                        toAdd.Description = "TO2";
                        result.Add(toAdd);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Добавить тарновера с днями в цикловой график
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<TurnoverWithDays> AddTurnoverWithDays(TurnoverWithDays input)
        {
            //Начинаем блядь КВН
            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                var sqlRTurnovers = new TurnoversRepoisitory(_logger);
                var sqlRDays = new DayOfRoutesRepoisitory(_logger);

                var turnover = await sqlRTurnovers.Add(_mapper.Map<TurnoverWithDays, Turnover>(input));
                var result = _mapper.Map<Turnover, TurnoverWithDays>(turnover);
                result.Days = new List<DayOfWeek>();
                foreach (var inputDay in input.Days)
                {
                    var day = await sqlRDays.Add(new DayOfRoute { TurnoverId = result.Id, Day = inputDay });
                    result.Days.Add(day.Day);
                }
                transaction.Complete();
                return result;
            }
        }

        public async Task<TurnoverWithDays> UpdateTurnoverWithDays(TurnoverWithDays input)
        {
            //Начинаем блядь КВН
            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                var sqlRTurnovers = new TurnoversRepoisitory(_logger);
                var sqlRDays = new DayOfRoutesRepoisitory(_logger);

                var turnover = await sqlRTurnovers.Update(_mapper.Map<TurnoverWithDays, Turnover>(input));
                var result = _mapper.Map<Turnover, TurnoverWithDays>(turnover);
                var currentDays = await sqlRDays.DaysByTurnoverId(input.Id);

                result.Days = new List<DayOfWeek>();
                foreach (var inputDay in input.Days)
                {
                    if (currentDays.Any(x => x.Day.Equals(inputDay)))
                    {
                        result.Days.Add(inputDay);
                        continue;
                    }
                    var day = await sqlRDays.Add(new DayOfRoute { TurnoverId = input.Id, Day = inputDay });
                    result.Days.Add(day.Day);
                }

                foreach (var currentDay in currentDays)
                {
                    if (!input.Days.Any(x => x.Equals(currentDay.Day)))
                        await sqlRDays.Delete(currentDay.Id);
                }

                transaction.Complete();
                return result;
            }
        }

        public async Task DeleteTurnoverWithDays(int id)
        {
            ////Начинаем блядь КВН
            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                var sqlRTurnovers = new TurnoversRepoisitory(_logger);
                var sqlRDays = new DayOfRoutesRepoisitory(_logger);
                var currentDays = await sqlRDays.DaysByTurnoverId(id);
                foreach (var currentDay in currentDays)
                {
                    await sqlRDays.Delete(currentDay.Id);
                }
                await sqlRTurnovers.Delete(id);
                transaction.Complete();
            }
        }

        public async Task<List<TripWithStartEndTimeAndDays>> GetTripsByTurnoverIdAndDays(int turnoverId, int routeId)
        {
            var sqlRTripOnRoute = new TripsOnRouteRepository(_logger);
            var sqlRRoute = new RoutesRepository(_logger);
            var sqlRDaysOfRoutes = new DayOfRoutesRepoisitory(_logger);
            var sqlRTrips = new TripsRepository(_logger);
            var sqlRStationOnTrips = new StantionOnTripsRepository(_logger);
            var sqlRStation = new StantionsRepository(_logger);
            var sqlRDayOfTrips = new DayOfTripsRepoisitory(_logger);
            //Получаем дни циклового графика
            var daysTurnoverObj = await sqlRDaysOfRoutes.DaysByTurnoverId(turnoverId);
            var daysTurnover = daysTurnoverObj.Select(x => x.Day).ToList();
            daysTurnover.Sort();
            //Надо найти трипсы с аналогичными днями.
            var trips = await sqlRTrips.GetAll();
            var result = new List<TripWithStartEndTimeAndDays>();


            var route = await sqlRRoute.ById(routeId);

            foreach (var trip in trips)
            {
                if (await sqlRTripOnRoute.ByRouteIdAndTripId(route.Id, trip.Id) != null)
                    continue;

                var currentTripDaysObj = await sqlRDayOfTrips.DaysByTripId(trip.Id);
                var currentTripDays = currentTripDaysObj.Select(x => x.Day).ToList();
                currentTripDays.Sort();

                var isGoodTrip = true;
                foreach (var d in daysTurnover)
                {
                    if (!currentTripDays.Any(x => x.Equals(d)))
                        isGoodTrip = false;
                }
                if (!isGoodTrip) continue;
                {
                    string days = null;
                    foreach (var day in currentTripDays)
                    {
                        if (days == null)
                            days = GetStringDayOfWeekShort(day);
                        else
                            days = days + ", " + GetStringDayOfWeekShort(day);
                    }
                    var tripStations = await sqlRStationOnTrips.ByTripId(trip.Id);
                    tripStations = tripStations.OrderBy(x => x.OutTime).ToList();
                    var startStation = tripStations.First();
                    var endStation = tripStations.Last();

                    var tripExtended = _mapper.Map<Trip, TripWithStartEndTimeAndDays>(trip);
                    tripExtended.Days = currentTripDays;
                    tripExtended.StartTime = startStation.OutTime.ToStringTimeOnly();
                    tripExtended.EndTime = endStation.InTime.ToStringTimeOnly();
                    tripExtended.StartStationName = (await sqlRStation.ById(startStation.StantionId)).Name;
                    tripExtended.EndStationName = (await sqlRStation.ById(endStation.StantionId)).Name;
                    result.Add(tripExtended);
                }
            }

            return result;
        }

        public class TripWithStartEndTimeAndDays : Trip
        {
            public string StartStationName { get; set;}
            public string EndStationName { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public List<DayOfWeek> Days { get; set; }

        }




        public class ScheduleRouteTripsTo
        {
            public double Mileage { get; set; }
            public string RouteName { get; set; }
            public int RouteId { get; set; }
            public List<TimeLine> TimeLines { get; set; }
        }

        public class TimeLine
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public DateTime StarTime { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public DateTime EndTime { get; set; }
            public int Id { get; set; }
            public TimelineTypeEnum EnumType { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Color { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string BorderColor { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Description { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public int TripOnRouteId { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public int InspectionRouteId { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public AdditionalTimeLineData AdditionalTimeLineData { get; set; }
        }

        public class AdditionalTimeLineData
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string TripStartStationName { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string TripStartStationShortName { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string TripEndStationName { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string TripEndStationShortName { get; set; }

            //Очередная поебня
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public List<TimeLineStantion> Stantions { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Description { get; set;}
        }

        public class TimeLineStantion
        {
            public string Name { get; set; }
            public string Time { get; set; }
            public bool Canseled { get; set; }
        }

        public class TripOnRouteWithDaysAndTurnoverId : TripOnRoute
        {
            public List<DayOfWeek> Days { get; set; }
            public int TurnoverId { get; set; }
        }


        public class RoutesWithTimelinePaging
        {
            public List<ScheduleRouteTripsTo> Data { get; set; }
            public int Total { get; set; }
            public string DirectionName { get; set; }
            public string TurnoverName { get; set; }
            public List<DayOfWeek> Days { get; set; }
        }

        //Чо выводить на таймлайне. диапазон или точку
        public enum TimelineTypeEnum
        {
            TimeRangeTo2 = 1,
            TimeRangeTrip = 2,
            CriticalTask = 3,
            Cto = 4,
            //ТО-1
            TimeRangeTo1 = 5,
            //Приемка поезда
            Inspection = 6,
            //Сдача поезда
            Surrender = 7,
            //Постановка на канал
            Channeling = 8,
            //Вход в Депо
            EnterToDepot = 9,
            //Посадка высадка бригады
            TimeBrigade = 10,
            //Перегонный рейс
            TimeRangeTripTransfer = 11,
            //Отмена Рейса
            TimeRangeCancelTrip = 12,
            //Изменен поезд
            ChangeTrain = 13
        }

        public static class ScheduleColors
        {
            public static string ColorTo2 = "#feae16";
            public static string ColorTo1 = "#ca9f25";
            public static string ColorToDropped = "#A600A6";
            public static string ColorToNotPlaned = "#BD0000";
            public static string ColorTrip = "#90caed";
            public static string ColorTransfer = "#ff00b1";
            public static string ColorRedBorder = "#FF1B00";
            public static string ColorEntryBrigadeToTrain = "#DD94E0";
            public static string ColorEscapeBrigadeFromTrain = "#0C04F1";
            public static string ColorCriticalTask = "#DD94E0";
            public static string ColorPakringInDepot = "#21e614";
            public static string ColorEnterToDepot = "#000";

            public static string ColorCto = "#ffec00";
            public static string ColorPriemka = "#8BC34A";
            public static string ColorSur = "#CDDC39";

        }


    }
}