using System;
using System.Collections.Generic;
using System.Linq;
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
using static Rzdppk.Api.ScheludePlanedDtos;

namespace Rzdppk.Controllers
{
    public class PlaneBrigadeTrainController : BaseController
    {

        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public PlaneBrigadeTrainController
        (
            ILogger<PlaneBrigadeTrainController> logger,
            IMapper mapper
        )
        {
            base.Initialize();
            _logger = logger;
            _mapper = mapper;
        }


        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter = null)
        {
            await CheckPermission();
            var sqlR = new PlaneBrigadeTrainsRepository(_logger);
            var result = await sqlR.GetAll(skip, limit, filter);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddOrUpdate([FromBody] PlaneBrigadeTrainDto input)
        {
            await CheckPermission();
            var service = new ScheludePlanedService(_logger,_mapper);
            if (!input.UserIds.Any())
                input.UserIds = new List<int> {input.UserId};
            if (input.Id == 0)
                return Json(await service.AddUserToTrain(input));
            throw new NotImplementedException("Update not implement");
            //return Json(await sqlR.Update(input));
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

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task Delete(int id)
        {
            await CheckPermission();
            var sqlR = new PlaneBrigadeTrainsRepository(_logger);
            await sqlR.Delete(id);
        }
    }
}
