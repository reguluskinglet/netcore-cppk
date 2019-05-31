using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Rzdppk.Controllers
{
    public class UserController : BaseController
    {
        private readonly ILogger _logger;

        public UserController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<UserController> logger
        )
        {
            base.Initialize();
            _logger = logger;

        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddOrUpdate([FromBody]User input)
        {
            await CheckPermission();
            if (input == null)
                throw new Microsoft.Rest.ValidationException(Error.ParserError);
            var sqlR = new UserRepository(_logger);
            var result = await sqlR.AddOrUpdate(input);
            return Json(result);
        }
        
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task Delete([FromBody]User input)
        {
            await CheckPermission();
            var er = new UserRepository(_logger);
            await er.Delete(input.Id);
            er.Dispose();
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter)
        {
            await CheckPermission();
            var sqlr = new UserRepository(_logger);
            var result = new UserRepository.UserPaging();
            if (filter != null)
                result = await sqlr.GetAll(skip, limit, filter);
            else
                result = await sqlr.GetAll(skip, limit);
            sqlr.Dispose();
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAllWithLogin(int skip, int limit, string filter = null, string sort = null)
        {
            var filterObj = filter!=null? JToken.Parse(filter):null;
            await CheckPermission();
            var sqlr = new UserRepository(_logger);
            var search = filterObj?.FirstOrDefault()?.SelectToken("value", false)?.ToString();
            var result = await sqlr.GetAllWithLogin(skip, limit, search, sort);
            sqlr.Dispose();
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAllWithOutLogin(int skip, int limit)
        {
            await CheckPermission();
            var sqlr = new UserRepository(_logger);
            var result = await sqlr.GetAllWithOutLogin(skip, limit);
            sqlr.Dispose();
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddStaff([FromBody]User input)
        {
            await CheckPermission();
            var sqlR = new UserRepository(_logger);

            if (string.IsNullOrWhiteSpace(input.PersonNumber) ||
                string.IsNullOrWhiteSpace(input.Name) ||
                string.IsNullOrWhiteSpace(input.PersonNumber))
            {
                throw new ValidationException(Error.NotFilledOptionalField);
            }

            if (input.Id != 0)
                await sqlR.UpdateStaff(input);
            else
                await sqlR.AddStaff(input);
            sqlR.Dispose();
            return Json(new { message = "addOrUpdate OK" });
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> DeleteStaff([FromBody]UserInternal ub)
        {
            await CheckPermission();
            var ur = new UserRepository(_logger);
            var pb = new PlaneBrigadeTrainsRepository(_logger);
            var pk = new PlanedRouteTrainsRepository(_logger);
            var user = await ur.GetStaffById(ub.UserId);

            if (user == null)
                throw new Exception("user.Id not found");

            var planeBrigades = await pb.ByUserId(user.Id);
            if(planeBrigades.Count > 0)
                throw new Exception("Сотрудник уже назначен на планирование поездов");

            var planedRoutes = await pk.ByUserId(user.Id);
            if (planedRoutes.Count > 0)
                throw new Exception("Сотрудник уже назначен на планирование маршрутов");

            await ur.DeleteStaff(user);
            ur.Dispose();

            return Json(new { message = "Delete OK" });
        }


        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task<JsonResult> GetStaffByBrigadeId(int brigade_id)
        //{
        //    await CheckPermission();
        //    var ur = new UserRepository(_logger);
        //    var br = new BrigadeRepository(_logger);

        //    var brigade = await br.ById(brigade_id);

        //    if (brigade == null)
        //        throw new Exception("brigade_id not exist");

        //    var result = await ur.GetStaffByBrigade(brigade);
        //    ur.Dispose();
        //    return Json(result);
        //}

        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task<JsonResult> AddStaffToBrigade([FromBody]UserInternal ub)
        //{
        //    await CheckPermission();
        //    var ur = new UserRepository(_logger);
        //    var br = new BrigadeRepository(_logger);

        //    var brigade = await br.ById(ub.BrigadeId);
        //    var user = await ur.GetStaffById(ub.UserId);

        //    if (brigade == null)
        //        throw new Exception("brigadeId not found");

        //    if (user == null)
        //        throw new Exception("userId not found");

        //    await ur.AddStaffToBrigade(user, brigade);
        //    ur.Dispose();
        //    return Json(new { message = "AddToBrigade OK" });
        //}

        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task<JsonResult> DeleteStaffFromBrigade([FromBody]UserInternal ub)
        //{
        //    await CheckPermission();
        //    var ur = new UserRepository(_logger);
        //    var user = await ur.GetStaffById(ub.UserId);

        //    if (user == null)
        //        throw new Exception("user.Id not found");

        //    if (user.Brigade == null)
        //        throw new Exception("user not added to brigade");

        //    await ur.DeleteStaffFromBrigade(user);
        //    ur.Dispose();

        //    return Json(new { message = "DeleteFromBrigade OK" });
        //}
    }

    public class UserInternal
    {
        public int UserId { get; set; }
    }
}
