using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Core.Extensions;
using Rzdppk.Core.Old;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public class TripsRepository  
    {

        private readonly ILogger _logger;
        private readonly TripsSql _sql;

        public TripsRepository(ILogger logger)
        {
            _logger = logger;
            _sql = new TripsSql();
        }

        public async Task<TripsPaging> GetAll(int skip, int limit, string filter)
        {
            Other.Other.CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, _sql);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<Trip>(sqlQueryData);
                var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
                var output = new TripsPaging
                {
                    Data = result.ToList(),
                    Total = count
                };

                return output;
            }
        }

        public class TripsPaging
        {
            public List<Trip> Data { get; set; }
            public int Total { get; set; }
        }

        public async Task<List<Trip>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<Trip>(_sql.Select());
                return result.ToList();
            }
        }


        public async Task<Trip> Add(Trip input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
                return await ById(id);
            }
        }

        public async Task<Trip> Update(Trip input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Update(input));
                return await ById(input.Id);
            }
        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Delete(id));
            }
        }

        public async Task<List<Trip>> GetTripsByRouteId(int routeId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new TripsSql();
                var result = await conn.QueryAsync<Trip>(sql.TripsByRouteId(routeId));

                return result.ToList();
            }
        }




        public async Task<Trip> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryFirstOrDefaultAsync<Trip>(_sql.ById(id));
                return result;
            }
        }




        //public async Task<TripPaging> GetAll(int skip, int limit, string filter = null)
        //{
        //    try
        //    {
        //        var sqlRStantionTrips = new StantionTripsRepository(_logger);
        //        var sqlRRoutes = new RoutesRepository();

        //        var sql = Sql.SqlQueryCach["Trips.All"];
        //        TripRoutes[] result;
        //        if (filter == null)
        //        {
        //            result = (await _db.Connection.QueryAsync<TripRoutes>(sql, new {skip = skip, limit = limit})).ToArray();
        //        }
        //        else
        //        {
        //            result = (await _db.Connection.QueryAsync<TripRoutes>(sql, new {skip = 0, limit = 9999999})).ToArray();
        //        }
        //        var sqlc = Sql.SqlQueryCach["Trips.CountAll"];
        //        var count = (await _db.Connection.QueryAsync<int>(sqlc)).FirstOrDefault();

        //        var outputUi = new List<TripsUi>();

        //        foreach (var value in result)
        //        {
        //            //value.StantionTrips = await sqlRStantionTrips.GetByTripId(value.Id);
        //            value.Routes = await sqlRRoutes.GetByTripId(value.Id);
        //            if (value.Routes.Length == 0)
        //                continue;
        //            var tripUi = new TripsUi();
        //            tripUi.Id = value.Id;
        //            tripUi.TripName = value.Name;
        //            //tripUi.Day = value.Day;
        //            tripUi.RouteName = value.Routes.FirstOrDefault().Name;
        //            tripUi.RouteId = value.Routes.FirstOrDefault().Id;
        //            tripUi.Stantion = new List<StantionTripUi>();

        //            if (filter != null)
        //            {
        //                var filters = JsonConvert.DeserializeObject<Other.Other.FilterBody[]>(filter);
        //                var isFiltered = false;
        //                for (var index = 0; index < filters.Length; index++)
        //                {
        //                    var item = filters[index];
        //                    switch (item.Filter)
        //                    {
        //                        //case "Day":
        //                        //    if (!item.Value.Equals(value.Day.ToString()))
        //                        //        isFiltered = true;
        //                        //    break;
        //                        case "RouteId":
        //                            if (!item.Value.Equals(tripUi.RouteId.ToString()))
        //                                isFiltered = true;
        //                            break;

        //                    }
        //                }
        //                if (isFiltered == true)
        //                    continue;
        //            }
        //                //сартировка гаавна
        //                //item.StantionTrips = item.StantionTrips.OrderBy(e => e.HourFrom).ThenBy(e => e.MinuteFrom).ToArray();

        //            //foreach (var station in value.StantionTrips)
        //            //{
        //            //    var stationUi = new StantionTripUi();
        //            //    stationUi.Id = station.Id;
        //            //    stationUi.Name = station.Stantion.Name;


        //            //    //время прибытия, отхуития и простоя.
        //            //    stationUi.ArrivalTime = new TimeSpan(station.HourFrom,station.MinuteFrom,0);
        //            //    stationUi.DepartmentTime = new TimeSpan(station.HourTo, station.MinuteTo, 0); 
        //            //    var downtimeMinutes = station.MinuteTo - station.MinuteFrom;
        //            //    var downtimeHours = station.HourTo - station.HourFrom;
        //            //    if (stationUi.ArrivalTime > stationUi.DepartmentTime)
        //            //        stationUi.Downtime = stationUi.ArrivalTime - stationUi.DepartmentTime;
        //            //    else
        //            //        stationUi.Downtime = stationUi.DepartmentTime - stationUi.ArrivalTime;

        //            //    if (station.CheckListType != null)
        //            //        stationUi.CheckList = station?.CheckListType.Value;
        //            //    else
        //            //        stationUi.CheckList = null;

        //            //    tripUi.Stantion.Add(stationUi);
        //            //}

        //            outputUi.Add(tripUi);
        //        }

        //        var outputUiPaging = new List<TripsUi>();

        //        var output = new TripPaging()
        //        {
        //            Data = outputUi.ToArray(),
        //            Total = count
        //        };

        //        if (filter != null)
        //        {
        //            output.Total = outputUi.Count;
        //            limit = skip + limit;
        //            if (limit > output.Total)
        //                limit = output.Total;
        //            if (skip == 0)
        //                skip++;

        //            for (var index = skip - 1; index < limit; index++)
        //            {
        //                outputUiPaging.Add(output.Data[index]);
        //            }
        //            output.Data = outputUiPaging.ToArray();
        //        }

        //        sqlRStantionTrips.Dispose();
        //        sqlRRoutes.Dispose();

        //        return output;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogTrace($"Exception on GetAll, e = {e.Data}");
        //        throw new Exception(e.Message);
        //    }
        //}

        public class StantionTripSort : StantionTrip
        {
            public TimeSpan TimeFrom { get; set; }
        }

        //public async Task<TripBrigadePaging> GetAllBrigades(int skip, int limit)
        //{
        //    var sql = Sql.SqlQueryCach["Trips.AllBrigades"];
        //    var result = (await _db.Connection.QueryAsync<TripsBrigadesSql>(sql)).ToList();
        //    //var sqlc = Sql.SqlQueryCach["Trips.CountAllBrigades"];
        //    //var count = _db.Connection.ExecuteScalar<int>(sqlc);

        //    var resultUi = new List<TripsBrigadesUi>();
        //    var sqlRBrigade = new BrigadeRepository(_logger);

        //    //result = result.OrderBy(e => e.RouteId).ThenBy(e => e.HourFrom).ThenBy(e => e.MinuteFrom).ToArray();

        //    var routeDictionary = new Dictionary<string, List<TripsBrigadesSql>>();
        //    foreach (var item in result)
        //    {
        //        if (!routeDictionary.ContainsKey(item.RouteName))
        //            routeDictionary.Add(item.RouteName, new List<TripsBrigadesSql>());

        //        routeDictionary[item.RouteName].Add(item);
        //    }

        //    foreach (var tripBrigades in routeDictionary.Values)
        //    {

        //        //if (tripBrigades.Count > 0 && tripBrigades.FirstOrDefault().BrigadeToId != null)
        //        //AddToOutputGetAllBrigades(resultUi, sqlRBrigade, tripBrigades, tripBrigades.FirstOrDefault());
        //        foreach (var tripBrigade in tripBrigades)
        //        {
        //            //if (tripBrigade.BrigadeFromId == null)
        //            //  continue;
        //            await AddToOutputGetAllBrigades(resultUi, sqlRBrigade, tripBrigades, tripBrigade);
        //        }
        //    }

        //    var blyad = new List<TripsBrigadesUi>();

        //    for (var index = 0; index < resultUi.Count; index++)
        //    {
        //        var item = resultUi[index];
        //        if (index == 0)
        //        {
        //            blyad.Add(item);
        //            continue;
        //        }

        //        if (item.RouteId == resultUi[index - 1].RouteId &&
        //            item.Day == resultUi[index - 1].Day &&
        //            item.Brigade.Id == resultUi[index - 1].Brigade.Id)
        //        {
        //            blyad.LastOrDefault().Output = item.Output;
        //            blyad.LastOrDefault().OutputStationTripId = item.OutputStationTripId;
        //            blyad.LastOrDefault().TripTime = blyad.LastOrDefault().TripTime + item.TripTime;
        //            continue;
        //        }

        //        blyad.Add(item);
        //    }

        //    var afterPaging = new List<TripsBrigadesUi>();

        //    limit = skip + limit;
        //    if (limit > blyad.Count) limit = blyad.Count;


        //    for (var index = skip; index < limit; index++)
        //    {
        //        var item = blyad[index];
        //        afterPaging.Add(item);
        //    }


        //    var output = new TripBrigadePaging()
        //    {
        //        Data = afterPaging.ToArray(),
        //        Total = resultUi.Count
        //    };

        //    sqlRBrigade.Dispose();

        //    return output;
        //}

        //private async Task AddToOutputGetAllBrigades(List<TripsBrigadesUi> resultUi, BrigadeRepository sqlRBrigade, List<TripsBrigadesSql> tripBrigades, TripsBrigadesSql tripBrigade)
        //{
        //    var toUi = new TripsBrigadesUi();
        //    toUi.RouteName = tripBrigade.RouteName;
        //    toUi.RouteId = tripBrigade.RouteId;
        //    if (tripBrigade.BrigadeToId == null)
        //        return;
        //    toUi.Brigade = await sqlRBrigade.ByIdWithStations(tripBrigade.BrigadeToId.Value);

        //    toUi.Input = $"{tripBrigade.StationName} {tripBrigade.HourTo}:{tripBrigade.MinuteTo}";
        //    var nextStation = GetNextStation(tripBrigade, tripBrigades.ToArray());

        //    toUi.InputStationTripId = tripBrigade.StantionTripId;

        //    if (nextStation != null)
        //    {
        //        toUi.OutputStationTripId = nextStation.StantionTripId;
        //        toUi.Output = $"{nextStation?.StationName} {nextStation?.HourFrom}:{nextStation?.MinuteFrom}";

        //        var timeStartFromCurrentStation = new TimeSpan(tripBrigade.HourTo, tripBrigade.MinuteTo, 0);
        //        var timeArrivalToNextStation = new TimeSpan(nextStation.HourFrom, nextStation.MinuteFrom, 0);

        //        var qq = new TimeSpan(tripBrigade.HourTo, 0, 0);
        //        var cc = timeStartFromCurrentStation.TotalSeconds - qq.TotalSeconds;
        //        var ww = timeArrivalToNextStation.TotalSeconds - qq.TotalSeconds;

        //        var travelTime2 = 86400 - Math.Abs(cc - ww);

        //        //var timeStartFromCurrentStationwc = 
        //        //var timeArrivalToNextStationwc = 

        //        var travelTime = TimeSpan.FromSeconds(Math.Abs(cc - ww));
        //        if (travelTime2 < travelTime.TotalSeconds)
        //            travelTime = TimeSpan.FromSeconds(travelTime2);

        //        toUi.TripTime = travelTime;

        //        toUi.Day = tripBrigade.Day;

        //        if (!tripBrigade.StationName.Equals(nextStation.StationName))
        //            resultUi.Add(toUi);
        //    }
        //}

        //public TripsBrigadesSql GetNextStation(TripsBrigadesSql current, TripsBrigadesSql[] all)
        //{
        //    var sortAll = all.OrderBy(e => e.HourFrom).ThenBy(e => e.MinuteFrom);

        //    //var nextStationTripId = 0;
        //    //var currentStationTime = new TimeSpan(current.HourFrom, current.MinuteFrom, 0);
        //    //TimeSpan foreachStationTime;
        //    for (var index = 0; index < all.Length; index++)
        //    {
        //        var item = all[index];
        //        //if (nextStationTripId == 0)
        //        //{
        //        //    nextStationTripId = item.StantionTripId;
        //        //    foreachStationTime = new TimeSpan(item.HourFrom, item.MinuteFrom, 0);
        //        //}
        //        if (item.StantionTripId == current.StantionTripId)
        //            if (index == all.Length - 1)
        //                return all[0];
        //            else
        //                return all[index + 1];

        //        //var currentStationTimeFrom = new TimeSpan(item.HourFrom, item.MinuteFrom, 0);
        //        //if (currentStationTimeFrom > currentStationTime && currentStationTimeFrom < foreachStationTime)
        //        //    nextStationTripId = item.StantionTripId;
        //    }
        //    //var nextStation = all.FirstOrDefault(item => item.StantionTripId == nextStationTripId);
        //    //if (nextStation != null && nextStation.BrigadeFromId == current.BrigadeToId)
        //    //    return nextStation;

        //    return null;
        //}


        //public class TripsBrigadesSql
        //{
        //    public string RouteName { get; set; }
        //    public int RouteId { get; set; }
        //    public int? BrigadeFromId { get; set; }
        //    public int? BrigadeToId { get; set; }
        //    public int HourFrom { get; set; }
        //    public int MinuteFrom { get; set; }
        //    public int HourTo { get; set; }
        //    public int MinuteTo { get; set; }
        //    public int StantionTripId { get; set; }
        //    public DayOfWeek Day { get; set; }
        //    public string StationName { get; set; }
        //    public int TripId { get; set; }
        //    public TimeSpan TimeFrom { get; set; }

        //}

        //public class TripsBrigadesUi
        //{
        //    public string RouteName { get; set; }
        //    public Brigade Brigade { get; set; }
        //    public string Input { get; set; }
        //    public int InputStationTripId { get; set; }
        //    public string Output { get; set; }
        //    public int OutputStationTripId { get; set; }
        //    public int StantionTripId { get; set; }
        //    public DayOfWeek Day { get; set; }
        //    public TimeSpan TripTime { get; set; }
        //    public int RouteId { get; set; }

        //}

        //public class StantionTripUi
        //{
        //    public int Id { get; set; }
        //    public string Name { get; set; }
        //    public TimeSpan ArrivalTime { get; set; }
        //    public TimeSpan DepartmentTime { get; set; }
        //    public TimeSpan Downtime { get; set; }
        //    public CheckListType? CheckList { get; set; }

        //}

        //public class TripRoutes : Trip
        //{
        //    public Route[] Routes { get; set; }
        //    public TimeSpan TimeFrom { get; set; }

        //}

        //public class TripsUi
        //{
        //    public int Id { get; set; }
        //    public string TripName { get; set; }
        //    public DayOfWeek Day { get; set; }
        //    public List<StantionTripUi> Stantion { get; set; }
        //    public string RouteName { get; set; }
        //    public int RouteId { get; set; }
        //}

        //public class TripBrigadePaging
        //{
        //    public TripsBrigadesUi[] Data { get; set; }
        //    public int Total { get; set; }

        //}

        //public class TripPaging
        //{
        //    public TripsUi[] Data { get; set; }
        //    public int Total { get; set; }

        //}


        //public int ConvertToSeconds(int hours, int minutes)
        //{
        //    var second = hours * 60 * 60 + minutes * 60;
        //    return second;
        //}


        ////public Dictionary<int, int> ConvertPermissionsToUi (int input)
        ////{
        ////    Dictionary<int, int> output = new Dictionary<int, int>();

        ////    var binary = Convert.ToString(input, 2);
        ////    var binaryCharArray = binary.ToCharArray();

        ////    var count = 0;
        ////    foreach (var binaryChar in binaryCharArray)
        ////    {
        ////        int.TryParse(binaryChar.ToString(), out var intBit);
        ////        output.Add(count, intBit);
        ////        count++;
        ////    }

        ////    return output;
        ////}


        ////public  int ConvertPermissionsToInt(Dictionary<int, int> input)
        ////{
        ////    string stringToConvert = null;
        ////    var reverce = input.Reverse();
        ////    foreach (var item in reverce)
        ////    { 
        ////        stringToConvert = $"{stringToConvert}{item.Value}";
        ////    }
        ////    //var wcwc = Convert.ToInt32(qweqwe, 2);
        ////    var binary = Convert.ToInt32(stringToConvert, 2);

        ////    var stringInt = Convert.ToString(binary, 10);

        ////    int.TryParse(stringInt, out var output);

        ////    return output;
        ////}


        ////public class UserRoleUi
        ////{
        ////    public UserRole Role { get; set;}
        ////    public Dictionary<int,int> PermissionsArray { get; set; }

        ////}

        ////public UserRole ByIdWithStations(int id)
        ////{
        ////    var sql = Sql.SqlQueryCach["UserRole.ById"];
        ////    var result = _db.Connection.Query<UserRole>(sql, new { id = id });
        ////    return result.FirstOrDefault();
        ////}

        //////ебохардкод
        ////public Dictionary<int,string> GetAuthorityArray()
        ////{
        ////    var result = new Dictionary<int,string>();
        ////    result.Add(0, "Доступ в журнал");
        ////    result.Add(1, "Доступ в задачи");
        ////    result.Add(2, "Доступ в расписание");
        ////    result.Add(3, "Доступ в справочники");
        ////    result.Add(4, "Доступ в метки");
        ////    result.Add(5, "Доступ в отчеты");

        ////    return result;
        ////}

        ////public async Task<int> Add(UserRoleUi input)
        ////{
        ////    input.Role.Permissions = ConvertPermissionsToInt(input.PermissionsArray);
        ////    var id =  _db.Connection.Query<int>(Sql.SqlQueryCach["UserRole.Add"], new { name = input.Role.Name, permissions = input.Role.Permissions });
        ////    return id.FirstOrDefault();
        ////}

        ////public async void AddUpdateUserRole(UserRoleUi input)
        ////{
        ////    if (input.Role.Id == 0)
        ////        await Add(input);
        ////    else
        ////    {
        ////        input.Role.Permissions = ConvertPermissionsToInt(input.PermissionsArray);
        ////        _db.Connection.Execute(Sql.SqlQueryCach["UserRole.Update"], new { id = input.Role.Id, name = input.Role.Name, permissions = input.Role.Permissions });
        ////    }

        ////}



        ////public  void Delete(int id)
        ////{
        ////    var sql = Sql.SqlQueryCach["UserRole.Delete"];
        ////    _db.Connection.Execute(sql, new { id = id });
        ////}


        //public void Dispose()
        //{
        //    _db.Connection.Close();
        //}

    }
}
