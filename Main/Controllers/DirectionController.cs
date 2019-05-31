using System;
using System.Data.SqlClient;
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
    public class DirectionController : BaseController
    {

        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly DirectionsRepository _sqlR;

        public DirectionController
        (
            ILogger<DirectionController> logger,
            IMapper mapper
        )
        {
            base.Initialize();
            _logger = logger;
            _mapper = mapper;
            _sqlR = new DirectionsRepository(_logger);
        }


        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter = null)
        {
            await CheckPermission();
            var result = await _sqlR.GetAll(skip, limit, filter);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddOrUpdate([FromBody] Direction input)
        {
            await CheckPermission();
            if (input.Id == 0)
                return Json(await _sqlR.Add(input));
            return Json(await _sqlR.Update(input));
        }


        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task Delete(int id)
        {
            await CheckPermission();
            await _sqlR.Delete(id);
        }
    }
}
