using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.StantionOnTrips;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Core.Repositoryes
{
    public class PlanedStationOnTripsRepository
    {
        private readonly ILogger _logger;
        private readonly PlaneStantionOnTripsSql _sql;

        public PlanedStationOnTripsRepository(ILogger logger)
        {
            _logger = logger;
            _sql = new PlaneStantionOnTripsSql();
        }

        public async Task<List<PlaneStantionOnTrip>> ByTripId(int tripId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<PlaneStantionOnTrip>(_sql.StationsOnTripByTripId(tripId));
                return result.ToList();
            }
        }

        public async Task<List<PlaneStantionOnTrip>> ByPlannedRouteTrainId(int tripId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<PlaneStantionOnTrip>(_sql.ByPlanedRouteTrainId(tripId));
                return result.ToList();
            }
        }

        //TODO перенести
        public async Task<List<PlaneBrigadeTrain>> ByUserIdAndTimeRange(int userId, DateTime startTime, DateTime endTime)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<PlaneBrigadeTrain>(_sql.ByUserIdAndTimeRange(userId, startTime, endTime));
                return result.ToList();
            }
        }

        //public async Task<PlaneStantionOnTrip> ByPlanedRouteTrainIdAndStationId(int planedRouteTrainId, int stationId)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        var result =
        //            await conn.QueryFirstOrDefaultAsync<PlaneStantionOnTrip>(
        //                _sql.ByPlanedRouteTrainIdAndStationId(planedRouteTrainId, stationId));
        //        return result;
        //    }
        //}

        public async Task<PlaneStantionOnTrip> Add(PlaneStantionOnTrip input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
                return await ById(id);
            }
        }

        //public async Task<StantionOnTrip> Update(StantionOnTrip input)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        var sql = new StantionOnTripsSql();
        //        await conn.ExecuteAsync(sql.Update(input));
        //        return await ById(input.Id);
        //    }
        //}

        public async Task<PlaneStantionOnTrip> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<PlaneStantionOnTrip>(_sql.ById(id));
            }
        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Delete(id));
            }


        }
    }
}