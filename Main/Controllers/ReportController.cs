using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using Rzdppk.Model.Enums;

namespace Rzdppk.Controllers
{
    public class ReportController : BaseController
    {
        private readonly ILogger _logger;

        public ReportController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<ReportController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetList()
        {
            await CheckPermission();
            var rr = new ReportRepository(_logger);

            var result = rr.GetList();
            rr.Dispose();
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Get(int id, int skip, int limit, string filter, string orderby)
        {
            await CheckPermission();
            var rr = new ReportRepository(_logger);

            var result = await rr.Get(id, skip, limit, filter, orderby);
            rr.Dispose();
            return Json(result);
        }
    }
}