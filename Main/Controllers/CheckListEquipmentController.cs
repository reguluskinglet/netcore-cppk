using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Model.Dto;
using Rzdppk.Model.Enums;

namespace Rzdppk.Controllers
{
    public class CheckListEquipmentController : BaseController
    {
        private readonly ICheckListEquipmentRepository _rep;

        public CheckListEquipmentController
        (
            ICheckListEquipmentRepository rep,
            IDb db,
            IMemoryCache memoryCache
        )
        {
            base.Initialize(db, memoryCache);

            _rep = rep;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetList(int equipmentModelId)
        {
            await CheckPermission();

            var ret = await _rep.GetList(equipmentModelId);

            return Json(ret);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> AddChecklist([FromBody] CheckListEquipmentRepository.CheckListEquipmentDto addDto)
        {
            await CheckPermission();

            var ret = await _rep.AddChecklist(addDto);

            return Json(ret);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> DeleteChecklist(int id)
        {
            await CheckPermission();

            await _rep.DeleteChecklist(id);

            return Json("Ok");
        }
    }
}
