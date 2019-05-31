using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Controllers
{
    public class RouteController : BaseController
    {

        private readonly ILogger _logger;

        public RouteController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<RouteController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter = null)
        {
            await CheckPermission();
            var er = new RoutesRepository(_logger);
            var result = await er.GetAll(skip, limit, filter);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddOrUpdate([FromBody] Route input)
        {
            await CheckPermission();
            var sqlR = new RoutesRepository(_logger);
            if (string.IsNullOrEmpty(input.Name))
                throw new ValidationException(Error.NotFilledOptionalField);

            if (input.TurnoverId != null)
            {
                var routesToCheck = await sqlR.ByTurnoverId((int) input.TurnoverId);
                if (input.Id != 0)
                    routesToCheck = routesToCheck.Where(x => x.Id != input.Id).ToList();
                if (routesToCheck.Any(x => x.Name.Equals(input.Name)))
                    throw new Other.GenaException(Error.AlreadyAddWithThisName);
            }
            else
            {
                var routesToCheck = await sqlR.WithoutTurnover();
                if (input.Id != 0)
                    routesToCheck = routesToCheck.Where(x => x.Id != input.Id).ToList();
                if (routesToCheck.Any(x => x.Name.Equals(input.Name)))
                    throw new Other.GenaException(Error.AlreadyAddWithThisName);
            }
            
            if (input.Id != 0)
                return Json(await sqlR.Update(input));
            return Json(await sqlR.Add(input));
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task Delete(int id)
        {
            await CheckPermission();
            var sqlR = new RoutesRepository(_logger);
            sqlR.Delete(id);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task ClearRouteTurnoverId(int routeId)
        {
            await CheckPermission();
            var er = new RoutesRepository(_logger);
            await er.ClearTurnoverId(routeId);
        }

    }
}
