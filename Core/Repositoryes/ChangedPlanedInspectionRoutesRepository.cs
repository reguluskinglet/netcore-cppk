using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.PlanedRouteTrains;
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes
{
    public class ChangedPlanedInspectionRoutesRepository
    {

        private readonly ILogger _logger;
        private readonly ChangedPlanedInspectionRoutesSql _sql;

        public ChangedPlanedInspectionRoutesRepository(ILogger logger)
        {
            _logger = logger;
            _sql = new ChangedPlanedInspectionRoutesSql();
        }

        public async Task<ChangedPlanedInspectionRoutePaging> GetAll(int skip, int limit, string filter)
        {
            CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, _sql);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<ChangedPlanedInspectionRoute>(sqlQueryData);
                var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
                var output = new ChangedPlanedInspectionRoutePaging()
                {
                    Data = result.ToList(),
                    Total = count
                };

                return output;
            }
        }

        public async Task<List<ChangedPlanedInspectionRoute>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<ChangedPlanedInspectionRoute>(_sql.GetAll());
                return result.ToList();
            }
        }

        public class ChangedPlanedInspectionRoutePaging
        {
            public List<ChangedPlanedInspectionRoute> Data { get; set; }
            public int Total { get; set; }
        }

        public async Task<ChangedPlanedInspectionRoute> Add(ChangedPlanedInspectionRoute input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
                return await ById(id);
            }
        }

        public async Task<ChangedPlanedInspectionRoute> Update(ChangedPlanedInspectionRoute input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Update(input));
                return await ById(input.Id);
            }
        }

        public async Task<ChangedPlanedInspectionRoute> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<ChangedPlanedInspectionRoute>(_sql.ById(id));
            }
        }

        public async Task<ChangedPlanedInspectionRoute> ByPlanedInspectionRouteId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<ChangedPlanedInspectionRoute>(_sql.ByPlanedInspectionRouteId(id));
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
