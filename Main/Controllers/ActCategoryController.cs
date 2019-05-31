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
    public class ActCategoryController : BaseController
    {
        private readonly ILogger _logger;

        public ActCategoryController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<ActCategoryController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll()
        {
            await CheckPermission();
            var sqlR = new ActCategoriesRepository();
            var result = await sqlR.GetAll();
            sqlR.Dispose();
            return Json(result);
        }


        ///// <summary>
        ///// AddOrUpdate
        ///// </summary>
        ///// <param name="input"></param>
        ///// <returns></returns>
        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public JsonResult Add([FromBody]Brigade input)
        //{
        //    CheckPermission();
        //    var sqlr = new BrigadeRepository();
        //    //var sqlBr = new BrigadeTypeRepository();
        //    //if (sqlBr.ByIdWithStations(input.BrigadeTypeId) == null)
        //    //    throw new Exception("some input id null");

        //    try
        //    {
        //        if (input.Id != 0)
        //            sqlr.Update(input);
        //        else
        //            sqlr.Add(input);
        //        return Json(new { message = "addOrUpdate OK" });
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception($"Can't add or update {this.GetType().ToString()} ex = {e}");
        //    }
        //}

        ///// <summary>
        ///// Удалить по ID
        ///// </summary>
        ///// <param name="equipment"></param>
        ///// <returns></returns>
        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public JsonResult Delete([FromBody]Brigade equipment)
        //{
        //    CheckPermission();
        //    var er = new BrigadeRepository();
        //    er.Delete(equipment.Id);

        //    return Json(new { message = "Delete OK" });
        //}
    }
}
