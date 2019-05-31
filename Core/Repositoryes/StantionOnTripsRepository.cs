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
    public class StantionOnTripsRepository
    {
        private readonly ILogger _logger;

        public StantionOnTripsRepository(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<List<StantionOnTrip>> ByTripId(int tripId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new StantionOnTripsSql();
                var result = await conn.QueryAsync<StantionOnTrip>(sql.StationsOnTripByTripId(tripId));
                return result.ToList();
            }
        }

        public async Task<StantionOnTrip> Add(StantionOnTrip input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new StantionOnTripsSql();
                var id = await conn.QueryFirstOrDefaultAsync<int>(sql.Add(input));
                return await ById(id);
            }
        }

        public async Task<StantionOnTrip> Update(StantionOnTrip input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new StantionOnTripsSql();
                await conn.ExecuteAsync(sql.Update(input));
                return await ById(input.Id);
            }
        }

        public async Task<StantionOnTrip> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new StantionOnTripsSql();
                return await conn.QueryFirstOrDefaultAsync<StantionOnTrip>(sql.ById(id));
            }
        }

        public async void Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new StantionOnTripsSql();
                await conn.ExecuteAsync(sql.Delete(id));
            }
        }
    }





}