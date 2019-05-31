using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Core.Extensions;
using Rzdppk.Core.Old;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using Rzdppk.Core.Repositoryes.Sqls.TripOnRoute;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Core.Repositoryes
{
    public class TripsOnRouteRepository 
    {

        private readonly ILogger _logger;
        private readonly TripOnRouteSql _sql;

        public TripsOnRouteRepository(ILogger logger)
        {
            _logger = logger;
            _sql = new TripOnRouteSql();
        }

        public async Task<TripOnRoute> ByRouteIdAndTripId(int routeId, int tripId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryFirstOrDefaultAsync<TripOnRoute>(_sql.ByRouteAndTripId(routeId, tripId));
                return result;
            }
        }

        public async Task<List<TripOnRoute>> ByRouteId(int routeId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<TripOnRoute>(_sql.ByRouteId(routeId));
                return result.ToList();
            }
        }

        public async Task<TripOnRoute> Add(TripOnRoute input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input.RouteId, input.TripId));
                return await ById(id);
            }
        }

        public async Task<TripOnRoute> Update(TripOnRoute input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Update(input));
                return await ById(input.Id);
            }
        }


        public async Task<TripOnRoute> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<TripOnRoute>(_sql.ById(id));
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
