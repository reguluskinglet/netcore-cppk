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
    public class TrainTaskAttributesRepository
    {

        private readonly ILogger _logger;
        private readonly TrainTaskAttributeSql _sql;

        public TrainTaskAttributesRepository(ILogger logger)
        {
            _logger = logger;
            _sql = new TrainTaskAttributeSql();
        }

        public async Task<TrainTaskAttributePaging> GetAll(int skip, int limit, string filter)
        {
            CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, _sql);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<TrainTaskAttribute>(sqlQueryData);
                var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
                var output = new TrainTaskAttributePaging()
                {
                    Data = result.ToList(),
                    Total = count
                };

                return output;
            }
        }

        public async Task<List<TrainTaskAttribute>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<TrainTaskAttribute>(_sql.GetAll());
                return result.ToList();
            }
        }

        public class TrainTaskAttributePaging
        {
            public List<TrainTaskAttribute> Data { get; set; }
            public int Total { get; set; }
        }

        //public async Task<TrainTaskAttribute> Add(TrainTaskAttribute input)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
        //        return await ById(id);
        //    }
        //}

        //public async Task<TrainTaskAttribute> Update(TrainTaskAttribute input)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        await conn.ExecuteAsync(_sql.Update(input));
        //        return await ById(input.Id);
        //    }
        //}

        public async Task<TrainTaskAttribute> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<TrainTaskAttribute>(_sql.ById(id));
            }
        }

        public async Task<List<TrainTaskAttribute>> ByInspectionId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return (await conn.QueryAsync<TrainTaskAttribute>(_sql.ByInspectionId(id))).ToList();
            }
        }

        public async Task<List<TrainTaskAttribute>> ByTaskId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return (await conn.QueryAsync<TrainTaskAttribute>(_sql.ByTaskId(id))).ToList();
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
