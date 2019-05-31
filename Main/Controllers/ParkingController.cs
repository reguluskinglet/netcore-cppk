using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Services;
using Rzdppk.Model.Raspisanie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rzdppk.Controllers
{
    public class ParkingController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ParkingController
        (
            ILogger<ModelController> logger,
            IMapper mapper
        )
        {
            base.Initialize();
            _mapper = mapper;
            _logger = logger;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter)
        {
            await CheckPermission();
            var rep = new ParkingRepository(_logger);
            var result = new ParkingRepository.ParkingPaging();
            if (filter != null)
                result = await rep.GetAll(skip, limit, filter);
            else
                result = await rep.GetAll(skip, limit);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Add([FromBody]Parking input)
        {
            await CheckPermission();
            if (string.IsNullOrEmpty(input.Name) || string.IsNullOrEmpty(input.StantionId.ToString()))
                throw new Exception("Some input parameters NULL");
            var sqlR = new ParkingRepository(_logger);
            if (input.Id != 0)
            {
                await sqlR.Update(input);
                return Json(new { message = "Update OK" });
            }
            return Json(await sqlR.Add(input));
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Delete([FromBody]Parking model)
        {
            await CheckPermission();
            var er = new ParkingRepository(_logger);
            await er.Delete(model.Id);
            return Json(new { message = "Delete OK" });
        }
    }
}
