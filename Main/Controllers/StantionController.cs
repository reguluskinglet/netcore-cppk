using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;

namespace Rzdppk.Controllers
{
    public class StantionController : BaseController
    {
        private readonly ILogger _logger;

        public StantionController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<StantionController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter)
        {
            await CheckPermission();
            var sr = new StantionsRepository(_logger);
            var result = new StantionsRepository.StantionPaging();
            if (filter != null)
                result = await sr.GetAll(skip, limit,filter);
            else
                result = await sr.GetAll(skip, limit);

            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetDepot(int skip, int limit)
        {
            await CheckPermission();
            var sr = new StantionsRepository(_logger);
            var result = await sr.GetDepot();

            
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Add([FromBody]Stantion input)
        {
            await CheckPermission();
            if (string.IsNullOrEmpty(input.Description) || string.IsNullOrEmpty(input.Name) || string.IsNullOrEmpty(input.StantionType.ToString()))
                throw new Exception("Some input parameters NULL");
            var sqlr = new StantionsRepository(_logger);
            try
            {
                if (input.Id != 0)
                    await sqlr.Update(input);
                else
                    await sqlr.Add(input);
                return Json(new { message = "addOrUpdate OK" });
            }
            catch (Exception e)
            {
                throw new Exception($"Can't add or update {this.GetType().ToString()} ex = {e}");
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Delete([FromBody]Stantion stantion)
        {
            await CheckPermission();
            var cer = new StantionsRepository(_logger);
            await cer.Delete(stantion.Id);
            return Json(new { message = "Delete OK" });
        }
    }
}