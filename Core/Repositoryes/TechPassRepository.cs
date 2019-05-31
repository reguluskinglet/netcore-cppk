using Dapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Model;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes
{
    public class TechPassRepository : ITechPassRepository
    {
        private readonly IDb _db;
        private readonly string _table;
        private readonly ILogger _logger;

        public TechPassRepository(IDb db)
        {
            _db = db;
            _table = "TechPasses";
        }

        public TechPassRepository(ILogger logger)
        {
            _logger = logger;
            _table = "TechPasses";
        }

        public async Task<TechPassPaging> GetAll(int skip, int limit, string filter)
        {
            var queries = CreateSqlFilterQueryCommon(skip, limit, filter, _table);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<TechPass>(queries.SqlQueryData);
                var count = await conn.ExecuteScalarAsync<int>(queries.SqlQueryCount);
                var output = new TechPassPaging
                {
                    Data = result.ToList(),
                    Total = count
                };
                return output;
            }
        }

        public async Task<List<TechPass>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<TechPass>(CommonSql.Select(_table));
                return result.ToList();
            }
        }

        public class TechPassPaging
        {
            public List<TechPass> Data { get; set; }
            public int Total { get; set; }
        }

        public async Task<TechPass> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<TechPass>(CommonSql.ById(_table, id));
            }
        }

        public async Task<TechPass> Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var toDel = await ById(id);
                if (toDel == null)
                    throw new NotFoundException();
                await conn.ExecuteAsync(CommonSql.Delete(_table, id));
                return toDel;
            }
        }


        public async Task<TechPass> Add(TechPass input)
        {
            //var all = await GetAll();
            //if (all.Any(x => x.Name.Equals(input.Name)))
            //    throw new ValidationException(Error.AlreadyAddWithThisName);
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(SqlAdd(input));
                return await ById(id);
            }
        }

        public async Task<TechPass> Update(TechPass input)
        {
            //var current = await ById(input.Id);
            //if (current.Name.Equals(input.Name))
            //    throw new ValidationException(Error.AlreadyAddWithThisName);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(SqlUpdate(input));
                return await ById(input.Id);
            }
        }


        private string SqlAdd(TechPass input)
        {
            return $@"
                insert into {_table}
                (Number, PlaceDrawUp, DateDrawUp, TrainId)
                VALUES
                ({input.Number},'{input.PlaceDrawUp}', '{input.DateDrawUp}', {input.TrainId})
                SELECT SCOPE_IDENTITY()
            ";
        }

        private string SqlUpdate(TechPass input)
        {
            return $@"
                update {_table}
                set
                Number = {input.Number},
                PlaceDrawUp = '{input.PlaceDrawUp}',
                DateDrawUp = '{input.DateDrawUp}',
                TrainId = {input.TrainId}
            ";
        }



    }

}