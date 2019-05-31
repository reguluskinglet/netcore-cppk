using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model.Dto;

namespace Rzdppk.Controllers
{
    public class TabletIssueStationSyncController : BaseExternalController
    {
        private readonly ILogger _logger;

        public TabletIssueStationSyncController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<TabletIssueStationSyncController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }

        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetAllUsers()
        {
            CheckApiKey();

            using (var sqlR = new UserRepository(_logger))
            {
                var result = await sqlR.GetAllForSync();

                return Json(result);
            }
        }

        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetAllDevices()
        {
            CheckApiKey();

            using (var sqlR = new DeviceRepository(_logger))
            {
                var result = await sqlR.GetAllForSync();

                return Json(result);
            }
        }

        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetAllDeviceFaults()
        {
            CheckApiKey();

            using (var sqlR = new DeviceFaultRepository(_logger))
            {
                var result = await sqlR.GetAllForSync();

                return Json(result);
            }
        }

        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> CreateTask([FromBody] TaskOutDto task)
        {
            CheckApiKey();

            using (var sqlR = new DeviceTaskRepository(_logger))
            {
                var ret = new InsertResDto();
                try
                {
                    await sqlR.CreateTask(task);
                    ret.IsSuccess = true;
                }
                catch (Exception e)
                {
                    ret.Error = e.Message;

                    if (e is SqlException sqle)
                    {
                        if (sqle.Number == 2601 && sqle.Message.ToLower().Contains("refid"))
                        {
                            ret.AlreadyExist = true;
                            ret.Error = "already exist";
                        }
                    }
                }

                return Json(ret);
            }
        }

        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> CreateOperation([FromBody] OperationOutDto oper)
        {
            CheckApiKey();

            using (var sqlR = new DeviceRepository(_logger))
            {
                var ret = new InsertResDto();
                try
                {
                    await sqlR.CreateOperation(oper);
                    ret.IsSuccess = true;
                }
                catch (Exception e)
                {
                    ret.Error = e.Message;

                    if (e is SqlException sqle)
                    {
                        if (sqle.Number == 2601 && sqle.Message.ToLower().Contains("refid"))
                        {
                            ret.AlreadyExist = true;
                            ret.Error = "already exist";
                        }
                    }
                }

                return Json(ret);
            }
        }
    }
}
