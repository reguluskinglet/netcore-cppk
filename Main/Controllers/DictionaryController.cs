using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Other;
using Rzdppk.Core.Services;
using static Rzdppk.Core.Other.DevExtremeTableData;
using static Rzdppk.Core.Services.DictionaryService;

namespace Rzdppk.Controllers
{
    public class DictionaryController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DictionaryController
        (
            ILogger<ChangedSheludeController> logger,
            IMapper mapper
        )
        {
            base.Initialize();
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Станции
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Stantion([FromBody] ReportRequest request)
        {
            await CheckPermission();
            var service = new DictionaryService(_logger, _mapper);
            request.ReportId = ((int)DictionaryTableTableEnum.Stantion).ToString();
            var result = await service.DictionaryTableManager(request);
            return Json(result);
        }

        /// <summary>
        /// Направления
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Direction([FromBody] ReportRequest request)
        {
            await CheckPermission();
            var service = new DictionaryService(_logger, _mapper);
            request.ReportId = ((int)DictionaryTableTableEnum.Direction).ToString();
            var result = await service.DictionaryTableManager(request);
            return Json(result);
        }

        /// <summary>
        /// Неисправности
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Fault([FromBody] ReportRequest request)
        {
            await CheckPermission();
            var service = new DictionaryService(_logger, _mapper);
            request.ReportId = ((int)DictionaryTableTableEnum.Fault).ToString();
            var result = await service.DictionaryTableManager(request);
            return Json(result);
        }

        /// <summary>
        /// Парковки
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Parking([FromBody] ReportRequest request)
        {
            await CheckPermission();
            var service = new DictionaryService(_logger, _mapper);
            request.ReportId = ((int)DictionaryTableTableEnum.Parking).ToString();
            var result = await service.DictionaryTableManager(request);
            return Json(result);
        }



        /// <summary>
        /// Рейсы с днями
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> TripWithDays([FromBody] ReportRequest request)
        {
            await CheckPermission();
            var service = new DictionaryService(_logger, _mapper);
            request.ReportId = ((int)DictionaryTableTableEnum.TripsWithDays).ToString();
            var result = await service.DictionaryTableManager(request);
            return Json(result);
        }

        /// <summary>
        /// DeviceFaults
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> DeviceFault([FromBody] ReportRequest request)
        {
            await CheckPermission();
            var service = new DictionaryService(_logger, _mapper);
            request.ReportId = ((int)DictionaryTableTableEnum.DeviceFaults).ToString();
            var result = await service.DictionaryTableManager(request);
            return Json(result);
        }


        /// <summary>
        /// Станции КРУД
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Stantion([FromBody] DictionaryCrudRequest request)
        {
            await CheckPermission();
            var service = new DictionaryService(_logger, _mapper);
            if (request?.IdToDelete != null)
                return Json(await service.StantionCrud(request));
            if (request?.Stantion.Name == null)
                throw new ValidationException(Error.NotFilledOptionalField);
            return Json(await service.StantionCrud(request));
        }



        /// <summary>
        /// Направления КРУД
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Direction([FromBody] DictionaryCrudRequest request)
        {
            await CheckPermission();
            var service = new DictionaryService(_logger, _mapper);
            if (request?.IdToDelete != null)
                return Json(await service.DirectionCrud(request));
            if (request?.Direction.Name == null)
                throw new ValidationException(Error.NotFilledOptionalField);
            return Json(await service.DirectionCrud(request));
        }


        /// <summary>
        /// Неисправности КРУД
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Fault([FromBody] DictionaryCrudRequest request)
        {
            await CheckPermission();
            var service = new DictionaryService(_logger, _mapper);
            if (request?.IdToDelete != null)
                return Json(await service.FaultCrud(request));
            var result = await service.FaultCrud(request);
            return Json(result);
        }

        /*/// <summary>
        /// Парковки КРУД
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Parking([FromBody] DictionaryCrudRequest request)
        {
            await CheckPermission();
            var service = new DictionaryService(_logger, _mapper);
            if (request?.IdToDelete != null)
                return Json(await service.ParkingCrud(request));
            if (request?.Parking.Name == null || request.Parking?.StantionId == 0)
                throw new ValidationException(Error.NotFilledOptionalField);
            return Json(await service.ParkingCrud(request));
        }*/


        /// <summary>
        /// Рейсы КРУД
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> TripWithDays([FromBody] DictionaryCrudRequest request)
        {
            await CheckPermission();
            var service = new DictionaryService(_logger, _mapper);
            if (request?.IdToDelete != null)
                return Json(await service.TripWithDaysCrud(request));
            if (string.IsNullOrEmpty(request?.TripWithDays?.Name))
                throw new ValidationException(Error.NotFilledOptionalField);
            return Json(await service.TripWithDaysCrud(request));
        }

        /// <summary>
        /// DeviceFaults КРУД
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> DeviceFault([FromBody] DictionaryCrudRequest request)
        {
            await CheckPermission();
            var service = new DictionaryService(_logger, _mapper);
            if (request?.IdToDelete != null)
                return Json(await service.DeviceFaultsCrud(request));
            if (request?.DeviceFault.Name == null)
                throw new ValidationException(Error.NotFilledOptionalField);
            return Json(await service.DeviceFaultsCrud(request));

        }




    }
}