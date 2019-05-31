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
    public class ChangePlaneBrigadeTrainsController : BaseController
    {

        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ChangePlaneBrigadeTrainsController
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
            var sqlR = new ChangePlaneBrigadeTrainsRepository(_logger);
            var result = await sqlR.GetAll(skip, limit, filter);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddOrUpdate([FromBody] ChangePlaneBrigadeTrain input)
        {
            await CheckPermission();
            var sqlR = new ChangePlaneBrigadeTrainsRepository(_logger);
            if (input.Id == 0)
                return Json(await sqlR.Add(input));
            return Json(await sqlR.Update(input));
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task Delete(int id)
        {
            await CheckPermission();
            var sqlR = new ChangePlaneBrigadeTrainsRepository(_logger);
            await sqlR.Delete(id);
        }
    }
}
