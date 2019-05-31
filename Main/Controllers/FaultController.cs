using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using System.Linq;
using System.Threading.Tasks;

namespace Rzdppk.Controllers
{
    public class FaultController : BaseController
    {
        private readonly IDb _db;
        private readonly ILogger _logger;

        public FaultController
            (
                ILogger<FaultController> logger
            )
        {
            _db = new Db();
            base.Initialize();
            _logger = logger;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter)
        {
            await CheckPermission();
            var cer = new FaultsRepository(_logger);
            var result = new FaultsRepository.FaultPaging();
            if (filter != null)
                result = await cer.GetAll(skip, limit, filter);
            else
                result = await cer.GetAll(skip, limit);
            result.Data = result.Data.OrderBy(e => e.Name).ToArray();
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetByEquipmentModelId(int id)
        {
            var sqlREquipmentModel = new EquipmentModelsRepository(_logger);
            var equipmentModel = await sqlREquipmentModel.ById(id);
            if (equipmentModel == null)
                return Json(new Fault[0]);
            await CheckPermission();
            var cer = new FaultsRepository(_logger);
            var result = await cer.GetByEquipmentId(equipmentModel.EquipmentId);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetByEquipmentId(int id)
        {
            await CheckPermission();
            var cer = new FaultsRepository(_logger);
            var result = await cer.GetByEquipmentId(id);
            return Json(result);
        }


        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Add([FromBody]Fault input)
        {
            await CheckPermission();
            var sqlR = new FaultsRepository(_logger);
            Fault result;
            if (input.Id != 0)
                result = await sqlR.Update(input);
            else
                result = await sqlR.Add(input);

            return Json(result);

        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Delete([FromBody]Fault equipmentCategory)
        {
            await CheckPermission();
            var cer = new FaultsRepository(_logger);
            await cer.Delete(equipmentCategory.Id);
            return Json(new { message = "Delete OK" });
        }
    }
}