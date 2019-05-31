using Dapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Sqls;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Rzdppk.Core.Repositoryes
{
    public abstract class AbstractRepository
    {

        private readonly ILogger _logger;
        private readonly string _table;

        protected AbstractRepository(ILogger logger, string table)
        {
            _logger = logger;
            _table = table;
        }

        public async Task<T> ById<T>(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<T>(CommonSql.ById(_table, id));
            }
        }

        //public async Task<DataPaging<T>> GetAll<T>(int skip, int limit, string filter = null)
        //{
        //    CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, Sql);

        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {

        //        var result = await conn.QueryAsync<T>(sqlQueryData);
        //        var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
        //        var output = new DataPaging<T>()
        //        {
        //            Data = result.ToList(),
        //            Total = count
        //        };

        //        return output;
        //    }
        //}

        //public class DataPaging<T>
        //{
        //    public List<T> Data { get; set; }
        //    public int Total { get; set; }
        //}




        //public async Task<List<ChangedPlanedInspectionRoute>> GetAll()
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        var result = await conn.QueryAsync<ChangedPlanedInspectionRoute>(_sql.GetAll());
        //        return result.ToList();
        //    }
        //}



        //public async Task<ChangePlaneBrigadeTrain> Add(ChangePlaneBrigadeTrain input)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
        //        return await ById(id);
        //    }
        //}

        //public async Task<ChangePlaneBrigadeTrain> Update(ChangePlaneBrigadeTrain input)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        await conn.ExecuteAsync(_sql.Update(input));
        //        return await ById(input.Id);
        //    }
        //}



        //public async Task<ChangePlaneBrigadeTrain> ByPlaneBrigadeTrainId(int id)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        return await conn.QueryFirstOrDefaultAsync<ChangePlaneBrigadeTrain>(_sql.ByPlaneBrigadeTrainId(id));
        //    }
        //}



        //public async Task Delete(int id)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        await conn.ExecuteAsync(_sql.Delete(id));
        //    }
        //}


    }
}
