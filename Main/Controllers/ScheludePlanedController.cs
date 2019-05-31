using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
using Rzdppk.Model.Auth;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Controllers
{
    public class ScheludePlanedController : BaseController
    {

        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ScheludePlanedController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<PlanedRouteTrain> logger,
            IMapper mapper
        )
        {
            base.Initialize();
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetPlanedRouteTrainsTable([FromBody] TimeRangeRequest timeRange)
        {
            await CheckPermission();
            var service = new ScheludePlanedService(_logger, _mapper);
            var result = await service.PlanedRouteTrainsTable(timeRange.StartTime, timeRange.EndTime);
            result = result.OrderBy(x => int.Parse("0" + Regex.Match(x.Route.Name, "(\\d+)").Value))
                           .ThenBy(x => string.Join("_", x.RouteDays.Select(d => ((int)d).ToString()))).ToList();
            return Json(result);
        }

        public class TimeRangeRequest
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }

        [HttpPost]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> PlanedRouteTrainsTable([FromBody] PlanedRouteTrain input)
        {
            await CheckPermission();
            var service = new ScheludePlanedService(_logger, _mapper);
            var result = await service.AddTrainToPlanedRouteTrains(input);
            return Json(result);
        }


        [HttpDelete]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task PlanedRouteTrainsTable(int id)
        {
            await CheckPermission();
            var service = new ScheludePlanedService(_logger, _mapper);
            await service.DeleteFromPlanedRouteTrains(id);
        }


        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetInputStation([FromBody] GetPlanedStationsRequest request)
        {
            await CheckPermission();
            var service = new ScheludePlanedService(_logger, _mapper);
            var result = await service.GetInputStation(request.PlanedRouteTrainId, request.Day, request.UserId);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetOutputStation([FromBody] GetPlanedStationsRequest request)
        {
            await CheckPermission();
            var service = new ScheludePlanedService(_logger, _mapper);
            var result = await service.GetOutputStation(request.PlanedRouteTrainId, request.InputStationId, request.Day, request.UserId);
            return Json(result);
        }

        public class GetPlanedStationsRequest
        {
            public DateTime Day { get; set; }
            public int PlanedRouteTrainId { get; set; }
            public int UserId { get; set; }
            public int? InputStationId { get; set; }
        }


        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task<JsonResult> AddOrUpdate([FromBody] Route input)
        //{
        //    await CheckPermission();
        //    var sqlR = new RoutesRepository(_logger);
        //    if (input.Id != 0)
        //        return Json(await sqlR.Update(input));
        //    return Json(await sqlR.Add(input));
        //}

        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task Delete(int id)
        //{
        //    await CheckPermission();
        //    var sqlR = new RoutesRepository(_logger);
        //    sqlR.Delete(id);
        //}
    }
}
