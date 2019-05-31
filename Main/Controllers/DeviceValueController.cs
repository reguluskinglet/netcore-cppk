using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;

namespace Rzdppk.Controllers
{
    public class DeviceValueController : BaseController
    {
        private readonly ILogger _logger;

        public DeviceValueController
        (
            ILogger<DeviceController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> GetDeviceLocationsTable([FromBody] DeviceValueRequest input)
        {
            await CheckPermission();

            using (var rep = new DeviceValueRepository(_logger))
            {
                var result = await rep.GetLocationsTable(input);
                return Json(result);
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> GetDeviceChargesTable([FromBody] DeviceValueRequest input)
        {
            await CheckPermission();

            using (var rep = new DeviceValueRepository(_logger))
            {
                var result = await rep.GetChargesTable(input);
                return Json(result);
            }
        }
    }
}
