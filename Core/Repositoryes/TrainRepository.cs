using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
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
using Rzdppk.Core.Repositoryes.Sqls.Routes;
using Rzdppk.Core.Repositoryes.Sqls.Stantions;
using Rzdppk.Core.Repositoryes.Sqls.Train;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Core.Repositoryes
{
    public class TrainRepository : ITrainRepository, IDisposable
    {
        private readonly IDb _db;
        private readonly ILogger _logger;
        private readonly TrainSql _sql;

        public TrainRepository(ILogger logger)
        {
            _db = new Db();
            _logger = logger;
            _sql = new TrainSql();
        }

        public TrainRepository(IDb db, ILogger logger)
        {
            _db = db;
            _logger = logger;
            _sql = new TrainSql();
        }

        public async Task<TrainPaging> GetAll(int skip, int limit)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Train.All"];
                var result = (await conn.QueryAsync<TrainExt, Stantion, TrainExt>(
                    sql,
                    (train, stantion) =>
                    {
                        train.Stantion = stantion;
                        return train;
                    }, new {skip = skip, limit = limit})).ToArray();

                foreach (var item in result)
                {
                    item.CanDelete = true;
                }

                var sqlc = Sql.SqlQueryCach["Train.CountAll"];
                var count = conn.ExecuteScalar<int>(sqlc);
                var output = new TrainPaging
                {
                    Data = result,
                    Total = count
                };

                return output;
            }
        }


        /// <summary>
        /// Силект звездочка по таблице, хули
        /// </summary>
        public async Task<List<Train>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return (await conn.QueryAsync<Train>(_sql.All())).ToList();
            }
        }

        /// <summary>
        /// По ид плановой станки
        /// </summary>
        public async Task<int> TrainIdByPlaneStationId(int planeStationId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<int>(_sql.ByPlaneStationId(planeStationId));
            }
        }

        public async Task<TrainPaging> GetAll(int skip, int limit, string filter)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                string sqlfilter, sql;
                CreateFilter(filter, out sqlfilter, out sql);
                var result = (await conn.QueryAsync<TrainExt, Stantion, TrainExt>(
                    sql,
                    (train, stantion) =>
                    {
                        train.Stantion = stantion;
                        return train;
                    }, new {skip = skip, limit = limit})).ToArray();

                foreach (var item in result)
                {
                    item.CanDelete = true;
                }

                var sqlc = $"{TrainCommon.sqlCountCommon} {sqlfilter}";
                var count = conn.ExecuteScalar<int>(sqlc);
                var output = new TrainPaging
                {
                    Data = result,
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
                sqlfilter = item.Filter.Equals("stantionId")
                                ? $"{sqlfilter} t.{item.Filter} = {item.Value} "
                                : $"{sqlfilter} t.{item.Filter} like '%{item.Value}%' ";
                if (index < (filters.Length - 1))
                    sqlfilter = $"{sqlfilter} AND ";

            }
            sql = $"{TrainCommon.sqlCommon} {sqlfilter} {Other.Other.SqlQueryPagingEndT}";
        }


        public async Task<Train> ByIdWithStations(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Train.ById"];
                var result = await conn.QueryAsync<TrainExt, Stantion, TrainExt>(
                    sql,
                    (train, stantion) =>
                    {
                        train.Stantion = stantion;
                        return train;
                    }, new {train_id = id});

                return result.FirstOrDefault();
            }
        }
        public async Task<Train> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<Train>(_sql.ById(id));
            }
        }

        public async Task<Train> ById(int? id)
        {
            if (id == null)
                return new Train();
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<Train>(_sql.ById(id));
            }
        }

        public async Task<Train> Update(Train train)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Train.Update"];
                await conn.QueryFirstOrDefaultAsync<Train>(sql,
                    new
                    {
                        name = train.Name,
                        description = train.Description,
                        id = train.Id,
                        stantion_id = train.StantionId
                    });

                return await ById(train.Id);
                
            }
        }

        public async Task<Train> Add(Train train)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var all = await GetAll();
                if (all.Any(x => x.Name.Equals(train.Name)))
                    throw new ValidationException(Error.AlreadyAdd);

                var id = await conn.QueryFirstOrDefaultAsync<int>(Sql.SqlQueryCach["Train.Add"],
                    new {name = train.Name, description = train.Description, stantion_id = train.StantionId});
                return await ById(id);
            }
        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Train.Delete"], new {id = id});
            }
        }

        public class TrainPaging
        {
            public TrainExt[] Data { get;set;}
            public int Total { get; set;}
        }

        public class TrainExt: Train
        {
            public bool CanDelete { get; set; }
        }

        public void Dispose()
        {
            _db.Connection.Close();
        }
    }
}
