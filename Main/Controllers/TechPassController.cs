using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Model;
using System.Threading.Tasks;
using Rzdppk.Api.Requests;

namespace Rzdppk.Controllers
{
    public class TechPassController : BaseController
    {

        private readonly ILogger _logger;
        private readonly ITechPassRepository _techPassRepository;

        public TechPassController
        (
            ILogger<TechPassController> logger,
            ITechPassRepository techPassRepository
        )
        {
            Initialize();
            _logger = logger;
            _techPassRepository = techPassRepository;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter = null)
        {
            await CheckPermission();
            var result = await _techPassRepository.GetAll(skip, limit, filter);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddOrUpdate([FromBody] TechPass input)
        {
            await CheckPermission();
            if (input.Id != 0)
                return Json(await _techPassRepository.Update(input));
            return Json(await _techPassRepository.Add(input));
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Delete([FromBody] DeleteCommonRequestDto request)
        {
            await CheckPermission();
            var result = await _techPassRepository.Delete(request.Id);
            return Json(result);
        }



        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> ById(int id)
        {
            await CheckPermission();
            var result = await _techPassRepository.ById(id);
            return Json(result);
        }

    }
}