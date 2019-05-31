using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.GridModels.Route;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Services;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Core.ViewModels;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Controllers
{
    public class TripsController : BaseController
    {
        private readonly ILogger _logger;
        private readonly ICommonService _commonService;
        private readonly IDb _db;

        public TripsController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<TripsController> logger,
            ICommonService commonService
        )
        {
            base.Initialize();

            _db = db;
            _logger = logger;
            _commonService = commonService;
        }


        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter = null)
        {
            await CheckPermission();
            var sqlR = new TripsRepository(_logger);
            var result = await sqlR.GetAll(skip, limit, filter);
            return Json(result);
        }


        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> GetById(int? id)
        {
            await CheckPermission();

            var dataSource = await _commonService.GetTripSource();

            if (!id.HasValue)
                return Ok(new TripUi{DataSource = dataSource });

            var trip = await _db.Connection.QueryFirstAsync<TripUi>(Sql.SqlQueryCach["Trips.GetById"], new {id = id});

            trip.DataSource = dataSource;
            trip.Stantions = _db.Connection.Query<TripStantion>(Sql.SqlQueryCach["Trips.Stantion"], new { id = id });
            trip.Days = _db.Connection.Query<int>(Sql.SqlQueryCach["Trips.Days"], new {id = id});
           
            return Ok(trip);
        }
    }

    public class TripUi
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<int> Days { get; set; } = new List<int>();
        public IEnumerable<TripStantion> Stantions { get; set; } = new List<TripStantion>();
        public TripSource DataSource { get; set; }
        public bool TripType { get; set; }
    }
}
