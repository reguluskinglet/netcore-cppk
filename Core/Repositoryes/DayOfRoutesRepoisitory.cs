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
    public class DayOfRoutesRepoisitory
    {
        private readonly ILogger _logger;
        private readonly DayOfRoutesSql _sql;

        public DayOfRoutesRepoisitory(ILogger logger)
        {
            _logger = logger;
            _sql = new DayOfRoutesSql();
        }

        public async Task<List<DayOfRoute>> DaysByTurnoverId(int turnoverId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<DayOfRoute>(_sql.DaysByTurnoverId(turnoverId));
                return result.ToList();
            }
        }

        public async Task<DayOfRoute> Add(DayOfRoute input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
                return await ById(id);
            }
        }

        public async Task<DayOfRoute> Update(DayOfRoute input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Update(input));
                return await ById(input.Id);
            }
        }

        public async Task<DayOfRoute> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<DayOfRoute>(_sql.ById(id));
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