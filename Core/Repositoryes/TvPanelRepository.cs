using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NPOI.SS.Formula.Functions;
using Rzdmonitors.Util;
using RzdMonitors.Util;
using Rzdppk.Core.GridModels.Journals;
using Rzdppk.Core.Helpers;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Dto;
using Rzdppk.Model.Enums;
using Rzdppk.Model.Raspisanie;
using Rzdppk.Model.TV;

namespace Rzdppk.Core.Repositoryes
{
    public class TvPanelRepository : ITvPanelRepository
    {
        private readonly IDb _db;

        public TvPanelRepository(IDb db)
        {
            _db = db;
        }

        #region Box/Panels registration
        public async Task<int> RegisterBox(TvBoxRegisterDto box)
        {
            try
            {
                _db.Transaction.BeginTransaction();

                const string sql = "INSERT INTO [TvBoxes]([Name]) VALUES(@Name) SELECT SCOPE_IDENTITY()";
                const string sqli = "INSERT INTO [TvPanels]([TvBoxId],[Number],[ScreenType]) VALUES(@TvBoxId,@Number,0)";

                var id = await _db.Connection.QuerySingleAsync<int>(sql, new { Name = box.Name }, _db.Transaction.Transaction);

                foreach (var panelId in box.PanelIds)
                {
                    await _db.Connection.ExecuteAsync(sqli, new { TvBoxId = id, Number = panelId }, _db.Transaction.Transaction);
                }

                _db.Transaction.CommitTransaction();

                return id;
            }
            catch (Exception)
            {
                _db.Transaction.RollBackTransaction();
                throw new Exception("db error");
            }
        }

        public async Task<List<TvPanelsDto>> GetBoxPanels(int boxId)
        {
            const string sql = "SELECT * FROM [TvPanels] WHERE [TvBoxId]=@TvBoxId ORDER BY [Number] ASC,[ScreenType] ASC";

            var list = (await _db.Connection.QueryAsync<TVPanel>(sql, new {TvBoxId = boxId}));

            var ret = list.GroupBy(o => o.Number)
                .Select(o => new TvPanelsDto {Num = o.Key, Types = o.Select(k => k.ScreenType).ToArray()}).ToList();

            //return list.Select(o => new TvPanelDto{Id = o.Number, Type = o.ScreenType}).ToList();
            return ret;
        }

        public async Task AddBoxPanels(TvBoxPanelsDto dto)
        {
            try
            {
                const string sql = "INSERT INTO [TvPanels]([TvBoxId],[Number],[ScreenType]) VALUES(@TvBoxId,@Number,0)";

                _db.Transaction.BeginTransaction();

                foreach (var panelId in dto.PanelIds)
                {
                    await _db.Connection.ExecuteAsync(sql, new { TvBoxId = dto.BoxId, Number = panelId }, _db.Transaction.Transaction);
                }

                _db.Transaction.CommitTransaction();
            }
            catch (Exception)
            {
                _db.Transaction.RollBackTransaction();
                throw new Exception("db error");
            }
        }

        public async Task DeleteBoxPanels(TvBoxPanelsDto dto)
        {
            try
            {
                const string sql = "DELETE FROM [TvPanels] WHERE [TvBoxId]=@TvBoxId AND [Number] IN @Ids";

                await _db.Connection.ExecuteAsync(sql, new { TvBoxId = dto.BoxId, Ids = dto.PanelIds }, _db.Transaction.Transaction);
            }
            catch (Exception)
            {
                throw new Exception("db error");
            }
        }
        #endregion

        public async Task<string> GetDepoName(int depoStantionId)
        {
            const string sql = "SELECT Name FROM [Stantions] WHERE [Id]=@Id AND StantionType=0";

            var name = (await _db.Connection.QueryFirstAsync<string>(sql, new { Id = depoStantionId }));

            return name;
        }

        public async Task<ScheduleDeviationTableDto> GetScheduleDeviationTable()
        {
            var ret = new ScheduleDeviationTableDto();

            var now = DateTime.Now;// - TimeSpan.FromDays(3);
            var dateFrom = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0, now.Kind);
            var dateTo = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999, now.Kind);

            var result = (await GetScheduleDeviationsInternal(dateFrom, dateTo)).Where(o => o.IsDropped || o.DiffInTime.Ticks != 0).OrderBy(o => o, new ScheduleDeviationInternalComparer()).ToList();

            ret.GraphViolationCount = result.Count;
            ret.TrainDepoCount = await GetTrainInDepoCount();
            ret.TrainInTripCount = await GetTrainInTripCount();

            var items = new List<ScheduleDeviationTableItemDto>();
            foreach (var item in result) //.Take(9)
            {
                var item2 = new ScheduleDeviationTableItemDto
                {
                    TrainName = item.Train.Name,
                    StationName = item.Stantion.Name,
                    TripNumber = item.Trip.Name,
                    //
                    PlanTime = item.PlannedInTime.ToString("t") + " - " + item.PlannedOutTime.ToString("t"),
                    FactTime = item.IsDropped ? "отменен" : item.FactInTime.ToString("t") + " - " + item.FactOutTime.ToString("t"),
                    //
                    //ArriveTimePlan = item.PlannedInTime.ToString("t"),
                    //DepartureTimePlan = item.PlannedOutTime.ToString("t"),
                    //ArriveDeviation = FormatTimeSpan(item.DiffInTime, item.IsDropped),
                    //DepartureDeviation = FormatTimeSpan(item.DiffOutTime, item.IsDropped),
                    StatusColor = item.Color
                };
                items.Add(item2);
            }

            ret.Items = items.ToArray();

