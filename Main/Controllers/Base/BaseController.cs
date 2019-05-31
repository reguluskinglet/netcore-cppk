using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Controllers.Base
{
    public class BaseController : Controller
    {
       // private static IDb _db;
        private static IMemoryCache _memoryCache;
        private bool _initialized = false;
        private ILogger<BaseController> _logger;

        public void Initialize()
        {
          //  _db = new Db();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _initialized = true;
        }

        public void Initialize(IDb db, IMemoryCache memoryCache)
        {
          //  _db = db;
            _memoryCache = memoryCache;
            _initialized = true;
        }

        public async Task<User> GetCurrentUser()
        {
            var userName = HttpContext.User.Identity.Name;

            var ur = new UserRepository();
            var user = await ur.GetUserByLogin(userName);
            ur.Dispose();
            return user;
        }

        public async Task<User> CheckPermission()
        {
            if (!_initialized)
                throw new Exception("base controller not initialized");

            var user = await GetCurrentUser();
            var permissions = user.Role?.Permissions ?? 0;

            var actionName = ControllerContext.RouteData.Values["action"].ToString();
            var controllerName = ControllerContext.RouteData.Values["controller"].ToString();

            var pr = new PermissionRepository();
            var needPermissionBits = pr.GetPermissionBits(controllerName, actionName);
            var res = permissions & needPermissionBits;
            pr.Dispose();
            if (res == needPermissionBits)
            {
                return user;
            }
            //TODO вернуть после расстановки прав в БД
            //throw new Exception("no permission to access");
            return user;
        }
    }
}
