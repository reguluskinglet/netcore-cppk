using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Sqls.InspectionRoutes;
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes
{
    public class InspectionRoutesRepository
    {
        private readonly ILogger _logger;

        public InspectionRoutesRepository(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<List<InspectionRoute>> GetByRouteId(int routeId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new InspectionRoutesSql();
                var result = await conn.QueryAsync<InspectionRoute>(sql.GetByRouteId(routeId));
                return result.ToList();
            }
        }

        public async Task<InspectionRoutesPaging> GetAll(int skip, int limit, string filter)
        {
            var sql = new InspectionRoutesSql();
            CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, sql);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<InspectionRoute>(sqlQueryData);
                var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
                var output = new InspectionRoutesPaging
                {
                    Data = result.ToList(),
                    Total = count
                };

                return output;
            }
        }

        public class InspectionRoutesPaging
        {
            public List<InspectionRoute> Data { get; set; }
            public int Total { get; set; }
        }

        public async Task<InspectionRoute> Add(InspectionRoute inspectionOnRoute)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new InspectionRoutesSql();
                var id = await conn.QueryFirstOrDefaultAsync<int>(sql.Add(inspectionOnRoute.RouteId, inspectionOnRoute.Start, inspectionOnRoute.End, (int?)inspectionOnRoute.CheckListType));
                return await ById(id);
            }
        }

        public async Task<InspectionRoute> Update(InspectionRoute inspectionRoute)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new InspectionRoutesSql();
                await conn.ExecuteAsync(sql.Update(inspectionRoute.RouteId, inspectionRoute.Start, inspectionRoute.End, (int?) inspectionRoute.CheckListType, inspectionRoute.Id));
                return await ById(inspectionRoute.Id);
            }
        }

        public async Task<InspectionRoute> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new InspectionRoutesSql();
                return await conn.QueryFirstOrDefaultAsync<InspectionRoute>(sql.ById(id));
            }
        }

        public async void Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new InspectionRoutesSql();
                await conn.ExecuteAsync(sql.Delete(id));
            }
        }


    }





}