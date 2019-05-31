using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.PlaneBrigadeTrain;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes
{
    public class PlanedInspectionRoutesRepository
    {

        private readonly ILogger _logger;
        private readonly string _table;
        private readonly PlanedInspectionRoutesSql _sql;

        public PlanedInspectionRoutesRepository(ILogger logger)
        {
            _logger = logger;
            _sql = new PlanedInspectionRoutesSql();
            _table = "PlanedInspectionRoutes";
        }

        public async Task<PlanedInspectionRoutePaging> GetAll(int skip, int limit, string filter)
        {
            CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, _sql);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<PlanedInspectionRoute>(sqlQueryData);
                var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
                var output = new PlanedInspectionRoutePaging
                {
                    Data = result.ToList(),
                    Total = count
                };

                return output;
            }
        }

        public class PlanedInspectionRoutePaging
        {
            public List<PlanedInspectionRoute> Data { get; set; }
            public int Total { get; set; }
        }

        public async Task<PlanedInspectionRoute> Add(PlanedInspectionRoute input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
                return await ById(id);
            }
        }

        public async Task<PlanedInspectionRoute> Update(PlanedInspectionRoute input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Update(input));
                return await ById(input.Id);
            }
        }

        public async Task<PlanedInspectionRoute> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<PlanedInspectionRoute>(_sql.ById(id));
            }
        }

        public async Task<PlanedInspectionRoute> ByTrainId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<PlanedInspectionRoute>(CommonSql.ByPropertyId(_table, "PlanedRouteTrainId", id));
            }
        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Delete(id));
            }
        }

        public async Task<List<PlanedInspectionRoute>> ByPlanedRouteTrainId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<PlanedInspectionRoute>(_sql.ByPlanedRouteTrainId(id));
                return result.ToList();
            }
        }


    }
}