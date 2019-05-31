using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Newtonsoft.Json;
using Rzdppk.Core.Options;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Brigade;
using Rzdppk.Model;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes
{
    public class BrigadeRepository : IBrigadeRepository
    {
        private readonly IDb _db;
        private readonly ILogger _logger;

        public BrigadeRepository(ILogger logger)
        {
            _db = new Db();
        }

        /// <summary>
        /// Получить все с пагинацией
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<BrigadePaging> GetAll(int skip, int limit)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Brigade.All"];
                var result = await conn.QueryAsync<Brigade>(sql, new { skip = skip, limit = limit });
                var sqlc = Sql.SqlQueryCach["Brigade.CountAll"];
                var count = conn.ExecuteScalar<int>(sqlc);
                var output = new BrigadePaging()
                {
                    Data = result.ToArray(),
                    Total = count
                };

                return output;
            }
        }

        /// <summary>
        /// Получить все
        /// </summary>
        /// <returns></returns>
        public async Task<List<Brigade>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Brigade.All"];
                var result = await conn.QueryAsync<Brigade>(sql, new { skip = 0, limit = int.MaxValue });
                return result.ToList();
            }
        }

        /// <summary>
        /// Получить все с пагинацией
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public BrigadePaging GetAllSync(int skip, int limit)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Brigade.All"];
                var result = conn.Query<Brigade>(sql, new { skip = skip, limit = limit });
                var sqlc = Sql.SqlQueryCach["Brigade.CountAll"];
                var count = conn.ExecuteScalar<int>(sqlc);
                var output = new BrigadePaging()
                {
                    Data = result.ToArray(),
                    Total = count
                };

                return output;
            }
        }

        public async Task<BrigadePaging> GetAll(int skip, int limit, string filter)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                CreateFilter(filter, out var sqlfilter, out var sql);
                var result = await conn.QueryAsync<Brigade>(sql, new { skip = skip, limit = limit });
                var sqlc = $"{BrigadeCommon.sqlCountCommon} {sqlfilter}";
                var count = conn.ExecuteScalar<int>(sqlc);
                var output = new BrigadePaging()
                {
                    Data = result.ToArray(),
                    Total = count
                };

                return output;
            }
        }

        private static void CreateFilter(string filter, out string sqlfilter, out string sql)
        {

            var filters = JsonConvert.DeserializeObject<FilterBody[]>(filter);
            sqlfilter = "where ";
            for (var index = 0; index < filters.Length; index++)
            {
                var item = filters[index];
                sqlfilter = $"{sqlfilter} {item.Filter} like '%{item.Value}%' ";
                if (index < (filters.Length - 1))
                    sqlfilter = $"{sqlfilter} AND ";

            }

            sql = $"{BrigadeCommon.sqlCommon} {sqlfilter} {SqlQueryPagingEnd}";
        }

        public async Task<Brigade> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Brigade.ById"];
                var result = await conn.QueryFirstOrDefaultAsync<Brigade>(sql, new { brigade_id = id });
                return result;
            }
        }

        public async Task Update(Brigade input)
        {
            var current = await ById(input.Id);
            if (current.Name.Equals(input.Name))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Brigade.Update"],
                    new
                    {
                        name = input.Name,
                        description = input.Description,
                        brigadeType = (int)input.BrigadeType,
                        id = input.Id
                    });
            }
        }

        public async Task Add(Brigade input)
        {
            var all = await GetAll();
            if (all.Any(x => x.Name.Equals(input.Name)))
                throw new ValidationException(Error.AlreadyAddWithThisName);
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Brigade.Add"],
                    new
                    {
                        name = input.Name,
                        description = input.Description,
                        brigadeType = (int)input.BrigadeType
                    });
            }
        }

        public async Task<Brigade> Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var brigade = await ById(id);
                if (brigade != null)
                {
                    var result = await conn.ExecuteAsync(Sql.SqlQueryCach["Brigade.Delete"], new { id = id });
                    if (result != 0)
                        return brigade;
                    throw Error.CommonError;
                }
                throw new NotFoundException("Brigade");
            }
        }

        public class BrigadePaging
        {
            public Brigade[] Data { get; set; }
            public int Total { get; set; }
        }

    }
}
