using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
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
using Rzdppk.Core.Repositoryes.Sqls.Fault;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Core.Repositoryes
{
    public class FaultsRepository
    {
        private readonly ILogger _logger;
        private readonly string _table;
        

        public FaultsRepository(ILogger logger)
        {
            _logger = logger;
            _table = "Faults";

        }

        public async Task<List<Fault>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<Fault>(CommonSql.GetAll(_table));
                return result.ToList();
            }
        }


        public async Task<FaultPaging> GetAll(int skip, int limit)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Fault.All"];
                var result = await conn.QueryAsync<Fault>(sql, new {skip = skip, limit = limit});
                var sqlc = Sql.SqlQueryCach["Fault.CountAll"];
                var count = await conn.ExecuteScalarAsync<int>(sqlc);
                var output = new FaultPaging()
                {
                    Data = result.ToArray(),
                    Total = count
                };

                return output;
            }
        }


        public async Task<FaultPaging> GetAll(int skip, int limit, string filter)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                string sqlfilter, sql;
                CreateFilter(filter, out sqlfilter, out sql);
                //var sql = Sql.SqlQueryCach["Fault.All"];
                var result = await conn.QueryAsync<Fault>(sql, new {skip = skip, limit = limit});
                var sqlc = $"{FaultCommon.sqlCountCommon} {sqlfilter}";
                var count = conn.ExecuteScalar<int>(sqlc);
                var output = new FaultPaging()
                {
                    Data = result.ToArray(),
                    Total = count
                };

                return output;
            }
        }


        private static void CreateFilter(string filter, out string sqlfilter, out string sql)
        {
            var filters = JsonConvert.DeserializeObject<Other.Other.FilterBody[]>(filter);
            sqlfilter = "where ";
            for (var index = 0; index < filters.Length; index++)
            {
                var item = filters[index];
                sqlfilter = $"{sqlfilter} {item.Filter} like '%{item.Value}%' ";
                if (index < (filters.Length - 1))
                    sqlfilter = $"{sqlfilter} AND ";

            }

            var queryPagingEnd =
                Other.Other.SqlQueryPagingEnd.Replace(" id ", " Name ", StringComparison.OrdinalIgnoreCase);
            sql = $"{FaultCommon.sqlCommon} {sqlfilter} {queryPagingEnd}";
        }

        public async Task<Fault> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Fault.ById"];
                var result = await conn.QueryFirstOrDefaultAsync<Fault>(sql, new {id = id});
                return result;
             
            }
        }

        public async Task<Fault> GetById(int id, bool transaction)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Fault.ById"];
                var result = await conn.QueryAsync<Fault>(sql, new {id = id});
                return result.FirstOrDefault();
            }
        }

        public async Task<Fault[]> GetByEquipmentId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Fault.ByEquipmentId"];
                var result = await conn.QueryAsync<Fault>(sql, new {id = id});
                return result.ToArray();
            }
        }

        public async Task<Fault> Update(Fault input)
        {

            var current = await ById(input.Id);
            if (current.Name.Equals(input.Name))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Fault.Update"],
                    new
                    {
                        name = input.Name,
                        description = input.Description,
                        faultType = input.FaultType,
                        id = input.Id
                    });
                return await ById(input.Id);
            }
        }

        public async Task<Fault> Add(Fault input)
        {
            var all = await GetAll();
            if (all.Any(x => x.Name.Equals(input.Name)))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var queryResult = await conn.QueryAsync<int>(Sql.SqlQueryCach["Fault.Add"],
                    new {name = input.Name, description = input.Description, faultType = input.FaultType});
                var id = queryResult.FirstOrDefault();
                return await ById(id);
            }
        }

        public async Task<Fault> Add(Fault input, bool transaction)
        {
            var all = await GetAll();
            if (all.Any(x => x.Name.Equals(input.Name)))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var queryResult = await conn.QueryAsync<int>(Sql.SqlQueryCach["Fault.Add"],
                    new {name = input.Name, description = input.Description, faultType = input.FaultType});
                var id = queryResult.FirstOrDefault();

                return await GetById(id, true);
            }
        }


        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                await conn.ExecuteAsync(Sql.SqlQueryCach["Fault.Delete"], new {id = id});
            }
        }

        public class FaultPaging
        {
            public Fault[] Data { get;set;}
            public int Total { get; set;}
        }

    }
}
