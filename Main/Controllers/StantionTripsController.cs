using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;

namespace Rzdppk.Controllers
{
    public class StantionTripsController : BaseController
    {

        private readonly ILogger _logger;

        public StantionTripsController
        (
            ILogger<StantionTripsController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }

        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task<JsonResult> GetFreeStation(int tripId)
        //{
        //    await CheckPermission();
        //    var sqlR = new StantionTripsRepository(_logger);
        //    var result = await sqlR.GetFreeStation(tripId);
        //    sqlR.Dispose();

        //    return Json(result);
        //}

        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task<JsonResult> GetLandingStation(int tripId, int stationTripId)
        //{
        //    await CheckPermission();
        //    var sqlR = new StantionTripsRepository(_logger);
        //    var result = await sqlR.GetLandingStation(tripId, stationTripId);
        //    sqlR.Dispose();
        //    return Json(result);
        //}

        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task AddCheckListToStation(int stationTripId, int? checkListType)
        //{
        //    await CheckPermission();
        //    var sqlR = new StantionTripsRepository(_logger);
        //    await sqlR.AddCheckListToStation(stationTripId, checkListType);
        //    sqlR.Dispose();
        //}

        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task AddBrigadeToStations(int tripId, int stationStartId, int stationEndId, int brigadeId)
        //{
        //    await CheckPermission();
        //    var sqlR = new StantionTripsRepository(_logger);
        //    await sqlR.AddBrigadeToStations(tripId, stationStartId, stationEndId, brigadeId);
        //    sqlR.Dispose();
        //}

        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task DeleteBrigadeFromStations(int tripId, int stationStartId, int stationEndId, int brigadeId)
        //{
        //    await CheckPermission();
        //    var sqlR = new StantionTripsRepository(_logger);
        //    await sqlR.DeleteBrigadeFromStations(tripId, stationStartId, stationEndId, brigadeId);
        //    sqlR.Dispose();
        //}

        ////TODO переименовать этот ебучий метод в получить свободные бригады ля ля ля
        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task<JsonResult> GetBrigadesFromStationRange(int tripId, int stationStartId, int stationEndId)
        //{
        //    await CheckPermission();
        //    var sqlR = new StantionTripsRepository(_logger);
        //    var result = await sqlR.GetBrigadesToPutToStationRange(tripId, stationStartId, stationEndId);
        //    sqlR.Dispose();
        //    return Json(result);
        //}
    }
}