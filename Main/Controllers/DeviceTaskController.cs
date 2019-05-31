using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;

namespace Rzdppk.Controllers
{
    public class DeviceTaskController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DeviceTaskController
        (
            ILogger<DeviceTaskController> logger,
            IMapper mapper
        )
        {
            base.Initialize();
            _logger = logger;
            _mapper = mapper;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> GetTable([FromBody] DeviceTaskRequest input)
        {
            await CheckPermission();

            using (var rep = new DeviceTaskRepository(_logger))
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

            using (var rep = new DeviceTaskRepository())
            {
                var result = await rep.GetById(id);

                return Json(result);
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetAllStatuses()
        {
            await CheckPermission();

            using (var rep = new DeviceTaskRepository())
            {
                var result = rep.GetAllStatuses();

                return Json(result);
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> Add([FromBody] DeviceTaskDto input)
        {
            await CheckPermission();

            var user = await GetCurrentUser();

            using (var rep = new DeviceTaskRepository())
            {
                input.User = new ClassifierDto{Id = user.Id};
                input.CreateDate = DateTime.Now;

                var result = await rep.Add(input);

                return Json(result);
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task SaveCommentAndStatus([FromBody] DeviceTaskCommentDto input)
        {
            await CheckPermission();

            var user = await GetCurrentUser();

            using (var rep = new DeviceTaskRepository())
            {
                input.User = user;
                input.Date = DateTime.Now;

                await rep.SaveCommentAndStatus(input);
            }
        }
    }
}
