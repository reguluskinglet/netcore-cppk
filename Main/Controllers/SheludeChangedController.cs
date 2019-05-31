using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Rzdppk.Api.Requests;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Mapping;
using Rzdppk.Core.Services;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Rzdppk.Api.ScheludeChangedDtos;
using static Rzdppk.Core.Services.ScheduleChangedService;
using static Rzdppk.Core.Services.ScheduleCycleService.TimelineTypeEnum;

namespace Rzdppk.Controllers
{
    public class ChangedSheludeController : BaseController
    {

        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ChangedSheludeController
        (
            ILogger<ChangedSheludeController> logger,
            IMapper mapper
        )
        {
            base.Initialize();
            _logger = logger;
            _mapper = mapper;
        }
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetTimeRange([FromBody] InputRequest request)
        {
            var user = await CheckPermission();
            var service = new ScheduleChangedService(_logger, _mapper, user.Id);
            var result = await service.ChangedRouteTrainsTable(request.Date.Date);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetRouteInformation(int planedRouteTrainId)
        {
            var user = await CheckPermission();
            var service = new ScheduleChangedService(_logger, _mapper, user.Id);
            var result = await service.GetRouteInformation(planedRouteTrainId);
            return Json(result);
        }

        [HttpPost]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetRouteInformationTable([FromBody] GetRouteInformationTableRequest input)
        {
            if (input.PlanedRouteTrainId == 0 || input.TimelineTypeEnum == 0)
                throw new ValidationException("Некорректный planedRouteTrainId или timelineTypeEnum");
            var service = new ScheludeChangedTableService(_logger, _mapper);
            var result = await service.ScheduleChangedTableManager(input);
            switch (input.TimelineTypeEnum)
            {
                case (int)TimeRangeTo2:
                case (int)Cto:
                    return Json(result.To2);
                case (int)TimeBrigade:
                    return Json(result.TimeBrigade);
                case (int)TimeRangeTrip:
                case (int)TimeRangeTripTransfer:
                    return Json(result.TimeRangeTrip);

                default:
                    throw new Exception("GetRouteInformationTable Error");
            }
        }


        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> ChangeStantion([FromBody] List<ChangePlaneStantionOnTripDto> request)
        {
            if (request == null)
                throw new ValidationException("Не распарсилось или пристали null");
            var user = await CheckPermission();
            var service = new ScheduleChangedService(_logger, _mapper, user.Id);

            var mapper = new ChangePlaneStantionOnTripMapper();
            var entities = mapper.ToEntity(request);
            var notDropped = entities.Where(x => x.Droped == false).ToList();
            if (notDropped.Count == 1)
                entities.ForEach(x => x.Droped = true);
            var result = await service.AddOrUpdateChangePlaneStationOnTrip(entities);
            return Json(mapper.ToDto(result));
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> ChangeBrigade([FromBody] List<ChangePlaneBrigadeTrainDto> request)
        {
            if (request == null)
                throw new ValidationException("Не распарсилось или пристали null");
            var user = await CheckPermission();
            var service = new ScheduleChangedService(_logger, _mapper, user.Id);
            var mapper = new ChangePlaneBrigadeTrainMapper();
            var entity = mapper.ToEntity(request);
            var result = await service.AddOrUpdateChangeBrigadeOnTrip(entity);
            return Json(mapper.ToDto(result));
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> ChangeInspection([FromBody] ChangedPlanedInspectionRoute request)
        {
            if (request == null)
                throw new ValidationException("Не распарсилось или пристали null");
            var user = await CheckPermission();
            var service = new ScheduleChangedService(_logger, _mapper, user.Id);
            var result = await service.AddOrUpdateChangeInspection(request);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetTimelineLegend()
        {
            await CheckPermission();
            var result = new List<LegendEntry>
            {
                LegendTimeLine.To1,
                LegendTimeLine.To2,
                LegendTimeLine.Trip,
                LegendTimeLine.EntryBrigadeToTrain,
                LegendTimeLine.EscapeBrigadeFromTrain,
                LegendTimeLine.CriticalIncident
            };
            return Json(result);
        }


        public class InputRequest
        {
            public DateTime Date { get; set; }
        }


    }
}