            return ret;
        }

        public async Task<ScheduleDeviationGraphDto> GetScheduleDeviationGraphData(DateTime start, DateTime end)
        {
            var ret = new ScheduleDeviationGraphDto();

            var now = DateTime.Now;
            var dateFrom = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0, now.Kind);
            var dateTo = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999, now.Kind);

            //trips
            const string sql = @"select * from
            PlaneStantionOnTrips ps
            left join Stantions s on s.Id = ps.StantionId
            left join Trips trip on trip.Id = ps.TripId
            left join ChangePlaneStantionOnTrips cs on cs.PlaneStantionOnTripId = ps.Id
            left join PlanedRouteTrains pt on pt.Id = ps.PlanedRouteTrainId
            left join Trains train on pt.TrainId = train.Id
            left join Routes route on route.id = pt.RouteId
            where pt.[Date] BETWEEN @DateFrom AND @DateTo";

            var resultTrips = (await _db.Connection.QueryAsync<PlaneStantionOnTrip, Stantion, Trip, ChangePlaneStantionOnTrip, PlanedRouteTrain, Train, Route, ScheduleDeviationStantionsInternal>(
                sql,
                (plannedStantion, stantion, trip, changePlannedStantion, plannedTrain, train, route) =>
                {
                    var res = new ScheduleDeviationStantionsInternal
                    {
                        PlannedRouteTrainId = plannedTrain.Id,
                        Train = train,
                        Route = route,
                        Trip = trip,
                        Stantion = stantion,
                        PlannedInTime = plannedStantion.InTime,
                        PlannedOutTime = plannedStantion.OutTime
                    };
                    if (changePlannedStantion == null)
                    {
                        res.FactInTime = res.PlannedInTime;
                        res.FactOutTime = res.PlannedOutTime;
                    }
                    else
                    {
                        res.FactInTime = changePlannedStantion.InTime;
                        res.FactOutTime = changePlannedStantion.OutTime;
                        res.IsDropped = changePlannedStantion.Droped;
                    }
                    return res;
                }, new { DateFrom = dateFrom, DateTo = dateTo })).ToList();

            var tripsList = new List<ScheduleDeviationTripsInternal>();
            foreach (var stantionsByTripGroup in resultTrips.GroupBy(o => o.Trip.Id))
            {
                var trip = new ScheduleDeviationTripsInternal();

                var listTmp = stantionsByTripGroup.OrderBy(o => o.PlannedInTime).ToList();
                var firstPlanStantion = listTmp.FirstOrDefault();
                var lastPlanStantion = listTmp.LastOrDefault();
                var firstFactStantion = listTmp.FirstOrDefault(o => !o.IsDropped);
                var lastFactStantion = listTmp.LastOrDefault(o => !o.IsDropped);

                var stantionsViolatedCnt = listTmp.Count(o => o.IsDropped || o.DiffInTime.Ticks > 0);

                if (firstPlanStantion != null && lastPlanStantion != null && lastPlanStantion.Stantion.Id != firstPlanStantion.Stantion.Id)
                {
                    trip.PlannedRouteTrainId = firstPlanStantion.PlannedRouteTrainId;
                    trip.Trip = firstPlanStantion.Trip;
                    trip.Train = firstPlanStantion.Train;
                    trip.Route = firstPlanStantion.Route;
                    trip.PlannedStartTime = firstPlanStantion.PlannedOutTime;
                    trip.PlannedEndTime = lastPlanStantion.PlannedInTime;
                    trip.IsDropped = false;

                    if (firstFactStantion != null && lastFactStantion != null && lastFactStantion.Stantion.Id != firstFactStantion.Stantion.Id)
                    {
                        trip.FactStartTime = firstFactStantion.FactOutTime;
                        trip.FactEndTime = lastFactStantion.FactInTime;
                    }
                    else
                    {
                        trip.IsDropped = true;
                        trip.HasStantionsTimeViolated = true;
                    }
                }

                if (stantionsViolatedCnt > 0)
                {
                    trip.HasStantionsTimeViolated = true;
                }

                tripsList.Add(trip);
            }

            ret.GraphViolationCount = (await GetScheduleDeviationsInternal(dateFrom, dateTo)).Count(o => o.IsDropped || o.DiffInTime.Ticks != 0);
            ret.TrainDepoCount = await GetTrainInDepoCount();
            ret.TrainInTripCount = await GetTrainInTripCount();

            //inspections
            const string sqli = @"select * from
            PlanedInspectionRoutes pi
            left join ChangedPlanedInspectionRoutes cpi on cpi.PlanedInspectionRouteId=pi.Id
            left join PlanedRouteTrains pt on pt.Id = pi.PlanedRouteTrainId
            left join Trains train on pt.TrainId = train.Id
            left join Routes route on route.id = pt.RouteId
            where pt.[Date] BETWEEN @DateFrom AND @DateTo";

            var resultInsp = (await _db.Connection
                .QueryAsync<PlanedInspectionRoute, ChangedPlanedInspectionRoute, PlanedRouteTrain, Train, Route, ScheduleDeviationInspectionInternal>(
                    sqli,
                    (plannedInspection, changePlannedInspection, plannedTrain, train, route) =>
                    {
                        var res = new ScheduleDeviationInspectionInternal
                        {
                            PlannedRouteTrainId = plannedInspection.PlanedRouteTrainId,
                            Train = train,
                            Route = route,
                            PlannedStart = plannedInspection.Start,
                            PlannedEnd = plannedInspection.End,
                            TypePlan = plannedInspection.CheckListType
                        };
                        if (changePlannedInspection == null)
                        {
                            res.FactStart = res.PlannedStart;
                            res.FactEnd = res.PlannedEnd;
                            res.TypeFact = res.TypePlan;
                        }
                        else
                        {
                            //fix dates, krokodil mudofil
                            res.FactStart = GetRealIspectionChangedDate(plannedInspection.Start, changePlannedInspection.Start);
                            res.FactEnd = GetRealIspectionChangedDate(plannedInspection.End, changePlannedInspection.End);
                            //
                            res.IsDropped = changePlannedInspection.Droped;
                            res.TypeFact = changePlannedInspection.CheckListType ?? res.TypePlan;
                        }

                        return res;
                    }, new {DateFrom = dateFrom, DateTo = dateTo})).ToList();

            var groupsDic = tripsList.GroupBy(o => o.PlannedRouteTrainId).Select(o => new TripsInspectionsGroupInternal
            {
                PlannedRouteTrainId = o.Key,
                Train = o.First().Train,
                Route = o.First().Route,
                //все попавшие в интервал трипсы
                Trips = o.Where(p => HasIntersection(start, end, p.PlannedStartTime, p.PlannedEndTime) || HasIntersection(start, end, p.FactStartTime, p.FactEndTime)).ToList(),
                Inspections = new List<ScheduleDeviationInspectionInternal>()
            }).ToDictionary(o => o.PlannedRouteTrainId);

            var groupInsp = resultInsp.GroupBy(o => o.PlannedRouteTrainId).Select(o => new TripsInspectionsGroupInternal
            {
                PlannedRouteTrainId = o.Key,
                Train = o.First().Train,
                Route = o.First().Route,
                //все попавшие в интервал инспекции
                Inspections = o.Where(p => HasIntersection(start, end, p.PlannedStart, p.PlannedEnd) || HasIntersection(start, end, p.FactStart, p.FactEnd)).ToList(),
                Trips = new List<ScheduleDeviationTripsInternal>()
            }).ToList();

            //add inspections to groups with trips
            foreach (var item in groupInsp)
            {
                if (groupsDic.ContainsKey(item.PlannedRouteTrainId))
                {
                    //update
                    groupsDic[item.PlannedRouteTrainId].Inspections = item.Inspections;
                }
                else
                {
                    //insert
                    groupsDic[item.PlannedRouteTrainId] = item;
                }
            }

            var items = groupsDic
                .Where(o => (o.Value.Inspections.Count + o.Value.Trips.Count) > 0)
                .Select(o => SetViolationAndMaxDiffTicks(o.Value))
                .OrderBy(o => o, new TripsInspectionsGroupInternalComparer())
                .Select(o => new RouteDiffRecordDto
            {
                RouteName = o.Route.Name,
                TrainName = o.Train.Name,
                BgColor = o.BgColor,
                EventsPlan = o.Trips.Select(k => new TimedTaskDto
                {
                    Start = k.PlannedStartTime,
                    End = k.PlannedEndTime,
                    Name = k.Trip.Name,
                    BgColor = "#d0c7e7ff",
                    FgColor = "#ff3c5466"
                }).Concat(
                    o.Inspections.Select(i => new TimedTaskDto
                    {
                        Start = i.PlannedStart,
                        End = i.PlannedEnd,
                        Name = FormatToType(i.TypePlan),
                        BgColor = "#fffffe85",
                        FgColor = "#ff3c5466"
                    })).OrderBy(p => p.Start).ToList(),
                EventsFact = o.Trips.Where(p => !p.IsDropped).Select(k => new TimedTaskDto
                {
                    Start = k.FactStartTime.Value,
                    End = k.FactEndTime,
                    Name = k.Trip.Name,
                    BgColor = "#d0c7e7ff",
                    FgColor = "#ff3c5466",
                    BorderColor = k.HasStantionsTimeViolated ? "#ffff0000" : null
                }).Concat(
                    o.Inspections.Where(p => !p.IsDropped).Select(i => new TimedTaskDto
                    {
                        Start = i.FactStart,
                        End = i.FactEnd,
                        Name = FormatToType(i.TypeFact),
                        BgColor = i.BgColor,
                        FgColor = "#ff3c5466",
                        BorderColor = i.BorderColor
                    })).OrderBy(p => p.Start).ToList(),
            }).ToArray();

            ret.Items = items;

            return ret;
        }

        public async Task<BrigadeScheduleDeviationTableDto> GetBrigadeScheduleDeviationTable()
        {
            var ret = new BrigadeScheduleDeviationTableDto();

            var now = DateTime.Now;// - TimeSpan.FromDays(3);
            var dateFrom = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0, now.Kind);
            var dateTo = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999, now.Kind);

            //var start = now - TimeSpan.FromHours(minusHours);
            //var end = now + TimeSpan.FromHours(plusHours);

            //trips
            const string sql = @"select * from
