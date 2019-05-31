using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Services;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Services.TripOnRoutesService;

namespace Rzdppk.Controllers
{
    public class TripOnRoutesController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public TripOnRoutesController( ILogger<TripOnRoutesController> logger, IMapper mapper)
        {
            base.Initialize();
            _logger = logger;
            _mapper = mapper;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> ByRouteAndTripId(int routeId, int TripId)
        {
            await CheckPermission();
            var service = new TripOnRoutesService(_logger, _mapper);
            var result = await service.TripWithStationsByRouteIdAndTripId(routeId, TripId);
            return Json(result);
        }


        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task<JsonResult> AddOrUpdate([FromBody] TripOnRouteWithStationsDto input)
        //{
        //    await CheckPermission();
        //    if (input.TripWithDateTimeStations.StantionOnTripsWithStringTime.Count < 2)
        //        throw new Exception($"Недопустимое количество станций маршруте: {input.TripWithDateTimeStations.StantionOnTripsWithStringTime.Count}");

        //    var sqlR = new TripOnRoutesService(_logger, _mapper);
        //    if (input.Id != 0)
        //    //    return Json(sqlR.Update(input));
        //    //return Json(await sqlR.Add(input));
        //}

        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task Delete(int id)
        //{
        //    await CheckPermission();
        //    var sqlR = new InspectionRoutesRepository(_logger);
        //    sqlR.Delete(id);
        //}

    }
}
