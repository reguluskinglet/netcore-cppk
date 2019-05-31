using System;
using System.Collections.Generic;
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
using static Rzdppk.Core.Repositoryes.EquipmentRepository;

namespace Rzdppk.Controllers
{
    public class EquipmentController : BaseController
    {
        private readonly ILogger _logger;

        public EquipmentController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<EquipmentController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [Obsolete]
        public async Task<JsonResult> GetEquipmentWithCheckLists(int model_id, int parent_id, int skip, int limit, string filter, List<int> ids)
        {
            await CheckPermission();
            var er = new EquipmentRepository(_logger);
            var result = new EquipmentUIPaging();
            if (filter != null)
                result = await er.GetEquipmentWithCheckLists(model_id, parent_id, skip, limit, filter);
            else
                result = await er.GetEquipmentWithCheckLists(model_id, parent_id, skip, limit);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetEquipment(int model_id, int parent_id, int skip, int limit, string filter)
        {
            await CheckPermission();
            var er = new EquipmentRepository(_logger);
            var result = new NewEquipmentUIPaging();
            if (filter != null)
                result = await er.GetEquipment(model_id, parent_id, skip, limit, filter);
            else
                result = await er.GetEquipment(model_id, parent_id, skip, limit);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [Obsolete]
        public async Task<JsonResult> AddUpdateEquipmentWithCheckLists([FromBody]CheckListEquipmentUI ces)
        {
            await CheckPermission();
            var er = new EquipmentRepository(_logger);
            var ret = await er.AddOrUpdateEquipmentWithCheckLists(ces);
            return Json(ret);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddUpdateEquipment([FromBody]EquipmentUI ces)
        {
            await CheckPermission();
            var er = new EquipmentRepository(_logger);
            var ret = await er.AddOrUpdateEquipment(ces);
            return Json(ret);
        }

        /// <summary>
        /// Отхуярить неисправность от оборудования
        /// </summary>
        /// <param name="faultEquipment"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> RemoveFaultFromEquipment([FromBody]FaultEquipment faultEquipment)
        {
            await CheckPermission();
            var mr = new EquipmentRepository(_logger);
            await mr.RemoveFaultFromEquipment(faultEquipment);
            return Json(new { message = "Delete OK" });
        }

        /// <summary>
        /// Прихуярить неисправность к оборудованию
        /// </summary>
        /// <param name="faultEquipment"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddFaultToEquipment([FromBody]FaultEquipment faultEquipment)
        {
            await CheckPermission();
            try
            {
                var mr = new EquipmentRepository(_logger);
                await mr.AddFaultToEquipment(faultEquipment);
                return Json(new { message = "Add OK" });
            }
            catch (Exception e)
            {
                return Json(e);
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddNewFaultToEquipment(string faultName, int equipmentId, int faultType)
        {
            await CheckPermission();
            var sqlRe = new EquipmentRepository(_logger);
            return Json(await sqlRe.AddNewFaultToEquipment(faultName, equipmentId, faultType));

        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit)
        {
            await CheckPermission();
            var er = new EquipmentRepository(_logger);
            var result = await er.GetAll(skip, limit);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetByCategoryId(int category_id, int skip, int limit, string filter)
        {
            await CheckPermission();
            EquipmentPaging result;
            var er = new EquipmentRepository(_logger);
            var cer = new CategoryEquipmentRepository(_logger);
            var category = await cer.GetById(category_id);
            if (category != null)
            {
                result = await er.GetByCategory(category, skip, limit, filter);
            }
            else
            {
                throw new Exception("category_id not exist");
            }
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Add([FromBody]EquipmentWitcAct input)
        {
            await CheckPermission();
            var sqlR = new EquipmentRepository(_logger);
            var sqlRcer = new CategoryEquipmentRepository(_logger);

            var item = await sqlRcer.GetById(input.CategoryId);
            if (item == null)
                throw new Exception("some input id null");

            EquipmentWitcAct result;
            if (input.Id != 0)
                result = await sqlR.Update(input);
            else
                result = await sqlR.Add(input);
            
            sqlRcer.Dispose();
            return Json(result);

        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Delete([FromBody]Equipment equipment)
        {
            await CheckPermission();
            var er = new EquipmentRepository(_logger);
            await er.Delete(equipment.Id);
            return Json(new { message = "Delete OK" });
        }
    }
}
