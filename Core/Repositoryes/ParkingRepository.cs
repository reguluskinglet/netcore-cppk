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
using Rzdppk.Core.Repositoryes.Sqls.Parkings;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Core.Repositoryes
{
    public class ParkingRepository
    {
        
        private readonly ILogger _logger;

        public ParkingRepository(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<ParkingPaging> GetAll(int skip, int limit)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Parkings.All"];
                var result = (await conn.QueryAsync<Parking, Stantion, Parking>(
                    sql,
                    (parking, stantion) => {
                        parking.Stantion = stantion;
                        return parking;
                    }, 
                    new { skip = skip, limit = limit })
                ).ToArray();
                var sqlc = Sql.SqlQueryCach["Parkings.CountAll"];
                var count = (await conn.QueryAsync<int>(sqlc)).FirstOrDefault();
                var output = new ParkingPaging
                {
                    Data = result.ToList(),
                    Total = count
                };

                return output;
            }
        }

        public async Task<ParkingPaging> GetAll(int skip, int limit, string filter)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                string sqlfilter, sql;
                CreateFilter(filter, out sqlfilter, out sql);
                var result = (await conn.QueryAsync<Parking, Stantion, Parking>(
                    sql,
                    (parking, stantion) => {
                        parking.Stantion = stantion;
                        return parking;
                    },
                    new { skip = skip, limit = limit })
                ).ToArray();
                var sqlc = $"{ParkingCommon.sqlCountCommon} {sqlfilter}";
                var count = conn.ExecuteScalar<int>(sqlc);
                var output = new ParkingPaging
                {
                    Data = result.ToList(),
                    Total = count
                };

                return output;
            }
        }

        public async Task<List<Parking>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Parkings.All"];
                var result = await conn.QueryAsync<Parking>(sql, new { skip = 0, limit = int.MaxValue });

                return result.ToList();
            }
        }

        public async Task<Parking> Add(Parking input)
        {
            var all = await GetAll();
            if (all.Any(x => x.Name.Equals(input.Name)))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Parkings.Add"];
                var id = await conn.QueryAsync<int>(sql,
                    new { name = input.Name, description = input.Description, stantionId = input.StantionId });

                input.Id = id.First();

                return input;
            }
        }

        public async Task Update(Parking input)
        {
            var all = await GetAll();
            if (all.Any(x => x.Name.Equals(input.Name)))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Parkings.Update"],
                    new
                    {
                        name = input.Name,
                        description = input.Description,
                        stantionId = (int)input.StantionId,
                        id = input.Id
                    });
            }
        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Parkings.Delete"], new { id = id });
            }
        }

        private static void CreateFilter(string filter, out string sqlfilter, out string sql)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var filters = JsonConvert.DeserializeObject<Other.Other.FilterBody[]>(filter);
                sqlfilter = "WHERE ";
                for (var index = 0; index < filters.Length; index++)
                {
                    var item = filters[index];
                    sqlfilter = $"{sqlfilter} p.{item.Filter} LIKE '%{item.Value}%' ";
                    if (index < (filters.Length - 1))
                        sqlfilter = $"{sqlfilter} AND ";

                }
                sql = $"{ParkingCommon.sqlCommon} {sqlfilter} {ParkingCommon.SqlQueryPagingEndT}";
            }
        }

        public class ParkingPaging
        {
            public List<Parking> Data { get; set; }
            public int Total { get; set; }
        }
    }
}
