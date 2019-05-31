using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;

namespace Rzdppk.Controllers
{
    public class TemplateLabelsController : BaseController
    {
        private readonly ILogger _logger;

        public TemplateLabelsController
        (
            ILogger<TemplateLabelsController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }

        /// <summary>
        /// Получить список с пагинацией
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit)
        {
            await CheckPermission();
            var sqlR = new TemplateLabelsRepository();
            var result = await sqlR.GetAll(skip, limit);
            sqlR.Dispose();
            
            return Json(result);
        }


        /// <summary>
        /// AddOrUpdate
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task AddOrUpdate([FromBody]TemplateLabel input)
        {
            await CheckPermission();
            var sqlR = new TemplateLabelsRepository();
            await sqlR.AddOrUpdate(input);
            sqlR.Dispose();
        }

        /// <summary>
        /// Удалить по ID
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task Delete([FromBody]TemplateLabel equipment)
        {
            await CheckPermission();
            var er = new TemplateLabelsRepository();
            await er.Delete(equipment.Id);
            er.Dispose();
        }
    }
}
