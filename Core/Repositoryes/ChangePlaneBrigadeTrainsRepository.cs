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
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes
{
    public class ChangePlaneBrigadeTrainsRepository
    {

        private readonly ILogger _logger;
        private readonly ChangePlaneBrigadeTrainsSql _sql;

        public ChangePlaneBrigadeTrainsRepository(ILogger logger)
        {
            _logger = logger;
            _sql = new ChangePlaneBrigadeTrainsSql();
        }

        public async Task<ChangePlaneBrigadeTrainPaging> GetAll(int skip, int limit, string filter)
        {
            CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, _sql);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<ChangePlaneBrigadeTrain>(sqlQueryData);
                var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
                var output = new ChangePlaneBrigadeTrainPaging()
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

        public class ChangePlaneBrigadeTrainPaging
        {
            public List<ChangePlaneBrigadeTrain> Data { get; set; }
            public int Total { get; set; }
        }

        public async Task<ChangePlaneBrigadeTrain> Add(ChangePlaneBrigadeTrain input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
                return await ById(id);
            }
        }

        public async Task<ChangePlaneBrigadeTrain> Update(ChangePlaneBrigadeTrain input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Update(input));
                return await ById(input.Id);
            }
        }

        public async Task<ChangePlaneBrigadeTrain> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<ChangePlaneBrigadeTrain>(_sql.ById(id));
            }
        }

        public async Task<ChangePlaneBrigadeTrain> ByPlaneBrigadeTrainId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<ChangePlaneBrigadeTrain>(_sql.ByPlaneBrigadeTrainId(id));
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
