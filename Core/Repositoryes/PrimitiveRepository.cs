using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using NPOI.OpenXmlFormats.Dml.Chart;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.PlaneBrigadeTrain;
using Rzdppk.Core.Repositoryes.Sqls.Primitive;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes
{
    public class PrimitiveRepository : AbstractRepository
    {
        
        private readonly ILogger _logger;
        private readonly string _table;

        public PrimitiveRepository(ILogger logger, string table) : base(logger, table)
        {
            _logger = logger;
            _table = table;
        }

        //public sealed override ISqlQueryStorage Sql { get; set; }




        //public async Task<PrimitivePaging> GetAll(int skip, int limit, string filter)
        //{
        //    CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, _sql);

        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        var result = await conn.QueryAsync<T>(sqlQueryData);
        //        var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
        //        var output = new PrimitivePaging
        //        {
        //            Data = result.ToList(),
        //            Total = count
        //        };

        //        return output;
        //    }
        //}

        //public class PrimitivePaging
        //{
        //    public List<T> Data { get; set; }
        //    public int Total { get; set; }
        //}

        //        public async Task<PlaneBrigadeTrain> Add(PlaneBrigadeTrain input)
        //        {
        //            using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //            {
        //                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
        //                return await ById(id);
        //            }
        //        }

        //        public async Task<PlaneBrigadeTrain> Update(PlaneBrigadeTrain input)
        //        {
        //            using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //            {
        //                await conn.ExecuteAsync(_sql.Update(input));
        //                return await ById(input.Id);
        //            }
        //        }

        //        public async Task<PlaneBrigadeTrain> ById(int id)
        //        {
        //            using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //            {
        //                return await conn.QueryFirstOrDefaultAsync<PlaneBrigadeTrain>(_sql.ById(id));
        //            }
        //        }

        //        public async void Delete(int id)
        //        {
        //            using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //            {
        //                await conn.ExecuteAsync(_sql.Delete(id));
        //            }
        //        }

        //        public async Task<List<PlaneBrigadeTrain>> ByPlannedRouteTrainId(int id)
        //        {
        //            using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //            {
        //                var result = await conn.QueryAsync<PlaneBrigadeTrain>(_sql.ByPlannedRouteTrainId(id));
        //                return result.ToList();
        //            }
        //        }


    }
}
