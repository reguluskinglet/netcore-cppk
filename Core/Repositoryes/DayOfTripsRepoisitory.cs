using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Core.Repositoryes
{
    public class DayOfTripsRepoisitory
    {
        private readonly ILogger _logger;
        private readonly DayOfTripsSql _sql;

        public DayOfTripsRepoisitory(ILogger logger)
        {
            _logger = logger;
            _sql = new DayOfTripsSql();
        }

        public async Task<List<DayOfTrip>> DaysByTripId(int turnoverId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<DayOfTrip>(_sql.DaysByTripId(turnoverId));
                return result.ToList();
            }
        }

        public async Task<DayOfTrip> Add(DayOfTrip input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
                return await ById(id);
            }
        }

        public async Task<DayOfTrip> Update(DayOfTrip input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Update(input));
                return await ById(input.Id);
            }
        }

        public async Task<DayOfTrip> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<DayOfTrip>(_sql.ById(id));
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