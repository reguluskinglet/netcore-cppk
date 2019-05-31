using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Newtonsoft.Json;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Services;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Core.ViewModels;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Controllers
{
    public class TurnoversController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly ICommonService _commonService;

        public TurnoversController
        (
            ILogger<TurnoversController> logger,
            IDb db,
            ICommonService commonService
        )
        {
            base.Initialize();
            _logger = logger;
            _db = db;
            _commonService = commonService;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter = null)
        {
            await CheckPermission();
            var er = new TurnoversRepoisitory(_logger);
            var result = await er.GetAll(skip, limit, filter);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddOrUpdate([FromBody] Turnover input)
        {
            //await CheckPermission();
            //if (input.Days == null || input.Days.Count == 0)
            //    throw new ValidationException("Не заполнены дни циклового графика");

            var sqlR = new TurnoversRepoisitory(_logger);
            if (input.Id != 0)
                return Json(await sqlR.Update(input));
            return Json(await sqlR.Add(input));
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task Delete(int id)
        {
            await CheckPermission();
            var sqlR = new TurnoversRepoisitory(_logger);
            await sqlR.Delete(id);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> GetTurnoverById(int? turnoverId)
        {
            var turnover = turnoverId.HasValue
                ? await _db.Connection.QueryFirstAsync<TurnoverUI>(Sql.SqlQueryCach["Turnovers.GetById"],
                    new {id = turnoverId})
                : new TurnoverUI();

            turnover.Days = await _db.Connection.QueryAsync<int>(Sql.SqlQueryCach["Turnovers.GetDaysByTurnoverId"], new { id = turnoverId });
            turnover.Directions = await _commonService.GetDirectionsSelectItem();

            return Ok(turnover);
        }

        ///// <summary>
        ///// AddOrUpdate
        ///// </summary>
        ///// <param name="input"></param>
        ///// <returns></returns>
        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task<JsonResult> Add([FromBody]Brigade input)
        //{
        //    await CheckPermission();
        //    var sqlr = new BrigadeRepository(_logger);
        //    //var sqlBr = new BrigadeTypeRepository();
        //    //if (sqlBr.ByIdWithStations(input.BrigadeTypeId) == null)
        //    //    throw new Exception("some input id null");

        //    try
        //    {
        //        if (input.Id != 0)
        //            await sqlr.Update(input);
        //        else
        //            await sqlr.Add(input);
        //        sqlr.Dispose();
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
        //public async Task<JsonResult> Delete([FromBody]Brigade equipment)
        //{
        //    await CheckPermission();
        //    var er = new BrigadeRepository(_logger);
        //    await er.Delete(equipment.Id);
        //    er.Dispose();
        //    return Json(new { message = "Delete OK" });
        //}
    }


    public class TurnoverUI
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DirectionId { get; set; }//Directions
        public IEnumerable<int> Days { get; set; } =new List<int>();

        public IEnumerable<SelectItem> Directions { get; set; }
    }
}
