using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using Rzdppk.Core.Repositoryes.Sqls.Turnovers;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes
{
    public class TurnoversRepoisitory
    {
        private readonly ILogger _logger;

        public TurnoversRepoisitory(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<TurnoversPaging> GetAll(int skip, int limit, string filter)
        {
            var sql = new TurnoversSql();
            CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, sql);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<Turnover>(sqlQueryData);
                var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
                var output = new TurnoversPaging
                {
                    Data = result.ToList(),
                    Total = count
                };

                return output;
            }
        }



        public async Task<Turnover> Add(Turnover turnover)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new TurnoversSql();
                var id = await conn.QueryFirstOrDefaultAsync<int>(sql.Add(turnover.DirectionId, turnover.Name));
                return await ById(id);
            }
        }

        public async Task<Turnover> Update(Turnover turnover)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new TurnoversSql();
                await conn.ExecuteAsync(sql.Update(turnover.DirectionId, turnover.Name, turnover.Id));
                return await ById(turnover.Id);
            }
        }

        public async Task<Turnover> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new TurnoversSql();
                return await conn.QueryFirstOrDefaultAsync<Turnover>(sql.ById(id));
            }
        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new TurnoversSql();
                await conn.ExecuteAsync(sql.Delete(id));
            }
        }

    }




    public class TurnoversPaging
    {
        public List<Turnover> Data { get; set; }
        public int Total { get; set; }
    }
}