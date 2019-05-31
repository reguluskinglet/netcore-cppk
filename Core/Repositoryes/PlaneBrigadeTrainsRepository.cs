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
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes
{
    public class PlaneBrigadeTrainsRepository
    {

        private readonly ILogger _logger;
        private readonly String _table;
        private readonly PlaneBrigadeTrainSql _sql;

        public PlaneBrigadeTrainsRepository(ILogger logger)
        {
            _logger = logger;
            _sql = new PlaneBrigadeTrainSql();
            _table = "PlaneBrigadeTrains";
        }

        public async Task<List<PlaneBrigadeTrain>> ByUserId(int userId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var data = await conn.QueryAsync<PlaneBrigadeTrain>(CommonSql.ByPropertyId(_table, "UserId", userId));
                return data.ToList();
            }
        }

        public async Task<List<PlaneBrigadeTrain>> ByPlanedRouteTrainId (int planedRouteTrainId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var data = await conn.QueryAsync<PlaneBrigadeTrain>(CommonSql.ByPropertyId(_table, "PlanedRouteTrainId", planedRouteTrainId));
                return data.ToList();
            }
        }

        public async Task<PlaneBrigadeTrainsPaging> GetAll(int skip, int limit, string filter)
        {
            CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, _sql);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<PlaneBrigadeTrain>(sqlQueryData);
                var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
                var output = new PlaneBrigadeTrainsPaging
                {
                    Data = result.ToList(),
                    Total = count
                };

                return output;
            }
        }

        public class PlaneBrigadeTrainsPaging
        {
            public List<PlaneBrigadeTrain> Data { get; set; }
            public int Total { get; set; }
        }

        public async Task<PlaneBrigadeTrain> Add(PlaneBrigadeTrain input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
                return await ById(id);
            }
        }

        public async Task<PlaneBrigadeTrain> Update(PlaneBrigadeTrain input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Update(input));
                return await ById(input.Id);
            }
        }

        public async Task<PlaneBrigadeTrain> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<PlaneBrigadeTrain>(_sql.ById(id));
            }
        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Delete(id));
            }
        }

        //public async Task<List<PlaneBrigadeTrain>> ByPlannedRouteTrainId(int id)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        var result = await conn.QueryAsync<PlaneBrigadeTrain>(_sql.ByPlannedRouteTrainId(id));
        //        return result.ToList();
        //    }
        //}


    }
}