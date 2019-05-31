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
using Rzdppk.Core.Repositoryes.Sqls.PlanedRouteTrains;
using Rzdppk.Core.Repositoryes.Sqls.Routes;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using Rzdppk.Core.Services;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes
{
    public class PlanedRouteTrainsRepository
    {

        private readonly ILogger _logger;
        private readonly PlanedRouteTrainsSql _sql;
        private readonly string _table;

        public PlanedRouteTrainsRepository(ILogger logger)
        {
            _logger = logger;
            _sql = new PlanedRouteTrainsSql();
            _table = "PlanedRouteTrains";
        }

        public async Task<PlanedRouteTrainPaging> GetAll(int skip, int limit, string filter)
        {
            CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, _sql);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<PlanedRouteTrain>(sqlQueryData);
                var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
                var output = new PlanedRouteTrainPaging
                {
                    Data = result.ToList(),
                    Total = count
                };

                return output;
            }
        }

        public async Task<List<PlanedRouteTrain>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<PlanedRouteTrain>(_sql.GetAll());
                return result.ToList();
            }
        }

        public class PlanedRouteTrainPaging
        {
            public List<PlanedRouteTrain> Data { get; set; }
            public int Total { get; set; }
        }

        public async Task<PlanedRouteTrain> Add(PlanedRouteTrain input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
                return await ById(id);
            }
        }

        //public async Task<Route> Update(Route input)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        await conn.ExecuteAsync(_sql.Update(input));
        //        return await ById(input.Id);
        //    }
        //}

        public async Task<List<PlanedRouteTrain>> ByTrainId(int trainId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var data = await conn.QueryAsync<PlanedRouteTrain>(CommonSql.ByPropertyId(_table, "TrainId", trainId));
                return data.ToList();
            }
        }

        public async Task<PlanedRouteTrain> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<PlanedRouteTrain>(_sql.ById(id));
            }
        }

        public async Task<List<PlanedRouteTrain>> ByUserId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return (await conn.QueryAsync<PlanedRouteTrain>(_sql.ByUserId(id))).ToList();
            }
        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Delete(id));
            }
        }



        //public async Task<RoutesByTurnoverIdPaging> GetByTurnoverIdPaging(int turnoverId, int skip, int limit)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        var sql = new RoutesSql();
        //        var routes = await conn.QueryAsync<Route>(sql.GetByTurnoverIdPaging(turnoverId, skip, limit));
        //        var count = await conn.ExecuteScalarAsync<int>(_sql.CountByTurnoverId(turnoverId));

        //        return new RoutesByTurnoverIdPaging{Data = routes.ToList(), Total = count};
        //    }

        //}

        //public class RoutesByTurnoverIdPaging
        //{
        //    public List<Route> Data { get; set; }
        //    public int Total { get; set; }
        //}



    }
}
