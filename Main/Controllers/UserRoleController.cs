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
using Rzdppk.Model.Auth;

namespace Rzdppk.Controllers
{
    public class UserRoleController : BaseController
    {
        private readonly ILogger _logger;

        public UserRoleController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<UserRoleController> logger
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
        public async Task<JsonResult> GetAll(int skip, int limit, string filter)
        {
            await CheckPermission();
            var er = new UserRoleRepository();
            var result = await er.GetAll(skip, limit);
            er.Dispose();
            return Json(result);
        }


        /// <summary>
        /// AddOrUpdate
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task AddOrUpdate([FromBody]UserRoleRepository.UserRoleUi input)
        {
            await CheckPermission();
            var sqlr = new UserRoleRepository();
            await sqlr.AddUpdateUserRole(input);
            sqlr.Dispose();
            //return Json(new { message = "addOrUpdate OK" });
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task Delete([FromBody]UserRole input)
        {
            await CheckPermission();
            var sqlR = new UserRoleRepository();
            await sqlR.Delete(input.Id);
            sqlR.Dispose();

        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAuthorityArray()
        {
            await CheckPermission();
            var sqlR = new UserRoleRepository();
            var result = sqlR.GetAuthorityArray();
            sqlR.Dispose();
            return Json(result);
        }
    }
}
