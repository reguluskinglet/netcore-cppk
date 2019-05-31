using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Model;

namespace Rzdppk.Controllers
{
    public class DeviceFaultController : BaseController
    {
        private readonly ILogger _logger;

        public DeviceFaultController
        (
            ILogger<DeviceFaultController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetAll()
        {
            await CheckPermission();

            using (var rep = new DeviceFaultRepository(_logger))
            {
                var result = await rep.GetAll();

                return Json(result);
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> AddOrUpdate([FromBody] DeviceFault input)
        {
            await CheckPermission();

            using (var rep = new DeviceFaultRepository(_logger))
            {
                if (input.Id == 0)
                    return Json(await rep.Add(input));

                return Json(await rep.Update(input));
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task Delete(int id)
        {
            await CheckPermission();

            using (var rep = new DeviceFaultRepository(_logger))
            {
                await rep.Delete(id);
            }
        }
    }
}
