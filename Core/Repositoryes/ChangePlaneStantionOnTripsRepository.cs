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
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes
{
    public class ChangePlaneStantionOnTripsRepository
    {

        private readonly ILogger _logger;
        private readonly ChangePlaneStantionOnTripsSql _sql;

        public ChangePlaneStantionOnTripsRepository(ILogger logger)
        {
            _logger = logger;
            _sql = new ChangePlaneStantionOnTripsSql();
        }

        public async Task<ChangePlaneStantionOnTripPaging> GetAll(int skip, int limit, string filter)
        {
            CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, _sql);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<ChangePlaneStantionOnTrip>(sqlQueryData);
                var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
                var output = new ChangePlaneStantionOnTripPaging()
                {
                    Data = result.ToList(),
                    Total = count
                };

                return output;
            }
        }

        public async Task<List<ChangePlaneStantionOnTrip>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<ChangePlaneStantionOnTrip>(_sql.GetAll());
                return result.ToList();
            }
        }

        public class ChangePlaneStantionOnTripPaging
        {
            public List<ChangePlaneStantionOnTrip> Data { get; set; }
            public int Total { get; set; }
        }

        public async Task<ChangePlaneStantionOnTrip> Add(ChangePlaneStantionOnTrip input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
                return await ById(id);
            }
        }

        public async Task<ChangePlaneStantionOnTrip> Update(ChangePlaneStantionOnTrip input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Update(input));
                return await ById(input.Id);
            }
        }

        public async Task<ChangePlaneStantionOnTrip> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<ChangePlaneStantionOnTrip>(_sql.ById(id));
            }
        }

        public async Task<ChangePlaneStantionOnTrip> ByPlaneStantionOnTripId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<ChangePlaneStantionOnTrip>(_sql.ByPlaneStantionOnTripId(id));
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
