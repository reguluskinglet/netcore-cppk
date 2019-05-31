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
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Controllers
{
    public class CategoryEquipmentController : BaseController
    {
        private FilterBody[] _filters;
        private readonly ILogger _logger;

        public CategoryEquipmentController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<CategoryEquipmentController> logger
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
            if (filter != null)
            {
                _filters = JsonConvert.DeserializeObject<FilterBody[]>(filter);
            }
            var cer = new CategoryEquipmentRepository(_logger);
            //var result = new CategoryEquipmentRepository.EquipmentCategoryPaging();
            var result = await cer.GetAll(skip, limit, _filters);
            cer.Dispose();
            return Json(result);


        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Add([FromBody]EquipmentCategory input)
        {
            await CheckPermission();
            var sqlr = new CategoryEquipmentRepository(_logger);
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
        public async Task<JsonResult> Delete([FromBody]EquipmentCategory equipmentCategory)
        {
            await CheckPermission();
            var cer = new CategoryEquipmentRepository(_logger);
            await cer.Delete(equipmentCategory.Id);

            return Json(new { message = "Delete OK" });
        }

        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task<string> GetEquipment()
        //{
        //    var er = new EquipmentRepository();
        //    var result = await er.GetAll(10, 10);
        //    var json = JsonConvert.SerializeObject(result);
        //    return json;
        //}
    }
}