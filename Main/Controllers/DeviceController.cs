using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Model;

namespace Rzdppk.Controllers
{
    public class DeviceController : BaseController
    {
        private readonly ILogger _logger;

        public DeviceController
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
        public async Task<JsonResult> GetTable([FromBody] DeviceRequest input)
        {
            await CheckPermission();

            using (var rep = new DeviceRepository(_logger))
            {
                var result = await rep.GetTable(input);
                return Json(result);
            } 
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetById(int id)
        {
            await CheckPermission();

            using (var rep = new DeviceRepository(_logger))
            {
                var result = await rep.ById(id);
                return Json(result);
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddOrUpdate([FromBody] Device input)
        {
            await CheckPermission();

            using (var rep = new DeviceRepository(_logger))
            {
                if (input.Id == 0)
                    return Json(await rep.Add(input));

                return Json(await rep.Update(input));
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task Delete(int id)
        {
            await CheckPermission();

            using (var rep = new DeviceRepository(_logger))
            {
                using (var rept = new DeviceTaskRepository(_logger))
                {
                    var cnt = await rept.GetDeviceOpenTaskCount(id);

                    if (cnt > 0)
                    {
                        throw new Exception("Запрещено удалять устройство,е сли есть открытые задачи");
                    }

                    await rep.Delete(id);
                }
            }
        }
    }
}
