using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Core.Extensions;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Label;
using Rzdppk.Core.Repositoryes.Sqls.Routes;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using Rzdppk.Core.Services;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes
{
    public class RoutesRepository
    {

        private readonly ILogger _logger;
        private readonly RoutesSql _sql;

        public RoutesRepository(ILogger logger)
        {
            _logger = logger;
            _sql = new RoutesSql();
        }

        public async Task<RoutesPaging> GetAll(int skip, int limit, string filter)
        {
            CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, _sql);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<Route>(sqlQueryData);
                var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
                var output = new RoutesPaging
                {
                    Data = result.ToList(),
                    Total = count
                };

                return output;
            }
        }

        public class RoutesPaging
        {
            public List<Route> Data { get; set; }
            public int Total { get; set; }
        }

        public async Task<Route> Add(Route input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
                return await ById(id);
            }
        }

        public async Task<Route> Update(Route input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Update(input));
                return await ById(input.Id);
            }
        }

        public async Task<Route> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<Route>(_sql.ById(id));
            }
        }

        public async Task<List<Route>> WithoutTurnover()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return (await conn.QueryAsync<Route>(_sql.WithoutTurnover())).ToList();
            }
        }

        public async Task ClearTurnoverId(int routeId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.ClearTurnoverId(routeId));
            }
        }


        public async void Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Delete(id));
            }
        }



        public async Task<RoutesByTurnoverIdPaging> GetByTurnoverIdPaging(int turnoverId, int skip, int limit)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new RoutesSql();
                var routes = await conn.QueryAsync<Route>(sql.GetByTurnoverIdPaging(turnoverId, skip, limit));
                var count = await conn.ExecuteScalarAsync<int>(_sql.CountByTurnoverId(turnoverId));

                return new RoutesByTurnoverIdPaging { Data = routes.ToList(), Total = count };
            }

        }

        public async Task<List<Route>> ByTurnoverId(int turnoverId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new RoutesSql();
                var result = await conn.QueryAsync<Route>(sql.ByTurnoverId(turnoverId));
                return result.ToList();
            }

        }

        public class RoutesByTurnoverIdPaging
        {
            public List<Route> Data { get; set; }
            public int Total { get; set; }
        }



    }
}