PlaneBrigadeTrains pb
left join ChangePlaneBrigadeTrains cb on cb.PlaneBrigadeTrainId=pb.id
left join PlanedRouteTrains pt on pt.Id = pb.PlanedRouteTrainId
left join Trains train on pt.TrainId = train.Id
left join Routes route on route.id = pt.RouteId
left join PlaneStantionOnTrips ps1 on ps1.id=pb.StantionStartId
left join PlaneStantionOnTrips ps2 on ps2.id=pb.StantionEndId
left join PlaneStantionOnTrips ps3 on ps3.id=cb.StantionStartId
left join PlaneStantionOnTrips ps4 on ps4.id=cb.StantionEndId
left join Stantions s1 on s1.id=ps1.StantionId
left join Stantions s2 on s2.id=ps2.StantionId
left join Stantions s3 on s3.id=ps3.StantionId
left join Stantions s4 on s4.id=ps4.StantionId
left join auth_users u1 on u1.id=pb.UserId
left join auth_users u2 on u2.id=cb.UserId
where pt.[Date] BETWEEN @DateFrom AND @DateTo";

            var result = (await _db.Connection.QueryAsync<BrigadeScheduleDeviationInternal>(sql,
                new []
                {
                    typeof(PlaneBrigadeTrain), typeof(ChangePlaneBrigadeTrain), typeof(PlanedRouteTrain), typeof(Train), typeof(Route), typeof(PlaneStantionOnTrip), typeof(PlaneStantionOnTrip), typeof(PlaneStantionOnTrip), typeof(PlaneStantionOnTrip), typeof(Stantion), typeof(Stantion), typeof(Stantion), typeof(Stantion), typeof(User), typeof(User)
                },
                objects =>
                {
                    var res = new BrigadeScheduleDeviationInternal
                    {
                        Train = objects[3] as Train,
                        Route = objects[4] as Route,
                        PlanStantionIn = objects[9] as Stantion,
                        PlanStantionOut = objects[10] as Stantion,
                        PlanUser = objects[13] as User
                    };

                    if (objects[1] != null)
                    {
                        //have changes
                        if (objects[1] is ChangePlaneBrigadeTrain changes)
                        {
                            res.IsDropped = changes.Droped;
                            res.FactStantionIn = objects[11] as Stantion;
                            res.FactStantionOut = objects[12] as Stantion;
                            res.FactUser = objects[14] as User;
                        }
                    }

                    if (res.FactStantionIn == null)
                    {
                        res.FactStantionIn = res.PlanStantionIn;
                    }

                    if (res.FactStantionOut == null)
                    {
                        res.FactStantionOut = res.PlanStantionOut;
                    }

                    if (res.FactUser == null)
                    {
                        res.FactUser = res.PlanUser;
                    }

                    return res;
                }, new { DateFrom = dateFrom, DateTo = dateTo })).Where(o => o.IsDropped || o.IsUserChanged || o.IsStantionChanged).OrderBy(o => o, new BrigadeScheduleDeviationInternalComparer()).ToList();

            //header
            ret.GraphViolationCount = (await GetScheduleDeviationsInternal(dateFrom, dateTo)).Count(o => o.IsDropped || o.DiffInTime.Ticks != 0);
            ret.TrainDepoCount = await GetTrainInDepoCount();
            ret.TrainInTripCount = await GetTrainInTripCount();

            var items = new List<BrigadeScheduleDeviationTableItemDto>();
            foreach (var item in result)
            {
                var item2 = new BrigadeScheduleDeviationTableItemDto
                {
                    TrainName = item.Train.Name,
                    RouteName = item.Route.Name,
                    StantionsPlan = $"{item.PlanStantionIn.Name} / {item.PlanStantionOut.Name}",
                    StantionsFact = item.IsDropped? "" : $"{item.FactStantionIn.Name} / {item.FactStantionOut.Name}",
                    UserPlan = item.PlanUser.Name,
                    UserFact = item.FactUser.Name,
                    BgColor = item.BgColor,
                    FgColor = item.FgColor
                };
                items.Add(item2);
            }

            ret.Items = items.ToArray();

            return ret;
        }

        public async Task<ToDeviationTableDto> GetToDeviationTable()
        {
            var ret = new ToDeviationTableDto();

            var now = DateTime.Now;
            var dateFrom = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0, now.Kind);
            var dateTo = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999, now.Kind);

            const string sqli = @"select * from
            PlanedInspectionRoutes pi
            left join ChangedPlanedInspectionRoutes cpi on cpi.PlanedInspectionRouteId=pi.Id
            left join PlanedRouteTrains pt on pt.Id = pi.PlanedRouteTrainId
            left join Trains train on pt.TrainId = train.Id
            left join Routes route on route.id = pt.RouteId
            where pt.[Date] BETWEEN @DateFrom AND @DateTo";

            var result = (await _db.Connection
                .QueryAsync<PlanedInspectionRoute, ChangedPlanedInspectionRoute, PlanedRouteTrain, Train, Route, ToDeviationInternal>(
                    sqli,
                    (plannedInspection, changePlannedInspection, plannedTrain, train, route) =>
                    {
                        var res = new ToDeviationInternal
                        {
                            Train = train,
                            Route = route,
                            PlannedStart = plannedInspection.Start,
                            PlannedEnd = plannedInspection.End,
                            TypePlan = plannedInspection.CheckListType
                        };
                        if (changePlannedInspection == null)
                        {
                            res.FactStart = res.PlannedStart;
                            res.FactEnd = res.PlannedEnd;
                            res.TypeFact = res.TypePlan;
                        }
                        else
                        {
                            res.FactStart = GetRealIspectionChangedDate(plannedInspection.Start, changePlannedInspection.Start);
                            res.FactEnd = GetRealIspectionChangedDate(plannedInspection.Start, changePlannedInspection.End);
                            res.IsDropped = changePlannedInspection.Droped;
                            res.TypeFact = changePlannedInspection.CheckListType ?? res.TypePlan;
                        }

                        return res;
                    }, new { DateFrom = dateFrom, DateTo = dateTo })).Where(o => o.IsDropped || o.DiffStart.Ticks > 0 || o.DiffEnd.Ticks > 0).OrderBy(o => o, new ToDeviationComparer()).ToList(); ;

            //header
            ret.GraphViolationCount = (await GetScheduleDeviationsInternal(dateFrom, dateTo)).Count(o => o.IsDropped || o.DiffInTime.Ticks != 0);
            ret.TrainDepoCount = await GetTrainInDepoCount();
            ret.TrainInTripCount = await GetTrainInTripCount();

            var items = new List<ToDeviationTableItemDto>();
            foreach (var item in result)
            {
                var item2 = new ToDeviationTableItemDto
                {
                    TrainName = item.Train.Name,
                    RouteName = item.Route.Name,
                    PlanTime = item.PlannedStart.ToString("t") + " - " + item.PlannedEnd.ToString("t"),
                    FactTime = item.IsDropped? "отменено" : item.FactStart.ToString("t") + " - " + item.FactEnd.ToString("t"),
                    Type = FormatToType(item.TypePlan) + " / " + FormatToType(item.TypeFact),
                    BgColor = item.BgColor,
                    FgColor = item.FgColor
                };
                items.Add(item2);
            }

            ret.Items = items.ToArray();

            return ret;
        }

        //Перечень составов с критическими неисправностями, за сутки
        public async Task<CriticalMalfunctionsTableDto> GetCriticalMalfunctionsTable()
        {
            var now = DateTime.Now;
            var dateFrom = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0, now.Kind);
            var dateTo = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999, now.Kind);

            var ret = new CriticalMalfunctionsTableDto
            {
                GraphViolationCount = (await GetScheduleDeviationsInternal(dateFrom, dateTo)).Count(o => o.IsDropped || o.DiffInTime.Ticks != 0),
                TrainDepoCount = await GetTrainInDepoCount(),
                TrainInTripCount = await GetTrainInTripCount()
            };

            const string sql = @"select tr.*,r.*,
                (select count(*) from TrainTaskAttributes ta1 where ta1.TrainTaskId = t.Id and TaskLevel = 3) as Id,
            (select count(*) from TrainTaskAttributes ta1 where ta1.TrainTaskId = t.Id) as Id
                from TrainTasks t
            left join Carriages c on c.Id = t.CarriageId
            left join Trains tr on tr.Id = c.TrainId
            left join PlanedRouteTrains ptr on (ptr.TrainId = tr.Id and ptr.Date between @DateFrom and @DateTo)
            left join [Routes] r on r.Id = ptr.RouteId
            where(select top 1 ts.Status from TrainTaskStatuses ts where ts.TrainTaskId = t.Id order by ts.Date desc) != 99";

            var result = (await _db.Connection.QueryAsync<Train, Route, int, int, CriticalMalfunctionItemInternal>(sql,
                (train, route, criticalCnt, totalCnt) =>
                {
                    var res = new CriticalMalfunctionItemInternal
                    {
                        Train = train,
                        Route = route,
                        CriticalCnt = criticalCnt,
                        TotalCnt = totalCnt
                    };
                    return res;
                }
                , new {DateFrom = dateFrom, DateTo = dateTo})).ToList();

            ret.Items = result.Where(o => o.Route != null).GroupBy(o => new {o.Train, o.Route})
                .Select(k => new CriticalMalfunctionsTableItemDto
                {
                    RouteName = k.Key.Route.Name,
                    TrainName = k.Key.Train.Name,
                    CriticalCount = k.Sum(t => t.CriticalCnt),
                    TotalCount = k.Sum(t => t.TotalCnt)
                }).ToArray();

            return ret;
        }

        //Количество и критичность неисправностей на составах запланированных для ремонтных работ и ТО на текущую смену
        public async Task<TrainsInDepoMalfunctionTableDto> GetTrainsInDepoDepoMalfunctionsTable(int depoStantionId)
        {
            var now = DateTime.Now;
            var dateFrom = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0, now.Kind);
            var dateTo = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999, now.Kind);

            var ret = new TrainsInDepoMalfunctionTableDto
            {
                GraphViolationCount = (await GetScheduleDeviationsInternal(dateFrom, dateTo)).Count(o =>o.IsDropped || o.DiffInTime.Ticks != 0),
                TrainDepoCount = await GetTrainInDepoCount(depoStantionId),
                TrainInTripCount = await GetTrainInTripCount()
            };

            const string sql = @"select t.*,de.InTime as Id,
                (select count(*) from TrainTaskAttributes ta1 left join TrainTasks tt1 on tt1.Id=ta1.TrainTaskId
                left join Carriages c on c.Id=tt1.CarriageId
                left join Trains t on t.Id=c.TrainId
                where t.Id=de.TrainId and ta1.TaskLevel = 3
                and (select top 1 ts.Status from TrainTaskStatuses ts where ts.TrainTaskId=tt1.Id order by ts.[Date] desc) != 99) as Id,
                (select count(*) from TrainTaskAttributes ta1 left join TrainTasks tt2 on tt2.Id=ta1.TrainTaskId
                left join Carriages c on c.Id=tt2.CarriageId
                left join Trains t on t.Id=c.TrainId
                where t.Id=de.TrainId and (select top 1 ts.Status from TrainTaskStatuses ts where ts.TrainTaskId=tt2.Id order by ts.[Date] desc) != 99) as Id
                from DepoEvents de
                left join Trains t on t.Id=de.TrainId
                left join Parkings p on p.Id=de.ParkingId
                where de.InTime <= @DateNow and (de.TestStopTime >= @DateNow or de.TestStopTime is null) and p.StantionId=@DepoStantionId";

            var result = (await _db.Connection.QueryAsync<Train, DateTime, int, int, TrainsInDepoMalfunctionTableItemDto>(sql,
                (train, inTime, criticalCnt, totalCnt) =>
                {
                    var res = new TrainsInDepoMalfunctionTableItemDto
                    {
                        TrainName = train.Name,
                        DepoInTime = inTime.ToString("t"),
                        CriticalCount = criticalCnt,
                        TotalCount = totalCnt
                    };
                    return res;
                }
                , new { DepoStantionId = depoStantionId, DateNow = now })).OrderByDescending(o => o.TotalCount).ToArray();

            ret.Items = result;

            return ret;
        }

        //Состояние составов в депо
        public async Task<TrainsInDepoStatusTableDto> GetTrainsInDepoStatusTable(int depoStantionId)
        {
            var now = DateTime.Now;
            var dateFrom = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0, now.Kind);
            var dateTo = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999, now.Kind);

            var ret = new TrainsInDepoStatusTableDto
            {
                GraphViolationCount =
                    (await GetScheduleDeviationsInternal(dateFrom, dateTo)).Count(o =>
                        o.IsDropped || o.DiffInTime.Ticks != 0),
                TrainDepoCount = await GetTrainInDepoCount(depoStantionId),
                TrainInTripCount = await GetTrainInTripCount()
            };

            const string sql = @"select de.*,t.*,p.*,i.*,
            (select count(*) from TrainTaskAttributes ta1 left join TrainTasks tt1 on tt1.Id=ta1.TrainTaskId
            left join Carriages c on c.Id=tt1.CarriageId
            where c.TrainId=de.TrainId and (select top 1 ts.[Status] from TrainTaskStatuses ts where ts.TrainTaskId=tt1.Id order by ts.[Date] desc) != 99) as Id,
            (select count(*) from TrainTaskAttributes ta2 left join TrainTasks tt2 on tt2.Id=ta2.TrainTaskId
            left join Carriages c2 on c2.Id=tt2.CarriageId
            where c2.TrainId=de.TrainId and (select top 1 ts2.[Status] from TrainTaskStatuses ts2 where ts2.TrainTaskId=tt2.Id and ts2.[Date] <= de.InTime order by ts2.[Date] desc) != 99) as Id
            from DepoEvents de
            left join Trains t on t.Id=de.TrainId
            left join Parkings p on p.Id=de.ParkingId
            left join Inspections i on i.Id=de.InspectionId
            where de.InTime <= @DateNow and (de.OutTime >= @DateNow or de.OutTime is null) and p.StantionId=@DepoStantionId
            ORDER BY de.UpdateDate Desc";

            ret.Items = (await _db.Connection
                .QueryAsync<DepoEvent, Train, Parking, Inspection, int, int, TrainsInDepoStatusItemInternal>(sql,
                    (depoEvent, train, parking, inspection, inCnt, nowCnt) =>
                    {
                        var res = new TrainsInDepoStatusItemInternal
                        {
                            Train = train,
                            Inspection = inspection,
                            Parking = parking,
                            DepoEvent = depoEvent,
                            OpenTasksCountAtInTime = inCnt,
                            OpenTasksCountNow = nowCnt,
                        };
                        return res;
                    }
                    , new {DateNow = now, DepoStantionId = depoStantionId })).Select(o =>
                new TrainsInDepoStatusTableItemDto
                {
                    TrainName = o.Train.Name,
                    ParkingName = o.Parking.Name,
                    InspectionName = o.Inspection == null? o.DepoEvent.InspectionTxt : FormatToType(o.Inspection.CheckListType),
                    InTime =  o.DepoEvent.InTime.Date == DateTime.Now.Date
                                                                     ? o.DepoEvent.InTime.ToString("t")
                                                                     : o.DepoEvent.InTime.ToString("dd.MM"),
                    OpenTasksCountAtInTime = o.OpenTasksCountAtInTime,
                    OpenTasksCountNow = o.OpenTasksCountNow,
                    UpdateDate = o.DepoEvent.UpdateDate
                }).ToArray();
            ret.Items = ret.Items.GroupBy(x => $"{x.TrainName}_{x.ParkingName}")
                           .Select(x => x.FirstOrDefault(y => x.Max(z => z.UpdateDate) == y.UpdateDate)).ToArray();
            return ret;
        }

        public async Task<JournalsTableDto> GetJournalsTable(int depoStantionId)
        {
            var now = DateTime.Now;
            var dateFrom = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0, now.Kind);
            var dateTo = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999, now.Kind);

            var ret = new JournalsTableDto
                      {
                          GraphViolationCount =
                              (await GetScheduleDeviationsInternal(dateFrom, dateTo)).Count(o =>
                                                                                                o.IsDropped ||
                                                                                                o.DiffInTime.Ticks !=
                                                                                                0),
                          TrainDepoCount = await GetTrainInDepoCount(depoStantionId),
                          TrainInTripCount = await GetTrainInTripCount()
                      };
            const string sql =
                @"SELECT  t.Id,OrderDate,Status, Count, Type, TrainName, Author, BrigadeType, EditDate, CreateDate, CarriageName, EquipmentName,HasInspection,TaskType,1 i
                FROM View_TaskInspections t
                WHERE ParentId IS NULL 
                and (SELECT TOP 1 StantionId FROM Trains WHERE Id = [t].TrainId)=@depoStantionId
                
                or (SELECT COUNT(*) FROM View_TaskInspections c WHERE t.HasInspection=1 and c.ParentId=t.Id and (SELECT TOP 1 StantionId FROM Trains WHERE Id = [t].TrainId)=@depoStantionId)> 0
                ORDER BY OrderDate Desc";
            ret.Items = (await _db.Connection.QueryAsync<JournalGridModel, int, JournalsTableItemDto>(sql,
                                                                                                     (j, i) =>
                                                                                                         new
                                                                                                             JournalsTableItemDto()
                                                                                                         {
                                                                                                             Id = j.Id,
                                                                                                             Type = j.Type.GetDescription(),
                                                                                                             TrainName =
                                                                                                                 j.TrainName,
                                                                                                             Author = j
                                                                                                                 .Author,
                                                                                                             CarriageName
                                                                                                                 = j
                                                                                                                     .CarriageName,
                                                                                                             EquipmentName
                                                                                                                 = j
                                                                                                                     .EquipmentName,
                                                                                                             Date =
                                                                                                                 j.CreateDate
                                                                                                                  .Date ==
                                                                                                                 DateTime
                                                                                                                     .Now
                                                                                                                     .Date
                                                                                                                     ? j
                                                                                                                       .CreateDate
                                                                                                                       .ToString("t")
                                                                                                                     : j
                                                                                                                       .CreateDate
                                                                                                                       .ToString("dd.MM"),
                                                                                                             OrderDate =
                                                                                                                 j.OrderDate,
                                                                                                             HasInspection = j.HasInspection
                                                                                                         },new{depoStantionId},splitOn:"Id,i"));
            return ret;
        }

        //вспомогательные методы

        private async Task<int> GetTrainInDepoCount(int depoStantionId = 0)
        {
            var now = DateTime.Now;
            var clause = depoStantionId > 0
                             ? $"and (select top 1 StantionId from Parkings where Id = e.ParkingId) = {depoStantionId}"
                             : "";
            string sqlDepoCount = $@"select count(Distinct(e.TrainId)) from
            DepoEvents e
            where e.InTime <= @DateNow and (e.OutTime >= @DateNow OR e.OutTime is null) {clause}";

            var count = await _db.Connection.QuerySingleAsync<int>(sqlDepoCount, new { DateNow = now });

            return count;
        }

        private async Task<int> GetTrainInTripCount()
        {
            var now = DateTime.Now;// - TimeSpan.FromDays(3);
            var dateFrom = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0, now.Kind);
            var dateTo = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999, now.Kind);

            const string sqlTrainCount = @"select count(Distinct(p.TrainId)) from
            PlanedRouteTrains p
            where p.[Date] BETWEEN @DateFrom AND @DateTo";

            var count = await _db.Connection.QuerySingleAsync<int>(sqlTrainCount, new { DateFrom = dateFrom, DateTo = dateTo });

            return count;
        }

        private DateTime GetRealIspectionChangedDate(DateTime plan, DateTime fact)
        {
            var ret = new DateTime(plan.Year, plan.Month, plan.Day, fact.Hour, fact.Minute, fact.Second, fact.Millisecond, plan.Kind);

            if (fact.Hour >= 0 && fact.Hour < 3)
            {
                ret = ret.AddDays(1);
            }

            return ret;
        }

        private async Task<List<ScheduleDeviationInternal>> GetScheduleDeviationsInternal(DateTime dateFrom, DateTime dateTo)
        {
            const string sql = @"select * from
            PlaneStantionOnTrips ps
            left join Stantions s on s.Id = ps.StantionId
            left join Trips trip on trip.Id = ps.TripId
            left join ChangePlaneStantionOnTrips cs on cs.PlaneStantionOnTripId = ps.Id
            left join PlanedRouteTrains pt on pt.Id = ps.PlanedRouteTrainId
            left join Trains train on pt.TrainId = train.Id
            where pt.[Date] BETWEEN @DateFrom AND @DateTo";

            var result = (await _db.Connection.QueryAsync<PlaneStantionOnTrip, Stantion, Trip, ChangePlaneStantionOnTrip, PlanedRouteTrain, Train, ScheduleDeviationInternal>(
                sql,
                (plannedStantion, stantion, trip, changePlannedStantion, plannedTrain, train) =>
                {
                    var res = new ScheduleDeviationInternal
                    {
                        Train = train,
                        Stantion = stantion,
                        Trip = trip,
                        PlannedInTime = plannedStantion.InTime,
                        PlannedOutTime = plannedStantion.OutTime
                    };
                    if (changePlannedStantion == null)
                    {
                        res.FactInTime = res.PlannedInTime;
                        res.FactOutTime = res.PlannedOutTime;
                    }
                    else
                    {
                        res.FactInTime = changePlannedStantion.InTime;
                        res.FactOutTime = changePlannedStantion.OutTime;
                        res.IsDropped = changePlannedStantion.Droped;
                    }
                    return res;
                }, new { DateFrom = dateFrom, DateTo = dateTo })).ToList();

            return result;
        }

        private static bool HasIntersection(DateTime? start1, DateTime? end1, DateTime? start2, DateTime? end2)
        {
            if (start1 == null || end1 == null || start2 == null || end2 == null)
            {
                return false;
            }

            return start1 <= end2 && end1 >= start2;
        }

        private static string FormatTimeSpan(TimeSpan time, bool isDropped)
        {
            if (isDropped)
                return "отменен";

            return ((time < TimeSpan.Zero) ? "-" : "") + time.ToString(@"hh\:mm");
        }

        private static TripsInspectionsGroupInternal SetViolationAndMaxDiffTicks(TripsInspectionsGroupInternal obj)
        {
            obj.IsViolated = obj.Inspections.Count(o => o.TypeFact != o.TypePlan || o.IsDropped) +
                             obj.Trips.Count(o => o.IsDropped) > 0;

            long maxInsp = 0;
            if (obj.Inspections.Count > 0)
            {
                maxInsp = obj.Inspections.Max(o => o.DiffStart.Ticks);
            }

            long maxTrips = 0;
            if (obj.Trips.Count > 0)
            {
                maxTrips = obj.Trips.Where(o => o.DiffStart.HasValue).Max(o => o.DiffStart.Value.Ticks);
            }

            obj.MaxDiffTicks = Math.Max(maxTrips, maxInsp);

            return obj;
        }

        private static string FormatToType(CheckListType? type)
        {
            var ret = "?";
            switch (type)
            {
                case CheckListType.Inspection:
                    ret = "Приемка";
                    break;
                case CheckListType.Surrender:
                    ret = "Сдача";
                    break;
                case CheckListType.TO1:
                    ret = "ТО1";
                    break;
                case CheckListType.TO2:
                    ret = "ТО2";
                    break;
                case CheckListType.CTO:
                    ret = "СТО";
                    break;
            }

            return ret;
        }
    }

 

    #region Internal classes
    //internal class TrainsInDepoMalfunctionItemInternal
    //{
    //    public Train Train { get; set; }

    //    public DateTime InTime { get; set; }

    //    public int CriticalCnt { get; set; }

    //    public int TotalCnt { get; set; }
    //}

    public class TrainsInDepoStatusItemInternal
    {
        public Train Train { get; set; }

        public Parking Parking { get; set; }

        public Inspection Inspection { get; set; }

        public DepoEvent DepoEvent { get; set; }

        public int OpenTasksCountAtInTime { get; set; }

        public int OpenTasksCountNow { get; set; }
    }

    internal class CriticalMalfunctionItemInternal
    {
        public Train Train { get; set; }

        public Route Route { get; set; }

        public int CriticalCnt { get; set; }

        public int TotalCnt { get; set; }
    }

    internal class ScheduleDeviationTripsInternal
    {
        public int PlannedRouteTrainId { get; set; }

        public Train Train { get; set; }

        public Route Route { get; set; }

        public Trip Trip { get; set; }

        public DateTime PlannedStartTime { get; set; }

        public DateTime PlannedEndTime { get; set; }

        public DateTime? FactStartTime { get; set; }

        public DateTime? FactEndTime { get; set; }

        public bool IsDropped { get; set; }

        public bool HasStantionsTimeViolated { get; set; }

        public TimeSpan? DiffStart => FactStartTime.HasValue ? FactStartTime.Value - PlannedStartTime : (TimeSpan?)null;

        //public TimeSpan? DiffEnd => FactEndTime.HasValue ? FactEndTime.Value - PlannedEndTime : (TimeSpan?)null;
    }

    internal class ScheduleDeviationStantionsInternal
    {
        public int PlannedRouteTrainId { get; set; }

        public Train Train { get; set; }

        public Route Route { get; set; }

        public Trip Trip { get; set; }

        public Stantion Stantion { get; set; }

        public DateTime PlannedInTime { get; set; }

        public DateTime PlannedOutTime { get; set; }

        public DateTime FactInTime { get; set; }

        public DateTime FactOutTime { get; set; }

        public bool IsDropped { get; set; }

        public TimeSpan DiffInTime => FactInTime - PlannedInTime;

        //public TimeSpan DiffOutTime => FactOutTime - PlannedOutTime;
    }

    internal class ScheduleDeviationInspectionInternal
    {
        public int PlannedRouteTrainId { get; set; } //plannedRouteId

        public Train Train { get; set; }

        public Route Route { get; set; }

        public DateTime PlannedStart { get; set; }

        public DateTime PlannedEnd { get; set; }

        public DateTime FactStart { get; set; }

        public DateTime FactEnd { get; set; }

        public bool IsDropped { get; set; }

        public CheckListType? TypePlan { get; set; }

        public CheckListType? TypeFact { get; set; }

        public string BorderColor => (IsDropped || TypePlan != TypeFact) ? "#ffff0000" : null;

        public string BgColor => "#fffffe85";

        public TimeSpan DiffStart => FactStart - PlannedStart;

        //public TimeSpan DiffEnd => FactEnd - PlannedEnd;
    }

    internal class ScheduleDeviationInternal
    {
        public Train Train { get; set; }

        public Trip Trip { get; set; }

        public Stantion Stantion { get; set; }

        public DateTime PlannedInTime { get; set; }

        public DateTime PlannedOutTime { get; set; }

        public DateTime FactInTime { get; set; }

        public DateTime FactOutTime { get; set; }

        public bool IsDropped { get; set; }

        public TimeSpan DiffInTime => FactInTime - PlannedInTime;

        public TimeSpan DiffOutTime => FactOutTime - PlannedOutTime;

        public string Color
        {
            get
            {
                if (IsDropped)
                {
                    return "#ffff9898"; //red
                }

                if (DiffInTime < new TimeSpan(0, 5, 0))
                {
                    return "#ffffffff"; //white
                }

                if (DiffInTime < new TimeSpan(1, 0, 0))
                {
                    return "#fffff3bc";
                }

                return "#ffff9898";
            }
        }
    }

    internal class TripsInspectionsGroupInternal
    {
        public int PlannedRouteTrainId { get; set; }

        public Train Train { get; set; }

        public Route Route { get; set; }

        public List<ScheduleDeviationInspectionInternal> Inspections { get; set; }

        public List<ScheduleDeviationTripsInternal> Trips { get; set; }

        public bool IsViolated { get; set; }
        //{
        //    get
        //    {
        //        return Inspections.Count(o => o.TypeFact != o.TypePlan || o.IsDropped) + Trips.Count(o => o.IsDropped) > 0;
        //    }
        //}

        public long MaxDiffTicks { get; set; }
            //=> Math.Max(Inspections.Max(o => o.DiffStart.Ticks), Trips.Max(o => o.DiffInTime.Ticks));

        //public string BgColor => IsViolated ? "#ffff0000" : "#ffffffff";

        public string BgColor => "#ffffffff";

        public TimeSpan MaxDiffInTime { get; set; }
    }

    internal class BrigadeScheduleDeviationInternal
    {
        public Train Train { get; set; }

        public Route Route { get; set; }

        public User PlanUser { get; set; }

        public User FactUser { get; set; }

        public Stantion PlanStantionIn { get; set; }

        public Stantion PlanStantionOut { get; set; }

        public Stantion FactStantionIn { get; set; }

        public Stantion FactStantionOut { get; set; }

        public bool IsDropped { get; set; }

        public bool IsUserChanged => PlanUser.Id != FactUser.Id;

        public bool IsStantionChanged => (PlanStantionIn.Id != FactStantionIn.Id) || (PlanStantionOut.Id != FactStantionOut.Id);

        public CheckListType? TypePlan { get; set; }

        public CheckListType? TypeFact { get; set; }

        public string BgColor => "#ffffffff";

        public string FgColor => "#ff3c5466";
    }

    internal class ToDeviationInternal
    {
        public Train Train { get; set; }

        public Route Route { get; set; }

        public DateTime PlannedStart { get; set; }

        public DateTime PlannedEnd { get; set; }

        public DateTime FactStart { get; set; }

        public DateTime FactEnd { get; set; }

        public bool IsDropped { get; set; }

        public CheckListType? TypePlan { get; set; }

        public CheckListType? TypeFact { get; set; }

        public TimeSpan DiffStart => FactStart - PlannedStart;

        public TimeSpan DiffEnd => FactEnd - PlannedEnd;

        public string BgColor => "#ffffffff";

        public string FgColor => "#ff3c5466";
    }
    #endregion

    #region Comparers
    internal class BrigadeScheduleDeviationInternalComparer : IComparer<BrigadeScheduleDeviationInternal>
    {
        public int Compare(BrigadeScheduleDeviationInternal first, BrigadeScheduleDeviationInternal second)
        {
            if (first != null && second != null)
            {
                //dropped to top
                if (first.IsDropped && second.IsDropped)
                {
                    return 0;
                }
                if (first.IsDropped)
                {
                    return -1;
                }
                if (second.IsDropped)
                {
                    return 1;
                }
                //by user
                if (first.IsUserChanged && second.IsUserChanged)
                {
                    return 0;
                }
                if (first.IsUserChanged)
                {
                    return -1;
                }
                if (second.IsUserChanged)
                {
                    return 1;
                }
                //by stantion
                if (first.IsStantionChanged && second.IsStantionChanged)
                {
                    return 0;
                }
                if (first.IsStantionChanged)
                {
                    return -1;
                }
                if (second.IsStantionChanged)
                {
                    return 1;
                }
                //other are equal
                return 0;
            }

            if (first == null && second == null)
            {
                return 0;
            }
            if (first != null)
            {
                return -1;
            }
            return 1;
        }
    }

    internal class TripsInspectionsGroupInternalComparer : IComparer<TripsInspectionsGroupInternal>
    {
        public int Compare(TripsInspectionsGroupInternal first, TripsInspectionsGroupInternal second)
        {
            if (first != null && second != null)
            {
                if (first.IsViolated && second.IsViolated)
                {
                    return 0;
                }
                if (first.IsViolated)
                {
                    return -1;
                }
                if (second.IsViolated)
                {
                    return 1;
                }
                //by max diff (intime/start)
                return second.MaxDiffTicks.CompareTo(first.MaxDiffTicks);
            }

            if (first == null && second == null)
            {
                // We can't compare any properties, so they are essentially equal.
                return 0;
            }

            if (first != null)
            {
                // Only the first instance is not null, so prefer that.
                return -1;
            }

            // Only the second instance is not null, so prefer that.
            return 1;
        }
    }

    internal class ScheduleDeviationInternalComparer : IComparer<ScheduleDeviationInternal>
    {
        public int Compare(ScheduleDeviationInternal first, ScheduleDeviationInternal second)
        {
            if (first != null && second != null)
            {
                // We can compare both properties.
                if (first.IsDropped && second.IsDropped)
                {
                    //compare by in time
                    return second.PlannedInTime.CompareTo(first.PlannedInTime);
                }
                if (first.IsDropped)
                {
                    return -1;
                }
                if (second.IsDropped)
                {
                    return 1;
                }
                //compare by diff timespan
                return second.DiffInTime.Duration().CompareTo(first.DiffInTime.Duration());
            }

            if (first == null && second == null)
            {
                // We can't compare any properties, so they are essentially equal.
                return 0;
            }

            if (first != null)
            {
                // Only the first instance is not null, so prefer that.
                return -1;
            }

            // Only the second instance is not null, so prefer that.
            return 1;
        }
    }

    internal class ToDeviationComparer : IComparer<ToDeviationInternal>
    {
        public int Compare(ToDeviationInternal first, ToDeviationInternal second)
        {
            if (first != null && second != null)
            {
                var maxDiffSecond = Math.Max(second.DiffStart.Ticks, second.DiffEnd.Ticks);
                var maxDiffFirst = Math.Max(first.DiffStart.Ticks, first.DiffEnd.Ticks);

                // We can compare both properties.
                if (first.IsDropped && second.IsDropped)
                {
                    //compare by in time
                    return maxDiffSecond.CompareTo(maxDiffFirst);
                }
                if (first.IsDropped)
                {
                    return -1;
                }
                if (second.IsDropped)
                {
                    return 1;
                }
                //compare by diff timespan
                return maxDiffSecond.CompareTo(maxDiffFirst);
            }

            if (first == null && second == null)
            {
                // We can't compare any properties, so they are essentially equal.
                return 0;
            }

            if (first != null)
            {
                // Only the first instance is not null, so prefer that.
                return -1;
            }

            // Only the second instance is not null, so prefer that.
            return 1;
        }
    }
    #endregion
}
