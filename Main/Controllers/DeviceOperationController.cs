using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;

namespace Rzdppk.Controllers
{
    public class DeviceOperationController : BaseController
    {
        private readonly ILogger _logger;

        public DeviceOperationController
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
        public async Task<JsonResult> GetDeviceHistoryTable([FromBody] DeviceOperationRequest input)
        {
            await CheckPermission();

            using (var rep = new DeviceOperationRepository(_logger))
            {
                var result = await rep.GetTable(input);
                return Json(result);
            }
        }
    }
}
