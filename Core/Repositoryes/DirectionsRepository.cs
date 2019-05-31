using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Rzdppk.Core.Options;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes.Sqls.PlaneBrigadeTrain;
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes
{
    public class DirectionsRepository
    {
        
        private readonly ILogger _logger;
        private readonly DirectionSql _sql;

        public DirectionsRepository(ILogger logger)
        {
            _logger = logger;
            _sql = new DirectionSql();
        }

        public async Task<DirectionPaging> GetAll(int skip, int limit, string filter)
        {
            CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, _sql);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<Direction>(sqlQueryData);
                var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
                var output = new DirectionPaging
                {
                    Data = result.ToList(),
                    Total = count
                };

                return output;
            }
        }

        public async Task<List<Direction>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<Direction>(_sql.GetAll());
                return result.ToList();
            }
        }

        public class DirectionPaging
        {
            public List<Direction> Data { get; set; }
            public int Total { get; set; }
        }

        public async Task<Direction> Add(Direction input)
        {
            var all = await GetAll();
            if (all.Any(x => x.Name.Equals(input.Name)))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
                return await ById(id);
            }
        }

        public async Task<Direction> Update(Direction input)
        {
            var current = await ById(input.Id);
            if (current.Name.Equals(input.Name))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Update(input));
                return await ById(input.Id);
            }
        }

        public async Task<Direction> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<Direction>(_sql.ById(id));
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
