using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Extensions;
using Rzdppk.Core.Old;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public class StantionTripsRepository : ITripsRepository, IDisposable
    {
        private readonly IDb _db;
        private readonly ILogger _logger;

        public StantionTripsRepository(ILogger logger)
        {
            _db = new Db();
        }

        public StantionTripsRepository(IDb db, ILogger logger)
        {
            _db = db;
            _logger = logger;
        }

    //    public async Task<StantionTrip[]> GetByTripId(int id)
    //    {
    //        var sql = Sql.SqlQueryCach["StantionTrips.ByTripId"];
    //        var result = (await _db.Connection.QueryAsync<StantionTrip>(sql, new { id = id})).ToList();
    //        var sqlRStations = new StantionRepository(_logger);
    //        foreach (var item in result)
    //        {
    //            item.Stantion = await sqlRStations.ByIdWithStations(item.StantionId);
    //        }
            
    //        return result.ToArray();
    //    }

    //    //TODO заменить трип ид на роуте. ЭТО РОУТЕ ID
    //    public async Task<FreeStationUi[]> GetFreeStation(int tripId)
    //    {
    //        var sql = Sql.SqlQueryCach["StantionTrips.FreeStationsByRouteId"];
    //        var stantionTripWithStartOrEnds = (await _db.Connection.QueryAsync<StantionTripWithStartOrEnd>(sql, new { id = tripId})).ToArray();
    //        var sqlRStations = new StantionRepository(_logger);
    //        var output = new List<FreeStationUi>();
    //        for (var index = 0; index < stantionTripWithStartOrEnds.Length; index++)
    //        {
    //            var item = stantionTripWithStartOrEnds[index];

    //            CheckStartOrEndTrip(stantionTripWithStartOrEnds, index, item);

    //            item.Stantion = await sqlRStations.ByIdWithStations(item.StantionId);
    //            //StationName = $"{item.Stantion.Name} НР",
    //            var stationName = item.Stantion.Name;
    //            if (item.StartTripStation.HasValue && item.StartTripStation.Value)
    //                stationName = $"{stationName} НР";
    //            if (item.EndTripStation.HasValue && item.EndTripStation.Value)
    //                continue;
    //                //stationName = $"{stationName} КР";

    //            //output.Add(new FreeStationUi
    //            //{
    //            //    StationName = stationName,
    //            //    StationTripId = item.Id,
    //            //    Time = new TimeSpan(item.HourFrom, item.MinuteFrom, 0)
    //            //});
    //        }
    //        //output = output.OrderBy(e => e.Time).ToList();
    //        return output.ToArray();
    //    }

    //    //TODO заменить трип ид на роуте. ЭТО РОУТЕ ID
    //    public async Task<FreeStationUi[]> GetLandingStation(int tripId, int stationTripId)
    //    {

    //        var sql = Sql.SqlQueryCach["StantionTrips.LandingStationsByRouteId"];
    //        var stantionTrips = (await _db.Connection.QueryAsync<StantionTripWithStartOrEnd>(sql, new { id = tripId})).ToArray();

    //        //stantionTrips = stantionTrips.OrderBy(e => e.HourFrom).ThenBy(e => e.MinuteFrom).ToArray();

    //        var landingStations = new List<StantionTripWithStartOrEnd>();

    //        //перебираем станции до конца массива, начиная с станции где хотим сесть
    //        var goodRange = true;
    //        bool goodStation = false;

    //        for (var index = 0; index < stantionTrips.Length; index++)
    //        {
    //            var item = stantionTrips[index];
    //            if (item.Id == stationTripId)
    //            {
    //                goodStation = true;
    //                continue;
    //            }

    //            //if (goodStation)
    //            //{
    //            //    if (item.BrigadeFromId == null)
    //            //    {
    //            //        CheckStartOrEndTrip(stantionTrips, index, item);
    //            //        landingStations.Add(item);
    //            //    }

    //            //    if (item.BrigadeFromId != null)
    //            //    {
    //            //        goodRange = false;
    //            //        break;
    //            //    }
    //            //}
    //        }

    //        // перебираем с начала массива до станции, где хотим сесть.
    //        if (goodRange)
    //        {
    //            for (var index = 0; index < stantionTrips.Length; index++)
    //            {
    //                var item = stantionTrips[index];
    //                if (item.Id == stationTripId)
    //                {
    //                    CheckStartOrEndTrip(stantionTrips, index, item);
    //                    landingStations.Add(item);
    //                    break;
    //                }

    //                //if (item.BrigadeFromId == null)
    //                //{
    //                //    CheckStartOrEndTrip(stantionTrips, index, item);
    //                //    landingStations.Add(item);
    //                //}

    //                //if (item.BrigadeFromId != null)
    //                //    break;
    //            }
    //        }

    //        var sqlRStations = new StantionRepository(_logger);
    //        var output = new List<FreeStationUi>();

    //        foreach (var item in landingStations)
    //        {
    //            item.Stantion = await sqlRStations.ByIdWithStations(item.StantionId);
    //            //StationName = $"{item.Stantion.Name} НР",
    //            var stationName = item.Stantion.Name;
    //            if (item.StartTripStation.HasValue && item.StartTripStation.Value)
    //                continue;
    //                //stationName = $"{stationName} НР";
    //            if (item.EndTripStation.HasValue && item.EndTripStation.Value)
    //                stationName = $"{stationName} КР";

    //            //output.Add(new FreeStationUi
    //            //{
    //            //    StationName = stationName,
    //            //    StationTripId = item.Id,
    //            //    Time = new TimeSpan(item.HourFrom, item.MinuteFrom, 0)
    //            //});

    //            //if (item.StartTripStation != null || item.EndTripStation != null)
    //            //{

    //            //    if (item.StartTripStation != null && item.StartTripStation.Value)
    //            //    {
    //            //        output.Add(new FreeStationUi
    //            //        {
    //            //            StationName = $"{item.Stantion.Name} НР",
    //            //            StationTripId = item.Id,
    //            //            Time = new TimeSpan(item.HourFrom, item.MinuteFrom, 0)
    //            //        });
    //            //        continue;
    //            //    }
    //            //    if (item.EndTripStation != null && item.EndTripStation.Value)
    //            //    {
    //            //        output.Add(new FreeStationUi
    //            //        {
    //            //            StationName = $"{item.Stantion.Name} КР",
    //            //            StationTripId = item.Id,
    //            //            Time = new TimeSpan(item.HourFrom, item.MinuteFrom, 0)
    //            //        });
    //            //        continue;
    //            //    }

    //            //}

    //        }

    //        //output = output.OrderBy(e => e.Time).ToList();
    //        return output.ToArray();
    //    }

    //    private static void CheckStartOrEndTrip(StantionTripWithStartOrEnd[] stantionTrips, int index, StantionTripWithStartOrEnd item)
    //    {
    //        var indexPreviousElement = 0;
    //        if (index == 0)
    //            indexPreviousElement = stantionTrips.Length - 1;
    //        else
    //            indexPreviousElement = index - 1;

    //        ////Начало трипа
    //        //if (item.TripId != stantionTrips[indexPreviousElement].TripId)
    //        //    item.StartTripStation = true;

    //        //var indexNextElement = 0;
    //        //if (index == stantionTrips.Length - 1)
    //        //    indexNextElement = 0;
    //        //else
    //        //    indexNextElement = index + 1;

    //        ////Конец трипа
    //        //if (item.TripId != stantionTrips[indexNextElement].TripId)
    //        //    item.EndTripStation = true;
    //    }

    //    public class StantionTripWithStartOrEnd : StantionTrip
    //    {
    //        public bool? StartTripStation { get; set; }
    //        public bool? EndTripStation { get; set; }
    //    }


    //    /// <summary>
    //    /// проставляет чеклист у записи в StantionTrips 
    //    /// </summary>
    //    /// <param name="stationTripId"></param>
    //    /// <param name="checkListType"></param>
    //    public async Task AddCheckListToStation (int stationTripId, int? checkListType)
    //    {
    //        var sql = Sql.SqlQueryCach["StantionTrips.UpdateCheckListType"];
    //        await _db.Connection.ExecuteAsync(sql, new { id = stationTripId, checkListType = checkListType });
    //    }

    //    /// <summary>
    //    /// Добавить бригаду к диапазону станций. Начиная от стартовой и заканчивая последней.
    //    /// </summary>
    //    /// <param name="tripId"></param>
    //    /// <param name="stationStartId"></param>
    //    /// <param name="stationEndId"></param>
    //    /// <param name="brigadeId"></param>
    //    public async Task AddBrigadeToStations(int RouteId, int stationStartId, int stationEndId, int brigadeId)
    //    {
    //        var sql = Sql.SqlQueryCach["StantionTrips.StationsTripByRouteId"];
    //        var stantionTrips = (await _db.Connection.QueryAsync<StantionTrip>(sql, new { id = RouteId })).ToArray();

    //        //сортируем всю хуйню, выкидываем ненужные станции.
    //        //stantionTrips = stantionTrips.OrderBy(e => e.HourFrom).ThenBy(e => e.MinuteFrom).ToArray();

    //        bool goodRange = false;
    //        bool endRange = true;

    //        try
    //        {
    //            _db.Transaction.BeginTransaction();
    //            foreach (var item in stantionTrips)
    //            {
    //                //if (item.Id == stationStartId)
    //                //{
    //                //    goodRange = true;
    //                //    //item.Value.BrigadeToId = brigadeId;
    //                //    item.BrigadeToId = await AddBrigadeToStation(item.Id, null, brigadeId, item);
    //                //    continue;
    //                //}
    //                //if (goodRange && item.Id == stationEndId)
    //                //{
    //                //    //item.Value.BrigadeFromId = brigadeId;
    //                //    item.BrigadeToId = await AddBrigadeToStation(item.Id, brigadeId, null, item);
    //                //    endRange = false;
    //                //    break;
    //                //}
    //                //if (goodRange)
    //                //{
    //                //    //item.Value.BrigadeFromId = brigadeId;
    //                //    //item.Value.BrigadeToId = brigadeId;
    //                //    item.BrigadeToId = await AddBrigadeToStation(item.Id, brigadeId, brigadeId, item);
    //                //}
    //            }

    //            if (endRange)
    //            {
    //                foreach (var item in stantionTrips)
    //                {
    //                    //if (item.Id == stationEndId)
    //                    //{
    //                    //    //item.Value.BrigadeFromId = brigadeId;
    //                    //    item.BrigadeToId = await AddBrigadeToStation(item.Id, brigadeId, null, item);
    //                    //    break;
    //                    //}

    //                    ////item.Value.BrigadeFromId = brigadeId;
    //                    ////item.Value.BrigadeToId = brigadeId;
    //                    //item.BrigadeToId = await AddBrigadeToStation(item.Id, brigadeId, brigadeId, item);

    //                }
    //            }
    //            _db.Transaction.CommitTransaction();
    //        }
    //        catch (Exception e)
    //        {
    //            _db.Transaction.RollBackTransaction();
    //            throw new Exception($"Ошибка {e}");
    //        }

    //    }

    //    public async Task DeleteBrigadeFromStations(int tripId, int stationStartId, int stationEndId, int brigadeId)
    //    {
    //        //var pashaSyka = stationEndId;
    //        //stationEndId = stationStartId;
    //        //stationStartId = pashaSyka;

    //        var sql = Sql.SqlQueryCach["StantionTrips.StationsTripByRouteId"];
    //        var stantionTrips = (await _db.Connection.QueryAsync<StantionTrip>(sql, new { id = tripId })).ToArray();
    //        var sqlRStations = new StantionRepository(_logger);

    //        //сортируем всю хуйню, выкидываем ненужные станции.
    //        //stantionTrips = stantionTrips.OrderBy(e => e.HourFrom).ThenBy(e => e.MinuteFrom).ToArray();

    //        bool goodRange = false;
    //        bool endRange = true;

    //        try
    //        {
    //            _db.Transaction.BeginTransaction();
    //            for (var index = 0; index < stantionTrips.Length; index++)
    //            {
    //                var item = stantionTrips[index];
    //                if (item.Id == stationStartId)
    //                {
    //                    goodRange = true;
    //                    //item.Value.BrigadeToId = brigadeId;
    //                    int prevIndex;
    //                    if (index == 0)
    //                        prevIndex = stantionTrips.Length - 1;
    //                    else
    //                        prevIndex = index - 1;
    //                    if (stantionTrips[prevIndex].StantionId == item.StantionId)
    //                    {
    //                        await DeleteBrigadeFromStation(stantionTrips[prevIndex].Id, 0, null, stantionTrips[prevIndex]);
    //                        await DeleteBrigadeFromStation(item.Id, null, null, item);
    //                    }
    //                    else
    //                        await DeleteBrigadeFromStation(item.Id, 0, null, item);

    //                    continue;
    //                }
    //                if (goodRange && item.Id == stationEndId)
    //                {
    //                    //item.Value.BrigadeFromId = brigadeId;
    //                    await DeleteBrigadeFromStation(item.Id, null, 0, item);
    //                    endRange = false;
    //                    break;
    //                }
    //                if (goodRange)
    //                {
    //                    //item.Value.BrigadeFromId = brigadeId;
    //                    //item.Value.BrigadeToId = brigadeId;
    //                    await DeleteBrigadeFromStation(item.Id, null, null, item);
    //                }
    //            }

    //            if (endRange)
    //            {
    //                for (var index = 0; index < stantionTrips.Length; index++)
    //                {
    //                    var item = stantionTrips[index];
    //                    if (item.Id == stationEndId)
    //                    {
    //                        //item.Value.BrigadeFromId = brigadeId;
    //                        await DeleteBrigadeFromStation(item.Id, null, 0, item);
    //                        break;
    //                    }

    //                    //item.Value.BrigadeFromId = brigadeId;
    //                    //item.Value.BrigadeToId = brigadeId;
    //                    await DeleteBrigadeFromStation(item.Id, null, null, item);
    //                }
    //            }
    //            _db.Transaction.CommitTransaction();
    //        }
    //        catch (Exception e)
    //        {
    //            _db.Transaction.RollBackTransaction();
    //            throw new Exception($"Ошибка {e}");
    //        }

    //    }

    //    public async Task DeleteBrigadeFromStation(int stantionTripId, int? brigadeFromId, int? brigadeToId, StantionTrip stationTrip)
    //    {
    //        //if (brigadeFromId == 0)
    //        //    if (stationTrip.BrigadeFromId != null) brigadeFromId = stationTrip.BrigadeFromId.Value;
    //        //    else
    //        //        brigadeFromId = null;
            
    //        //if (brigadeToId == 0)
    //        //    if (stationTrip.BrigadeToId != null)
    //        //        brigadeToId = stationTrip.BrigadeToId.Value;
    //        //    else
    //        //        brigadeToId = null;
                

    //        var sql = Sql.SqlQueryCach["StantionTrips.UpdateBrigadeFromTo"];
    //        await _db.Connection.ExecuteAsync(sql, new { brigadeFromId = brigadeFromId, brigadeToId = brigadeToId, id = stantionTripId }, _db.Transaction.Transaction);
    //    }

    //    public async Task<int?> AddBrigadeToStation(int stantionTripId, int? brigadeFromId, int? brigadeToId, StantionTrip stationTrip)
    //    {
    //        //if (brigadeFromId == null)
    //        //    if (stationTrip.BrigadeFromId != null) brigadeFromId = stationTrip.BrigadeFromId.Value;
    //        //if (brigadeToId == null)
    //        //    if (stationTrip.BrigadeToId != null) brigadeToId = stationTrip.BrigadeToId.Value;

    //        var sql = Sql.SqlQueryCach["StantionTrips.UpdateBrigadeFromTo"];
    //        await _db.Connection.ExecuteAsync(sql, new { brigadeFromId = brigadeFromId, brigadeToId = brigadeToId, id = stantionTripId}, _db.Transaction.Transaction);
    //        return brigadeToId;

    //    }

    //    ////TODO заменить нахуй TripID на RouteID БЛЯДь и в UI тоже
    //    //public async Task<StationsTripsBrigadeToUi[]> GetBrigadesToPutToStationRange(int routeId, int stationStartId, int stationEndId)
    //    //{
    //    //    var sql = Sql.SqlQueryCach["Trips.AllBrigades"];
    //    //    var result = (await _db.Connection.QueryAsync<TripsRepository.TripsBrigadesSql>(sql, new { skip = 0, limit = 9999999 })).ToList();
            
    //    //    //берем все ебучие стайшнтрипсы и раскладываем по маршрутам
    //    //    var routeDictionary = new Dictionary<int, List<TripsRepository.TripsBrigadesSql>>();
    //    //    foreach (var item in result)
    //    //    {
    //    //        if (!routeDictionary.ContainsKey(item.RouteId))
    //    //            routeDictionary.Add(item.RouteId, new List<TripsRepository.TripsBrigadesSql>());
    //    //        routeDictionary[item.RouteId].Add(item);
    //    //    }

    //    //    //день недели - всякая хуйня с бригадой
    //    //    var brigadeUsing = new Dictionary<DayOfWeek, List<BrigadeUsingTime>>();

    //    //    foreach (var items in routeDictionary.Values)
    //    //    {
    //    //        var tripBrigades = items.OrderBy(e => e.HourFrom).ThenBy(e => e.MinuteFrom).ToArray();

    //    //        for (var index = 0; index < tripBrigades.Length; index++)
    //    //        {
    //    //            var tripBrigade = tripBrigades[index];
    //    //            if (tripBrigade.BrigadeToId != null)
    //    //            {
    //    //                //добавляем день недели, если его нет.
    //    //                if (!brigadeUsing.ContainsKey(tripBrigade.Day))
    //    //                    brigadeUsing.Add(tripBrigade.Day, new List<BrigadeUsingTime>());
                        
    //    //                var startTrainTime = new TimeSpan(tripBrigade.HourTo, tripBrigade.MinuteTo, 0);

    //    //                TimeSpan stopTrainTime;
    //    //                if (index == tripBrigades.Length - 1)
    //    //                    stopTrainTime = new TimeSpan(tripBrigades[0].HourFrom, tripBrigades[0].MinuteFrom, 0);
    //    //                else
    //    //                    stopTrainTime = new TimeSpan(tripBrigades[index + 1].HourFrom, tripBrigades[index + 1].MinuteFrom, 0);

    //    //                //кто засунет инициализацию объекта в адд листа = получит пизды
    //    //                var toAdd = new BrigadeUsingTime
    //    //                {
    //    //                    BrigadeId = tripBrigade.BrigadeToId.Value,
    //    //                    StartTrainTime = startTrainTime,
    //    //                    StopTrainTime = stopTrainTime
    //    //                };

    //    //                brigadeUsing[tripBrigade.Day].Add(toAdd);

    //    //            }
    //    //        }
    //    //    }

    //    //    //теперь надо диапзоны време когда между указанными на входе станциями
    //    //    var needTime = new List<StartStopTimes>();
    //    //    var targetRouteStationTrips = routeDictionary[routeId].OrderBy(e => e.HourFrom).ThenBy(e => e.MinuteFrom).ToList();
    //    //    TimeSpan startTime;
    //    //    TimeSpan stopTime;

    //    //    bool goodRange = false;
    //    //    bool endRange = true;

    //    //    for (var index = 0; index < targetRouteStationTrips.Count; index++)
    //    //    {
    //    //        var item = targetRouteStationTrips[index];
    //    //        if (item.StantionTripId == stationStartId)
    //    //        {
    //    //            goodRange = true;
    //    //            startTime = new TimeSpan(item.HourTo, item.MinuteTo, 0);
    //    //            continue;
    //    //        }
    //    //        if (goodRange && item.StantionTripId == stationEndId)
    //    //        {
    //    //            stopTime = new TimeSpan(item.HourFrom, item.MinuteFrom, 0);
    //    //            var toAdd = new StartStopTimes
    //    //            {
    //    //                StartTrainTime = startTime,
    //    //                StopTrainTime = stopTime
    //    //            };
    //    //            needTime.Add(toAdd);
    //    //            endRange = false;
    //    //            break;
    //    //        }
    //    //        if (goodRange)
    //    //        {
    //    //            stopTime = new TimeSpan(item.HourFrom, item.MinuteFrom, 0);
    //    //            var toAdd = new StartStopTimes
    //    //            {
    //    //                StartTrainTime = startTime,
    //    //                StopTrainTime = stopTime
    //    //            };
    //    //            needTime.Add(toAdd);
    //    //            startTime = new TimeSpan(item.HourTo, item.MinuteTo, 0);
    //    //            stopTime = new TimeSpan(0,0,0);
    //    //            //item.Value.BrigadeFromId = brigadeId;
    //    //            //item.Value.BrigadeToId = brigadeId;
    //    //        }
    //    //    }

    //    //    if (endRange)
    //    //    {
    //    //        foreach (var item in targetRouteStationTrips)
    //    //        {
    //    //            if (item.StantionTripId == stationEndId)
    //    //            {
    //    //                stopTime = new TimeSpan(item.HourFrom, item.MinuteFrom, 0);
    //    //                var toAdd1 = new StartStopTimes
    //    //                {
    //    //                    StartTrainTime = startTime,
    //    //                    StopTrainTime = stopTime
    //    //                };
    //    //                needTime.Add(toAdd1);
    //    //                break;
    //    //            }

    //    //            stopTime = new TimeSpan(item.HourFrom, item.MinuteFrom, 0);
    //    //            var toAdd2 = new StartStopTimes
    //    //            {
    //    //                StartTrainTime = startTime,
    //    //                StopTrainTime = stopTime
    //    //            };
    //    //            needTime.Add(toAdd2);
    //    //            startTime = new TimeSpan(item.HourTo, item.MinuteTo, 0);
    //    //            stopTime = new TimeSpan(0, 0, 0);


    //    //        }
    //    //    }

    //    //    //теперь у нас есть needTime и блядь brigadeUsing. надо день по роуту и ид всех бригад
    //    //    var sqlRRoute = new RoutesRepository(_logger);
    //    //    var day = await sqlRRoute.GetDayByRouteId(routeId);

    //    //    //получаем все бригады
    //    //    var sqlRBrigade = new BrigadeRepository(_logger);
    //    //    var allBrigadesData = await sqlRBrigade.GetAll(0, 999999);
    //    //    var allBrigades = allBrigadesData.Data.ToList();

    //    //    //нет маршрутов в этот день = ахуительно. Правда непонятно как такое может быть
    //    //    if (!brigadeUsing.ContainsKey((DayOfWeek) day))
    //    //        return BrigadesToUi(allBrigades.ToArray());

    //    //    var toRemove = new Brigade();
    //    //    foreach (var itemBrigades in brigadeUsing[(DayOfWeek) day])
    //    //    {
    //    //        foreach (var itemTime in needTime)
    //    //        {
    //    //            if ((itemTime.StartTrainTime > itemBrigades.StartTrainTime &&
    //    //                 itemTime.StartTrainTime < itemBrigades.StopTrainTime)
    //    //                ||
    //    //                (itemTime.StopTrainTime > itemBrigades.StartTrainTime &&
    //    //                 itemTime.StopTrainTime < itemBrigades.StopTrainTime)
    //    //                )
    //    //                //foreach (var allBrigade in allBrigades)
    //    //                //{
    //    //                //    if (allBrigade.Id == itemBrigades.BrigadeId)
    //    //                //        toRemove = allBrigade;
    //    //                //}
    //    //                toRemove = allBrigades.FirstOrDefault(e => e.Id == itemBrigades.BrigadeId);
    //    //                allBrigades.Remove(toRemove);
    //    //        }
    //    //    }

    //    //    return BrigadesToUi(allBrigades.ToArray());
    //    //}

    //    public StationsTripsBrigadeToUi[] BrigadesToUi (Brigade[] input)
    //    {
    //        var output = new List<StationsTripsBrigadeToUi>();
    //        foreach (var item in input)
    //        {
    //            output.Add(new StationsTripsBrigadeToUi{BrigadeId = item.Id, BrigadeName = item.Name});
    //        }
    //        return output.ToArray();
    //    }


    //    public class BrigadeUsingTime
    //    {
    //        public int BrigadeId { get; set; }
    //    //public List<StartStopTimes> Times { get; set; }
    //        public TimeSpan StartTrainTime { get; set; }
    //        public TimeSpan StopTrainTime { get; set; }
    //}

    //    public class StartStopTimes
    //    {

    //        public TimeSpan StartTrainTime { get; set; }
    //        public TimeSpan StopTrainTime { get; set; }

    //    }

    //    public class StationsTripsBrigadeToUi
    //    {
    //        public int BrigadeId { get; set;}
    //        public string BrigadeName { get; set; }
            
    //    }

    //    public class FreeStationUi
    //    {
    //        public int StationTripId { get; set; }
    //        public string StationName { get; set; }
    //        public TimeSpan Time { get; set; }

    //    }

    //    public async Task<StationsTripsBrigadeToUi[]> GetBrigadesFromStationRange(int tripId, int stationStartId, int stationEndId)
    //    {
    //        var sqlRBrigade = new BrigadeRepository(_logger);
    //        var stationTrips = await GetByTripId(tripId);

    //        //stationTrips = stationTrips.OrderBy(e => e.HourFrom).ThenBy(e => e.MinuteFrom).ToArray();
    //        var targetStationTrips = new List<StantionTrip>();
    //        bool goodRange = false;
    //        bool endRange = true;

    //        foreach (var item in stationTrips)
    //        {
    //            if (item.Id == stationStartId)
    //            {
    //                goodRange = true;
    //                targetStationTrips.Add(item);
    //                continue;
    //            }
    //            if (goodRange && item.Id == stationEndId)
    //            {
    //                targetStationTrips.Add(item);
    //                endRange = false;
    //                break;
    //            }
    //            if (goodRange)
    //            {
    //                targetStationTrips.Add(item);
    //            }
    //        }

    //        if (endRange)
    //        {
    //            foreach (var item in stationTrips)
    //            {
    //                if (item.Id == stationEndId)
    //                {
    //                    targetStationTrips.Add(item);
    //                    break;
    //                }

    //                targetStationTrips.Add(item);
    //            }
    //        }

    //        var output = new List<StationsTripsBrigadeToUi>();
    //        foreach (var item in targetStationTrips)
    //        {
    //            //if (item.BrigadeFromId != null)
    //            //{
    //            //    var brigade = await sqlRBrigade.ByIdWithStations(item.BrigadeFromId.Value);
    //            //    var toAdd = new StationsTripsBrigadeToUi
    //            //    {
    //            //        BrigadeId = item.BrigadeFromId.Value,
    //            //        BrigadeName = brigade.Name
    //            //    };
    //            //    output.Add(toAdd);
    //            //}
    //        }
    //        return output.ToArray();
    //    }

        public void Dispose()
        {
            _db.Connection.Close();
        }

    }
}
