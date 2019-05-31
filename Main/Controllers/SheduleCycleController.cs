using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Other;
using Rzdppk.Core.Services;
using Rzdppk.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static Rzdppk.Core.Services.ScheduleCycleService;

namespace Rzdppk.Controllers
{
    public class SheduleCycleController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public SheduleCycleController
        (
            ILogger<BrigadeController> logger,
            IMapper mapper
        )
        {
            base.Initialize();
            _mapper = mapper;
            _logger = logger;
        }




        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetRoutesWithTripsAndToByTurnoverId(int turnoverId, int skip, int limit)
        {
            await CheckPermission();
            var service = new ScheduleCycleService(_logger, _mapper);
            var result = await service.GetRoutesWithTripsAndToByTurnoverId(turnoverId, skip, limit);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetRoutesWithTimeline(int turnoverId, int skip, int limit)
        {
            await CheckPermission();
            var service = new ScheduleCycleService(_logger, _mapper);
            var result = await service.GetRoutesWithTimeline(turnoverId, skip, limit);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> TripsByTurnoverIdAndDays(int turnoverId, int routeId)
        {
            await CheckPermission();

            if (turnoverId == 0 || routeId == 0)
                throw new ValidationException(Error.NotFilledOptionalField);

            var service = new ScheduleCycleService(_logger, _mapper);
            var result = await service.GetTripsByTurnoverIdAndDays(turnoverId, routeId);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddOrUpdateTripToRoute([FromBody] TripOnRoutesService.TripOnRouteWithStationsDto input)
        {
            await CheckPermission();

            var service = new TripOnRoutesService(_logger, _mapper);
            //Добавляем новый трип без добавления на роут
            if (input.RouteId == 0 && input.Days != null && input.TripWithDateTimeStations != null)
            {
                ValidateTripData(input);
                return Json(await service.AddTripOnRoute(input));
            }
            //Добавляем существующий трип на роут
            if ((input.TripId != 0 || input.TripIds.AnyOrNotNull()) && input.Id == 0 && input.RouteId != 0)
            {
                var tripIds = new List<int>();
                if (input.TripId != 0)
                {
                    tripIds.Add(input.TripId);
                }
                else
                {
                    tripIds.AddRange(input.TripIds);
                }

                return Json(await service.AddExistingTripsOnRouteToRoute(input.RouteId, tripIds));

            }

            //Обновляем существующий трип(Врятле работает) хД)
            if (input.Id != 0 && input.Days != null && input.Days.Any())
            {
                ValidateTripData(input);
                return Json(await service.UpdateTripOnRoute(input));
            }

            throw Error.CommonError;

        }

        private static void ValidateTripData(TripOnRoutesService.TripOnRouteWithStationsDto input)
        {
            if (input.TripWithDateTimeStations?.Name == null ||
                input.TripWithDateTimeStations?.StantionOnTrips?.Count < 2 ||
                input.TripWithDateTimeStations.StantionOnTripsWithStringTime == null ||
                input.TripWithDateTimeStations.StantionOnTripsWithStringTime.Count < 2 ||
                !input.Days.Any())
                throw new ValidationException(Error.NotFilledOptionalField);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task RemoveTripFromRoute(int tripOnrouteId)
        {
            await CheckPermission();
            var service = new TripOnRoutesService(_logger, _mapper);
            await service.RemoveTripsOnRouteFromRoute(tripOnrouteId);
        }




        #region Графики оборота с днями.

        [HttpGet]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> TurnoverWithDays(int skip, int limit)
        {
            await CheckPermission();
            var service = new ScheduleCycleService(_logger, _mapper);
            var result = await service.GetTurnoversWithDays(skip, limit);
            return Json(result);
        }

        [HttpPost]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> TurnoverWithDays([FromBody] TurnoverWithDays input)
        {
            await CheckPermission();
            if (input.Days == null || input.Days.Count == 0)
                throw new ValidationException("Не заполнены дни циклового графика");
            var service = new ScheduleCycleService(_logger, _mapper);
            if (input.Id != 0)
                return Json(await service.UpdateTurnoverWithDays(input));
            return Json(await service.AddTurnoverWithDays(input));
        }

        [HttpDelete("{id}")]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task TurnoverWithDays(int id)
        {
            await CheckPermission();
            var service = new ScheduleCycleService(_logger, _mapper);
            await service.DeleteTurnoverWithDays(id);
        }

        #endregion

    }
}
