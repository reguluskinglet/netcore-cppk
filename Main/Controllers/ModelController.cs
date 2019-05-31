using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rzdppk.Controllers
{
    public class ModelController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ModelController
        (
            ILogger<ModelController> logger,
            IMapper mapper
        )
        {
            base.Initialize();
            _mapper = mapper;
            _logger = logger;
        }


        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> CloneModelWithChild([FromBody] Model.Model model)
        {
            await CheckPermission();
            var sqlR = new ModelService(_logger, _mapper);
            var res = await sqlR.CloneModelWithChild(model);
            return Json(res);
        }


        /// Прихуярить Модель к оборудованию с указанием местоположения или без
        //добавление местоположения (парент_ид=нулл) или оборудования в местоположение (ид местоположения=парент_ид)
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddEquipmentToModel([FromBody]Model.EquipmentModel equipmentModel)
        {
            await CheckPermission();
            var sqlR = new ModelRepository(_logger);
            var res = await sqlR.AddEquipmentToModel(equipmentModel);
            return Json(res);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> DeleteEquipmentFromModel([FromBody]Model.EquipmentModel equipmentModel)
        {
            await CheckPermission();
            var mr = new ModelRepository(_logger);
            var er = new EquipmentRepository(_logger);
            var eq = await er.GetCheckListByEquipmentModelId(equipmentModel.Id);
            if (eq == null)
                throw new Exception("EquipmentModel not found");

            //if (!er.IsEquipmentChecklistsEmpty(eq))
            //    throw new Exception("Checklists not empty");
            await er.DeleteEquipmentWithCheckLists(eq);

            await mr.DeleteEquipmentFromModel(equipmentModel.Id);
            return Json(new { message = "Delete OK" });
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetEquipmentsByModel(int model_id)
        {
            await CheckPermission();
            var mr = new ModelRepository(_logger);
            var m = await mr.GetById(model_id);
            if (m == null)
                throw new Exception("model not found");

            var result = await mr.GetEquipmentByModel(m);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetEquipmentsByCarriage(int carriage_id, bool isMark = false, bool sortToTaskList = false)
        {
            await CheckPermission();
            var cr = new CarriageRepository(_logger);
            var mr = new ModelRepository(_logger);

            var carriage = await cr.GetById(carriage_id);

            if (carriage == null)
                throw new Exception("carriage not found");

            var eq = await mr.GetEquipmentByModel(carriage.Model);
            var result = eq.Select(row => new ModelRepository.EquipmentTmp
            {
                EquipmentId = row.Equipment.Id,
                EquipmentModelId = row.Id,
                EquipmentName = row.Equipment.Name,
                IsMark = row.IsMark,
                ParentId = row.ParentId,
                Id = row.Id
            }).ToArray();

            if (isMark)
                result = result.Where(e => e.IsMark).ToArray();

            if (sortToTaskList)
            {
                var main = result.Where(e => e.ParentId == 0).ToArray();
                var sortResult = new List<ModelRepository.EquipmentTmp>();
                foreach (var item in main)
                {
                    sortResult.Add(item);
                    sortResult.AddRange(result.Where(e => e.ParentId == item.Id).OrderBy(q => q.EquipmentName));
                }
                foreach (var item in sortResult)
                {
                    if (item.ParentId != 0)
                        item.EquipmentName = $"  {item.EquipmentName}";
                }

                result = sortResult.ToArray();
            }

            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter)
        {
            await CheckPermission();
            var er = new ModelRepository(_logger);
            var result = new ModelRepository.ModelPaging();
            if (filter != null)
                result = await er.GetAll(skip, limit, filter);
            else
                result = await er.GetAll(skip, limit);
            return Json(result);
        }

        /// <summary>
        /// Внезапно addorUpdate
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Add([FromBody]Model.Model input)
        {
            await CheckPermission();
            if (string.IsNullOrEmpty(input.Name) || string.IsNullOrEmpty(input.ModelType.ToString()))
                throw new Exception("Some input parameters NULL");
            var sqlR = new ModelRepository(_logger);
            if (input.Id != 0)
            {
                await sqlR.Update(input);
                return Json(new { message = "Update OK" });
            }
            return Json(await sqlR.Add(input));


        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Delete([FromBody]Model.Model model)
        {
            await CheckPermission();
            var er = new ModelRepository(_logger);
            await er.Delete(model.Id);
            return Json(new { message = "Delete OK" });
        }
    }
}
