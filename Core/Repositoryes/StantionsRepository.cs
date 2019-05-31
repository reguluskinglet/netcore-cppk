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
using Rzdppk.Core.Repositoryes.Sqls.Stantions;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Core.Repositoryes
{
    public class StantionsRepository  
    {
        
        private readonly ILogger _logger;
        private readonly string _tableName;

        public StantionsRepository(ILogger logger)
        {
 
            _logger = logger;
            _tableName = "Stantions";
        }

        //public async Task<List<Stantion>> GetAllPaging(DevExtremeTableData.Paging paging)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        var result = await conn.QueryAsync<Stantion>(CommonSql.GetAll(_tableName));
        //        return result.ToList();
        //    }
        //}

        public async Task<List<Stantion>> ByTripId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Stantion.ById"];
                var result = await conn.QueryAsync<Stantion>(CommonSql.ByPropertyId(_tableName, "TripId", id));
                return result.ToList();
            }
        }

        public async Task<List<Stantion>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<Stantion>(CommonSql.GetAll(_tableName));
                return result.ToList();
            }
        }


        public async Task<StantionPaging> GetAll(int skip, int limit)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Stantion.All"];
                var result = await conn.QueryAsync<Stantion>(sql, new {skip = skip, limit = limit});
                var sqlc = Sql.SqlQueryCach["Stantion.CountAll"];
                var count = conn.ExecuteScalar<int>(sqlc);
                var output = new StantionPaging
                {
                    Data = result.ToArray(),
                    Total = count
                };

                return output;
            }
        }





        public async Task<StantionPaging> GetAll(int skip, int limit, string filter)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                string sqlfilter, sql;
                CreateFilter(filter, out sqlfilter, out sql);
                var result = await conn.QueryAsync<Stantion>(sql, new {skip = skip, limit = limit});
                var sqlc = $"{StantionsCommon.sqlCountCommon} {sqlfilter}";
                var count = conn.ExecuteScalar<int>(sqlc);
                var output = new StantionPaging
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
            sql = $"{StantionsCommon.sqlCommon} {sqlfilter} {Other.Other.SqlQueryPagingEnd}";
        }

        public async Task<Stantion[]> GetDepot()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Stantion.Depot"];
                var result = await conn.QueryAsync<Stantion>(sql);
                return result.ToArray();
            }
        }

        public async Task<Stantion> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Stantion.ById"];
                var result = await conn.QueryAsync<Stantion>(sql, new {stantion_id = id});
                return result.FirstOrDefault();
            }
        }


        public async Task<Stantion> Update(Stantion stantion)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                if ((await ById(stantion.Id)) == null)
                    throw new ValidationException("Нет станции с данным Id");

                await conn.ExecuteAsync(Sql.SqlQueryCach["Stantion.Update"],
                    new
                    {
                        name = stantion.Name,
                        description = stantion.Description,
                        type = (int) stantion.StantionType,
                        id = stantion.Id
                    });
                
                return await ById(stantion.Id);
            }
        }

        public async Task<Stantion> Add(Stantion stantion)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var resultId = await conn.QueryFirstOrDefaultAsync<int>(Sql.SqlQueryCach["Stantion.Add"],
                    new {name = stantion.Name, description = stantion.Description, type = (int) stantion.StantionType});

                return await ById(resultId);
            }
        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Stantion.Delete"], new {id = id});
            }

        }

        public class StantionPaging
        {
            public Stantion[] Data { get;set;}
            public int Total { get; set;}
        }

    }
}
