using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using System;
using System.Threading.Tasks;

namespace Rzdppk.Controllers
{
    public class BrigadeController : BaseController
    {
        private readonly ILogger _logger;

        public BrigadeController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<BrigadeController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }

        /// <summary>
        /// Получить список
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetBrigades()
        {
            await CheckPermission();
            var er = new BrigadeRepository(_logger);
            var result = await er.GetAll();
            return Json(result);
        }

        /// <summary>
        /// Получить список с пагинацией
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter)
        {
            await CheckPermission();
            var er = new BrigadeRepository(_logger);
            BrigadeRepository.BrigadePaging result;

            if (filter != null)
                result = await er.GetAll(skip, limit, filter);
            else
                result = await er.GetAll(skip, limit);

            return Json(result);
        }

        /// <summary>
        /// AddOrUpdate
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Add([FromBody]Brigade input)
        {
            await CheckPermission();
            var sqlR = new BrigadeRepository(_logger);
            if (input.Id != 0)
                await sqlR.Update(input);
            else
                await sqlR.Add(input);
            return Json(new { message = "addOrUpdate OK" });

        }

        /// <summary>
        /// Удалить по ID
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Delete([FromBody]Brigade equipment)
        {
            await CheckPermission();
            var sqlR = new BrigadeRepository(_logger);
            var result = await sqlR.Delete(equipment.Id);
            return Json(result);
        }
    }
}
