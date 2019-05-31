using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Remotion.Linq.Clauses;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;

namespace Rzdppk.Controllers
{
    public class CarriageController : BaseController
    {
        private readonly ILogger _logger;

        public CarriageController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<CarriageController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetByTrainId(int train_id)
        {
            await CheckPermission();
            var tr = new TrainRepository(_logger);
            var cr = new CarriageRepository(_logger);


            var train = await tr.ByIdWithStations(train_id);
            if (train == null)
                throw new Exception("train not found");

            var result = await cr.GetByTrain(train);

            tr.Dispose();
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetByIdWithEquipment(int id)
        {
            await CheckPermission();
            var cr = new CarriageRepository(_logger);

            var ceq = await cr.GetByIdWithEquipment(id);
            ceq.Equipment = ceq.Equipment.OrderBy(e => e.Location).ThenBy(q => q.Equipment).ToArray();
            return Json(ceq);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Add([FromBody]Carriage carriage)
        {
            await CheckPermission();
            if (string.IsNullOrEmpty(carriage.Serial) || (carriage.TrainId == null))
                throw new Exception("Some input parameters NULL");

            var sqlr = new CarriageRepository(_logger);
            try
            {
                if (carriage.Id != 0)
                {
                    await sqlr.Update(carriage);
                    return Json(new { message = "Update OK" });
                }
                else
                {
                    var res = await sqlr.Add(carriage);
                    return Json(res);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Can't add or update {GetType()} ex = {e}");
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Delete([FromBody]Carriage carriage)
        {
            await CheckPermission();
            var cer = new CarriageRepository(_logger);
            await cer.Delete(carriage.Id);

            return Json(new { message = "Delete OK" });
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        //выцепить вагон и привязать к станции
        public async Task<JsonResult> Unlink([FromQuery] int carriageId, [FromQuery] int stantionId)
        {
            await CheckPermission();

            var cer = new CarriageRepository(_logger);
            await cer.Unlink(carriageId, stantionId);

            return Json(new { message = "Unlink OK" });
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        //прицепить вагон обратно
        public async Task<JsonResult> RestoreLink([FromQuery] int carriageId)
        {
            await CheckPermission();

            var cer = new CarriageRepository(_logger);
            await cer.RestoreLink(carriageId);

            return Json(new { message = "RestoreLink OK" });
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> GetMigrationHistoryTable([FromBody] CarriageRepository.CarriageMigrationHistoryRequest input)
        {
            await CheckPermission();

            var cer = new CarriageRepository(_logger);

            var result = await cer.GetMigrationHistoryTable(input);

            return Json(result);
        }
    }
}