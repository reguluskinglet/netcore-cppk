using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Rzdppk.Controllers.Base;
using Rzdppk.Controllers.DepoEvents.UI;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Core.ViewModels;

namespace Rzdppk.Controllers.DepoEvents
{
    public class DepoEventsController : BaseController
    {
        private readonly IDepoEventsRepository _depoEventsRepository;
        private readonly IDb _db;
        private readonly ICommonService _commonService;

        public DepoEventsController
        (
            IDepoEventsRepository depoEventsRepository,
            IDb db,
            IMemoryCache memoryCache,
            ICommonService commonService
        )
        {
            base.Initialize(db, memoryCache);
            _db = db;
            _depoEventsRepository = depoEventsRepository;
            _commonService = commonService;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> AddOrUpdate([FromBody] DepoEventDtoUi model)
        {
            await CheckPermission();

            _db.Connection.Execute(model.Id.HasValue ? Sql.SqlQueryCach["DepoEvents.Update"] : Sql.SqlQueryCach["DepoEvents.Insert"],model);

            return Ok();
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<IActionResult> GetById(int? id)
        {
            await CheckPermission();

            DepoEventDtoUi model = new DepoEventDtoUi();

            if (id.HasValue)
                model = await _db.Connection.QueryFirstAsync<DepoEventDtoUi>(Sql.SqlQueryCach["DepoEvents.GetById"], new
                {
                    id = id
                });

            model.DepoEventDataSource = await _commonService.GetDepoEventDataSource();

            if (id.HasValue)
                model.DepoEventDataSource.Inspections = await GetInspectionsSelectItem(model.TrainId);

            return Ok(model);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<IActionResult> GetInspections(int trainId)
        {
            await CheckPermission();

            return Ok(await GetInspectionsSelectItem(trainId));
        }

        //TODO сделать нормально
        private async Task<IEnumerable<SelectItem>> GetInspectionsSelectItem(int trainId)
        {
            return (await _depoEventsRepository.GetInspections(trainId, null)).Select(x => new SelectItem
            {
                Value = x.Id,
                Text = x.Name
            });
        }


        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody]DepoEventsDeleteOptions req)
        {
            await CheckPermission();

            var now = DateTime.Now;
            string sql = Sql.SqlQueryCach["DepoEvents.Delete"];

            if(req.ids.Count() > 0)
            {
                if (req.IsSelectedAll == true)
                    sql += $"{Environment.NewLine} WHERE Id NOT IN @ids";
                else
                    sql += $"{Environment.NewLine} WHERE Id IN @ids";
            }
            else
            {
                if (req.IsInDepo == true)
                    sql += $"{Environment.NewLine} WHERE InTime <= '{now}' AND (OutTime IS NULL OR OutTime >= '{now}')";
            }

            await _db.Connection.ExecuteAsync(sql, new {ids = req.ids});

            return Ok();
        }

        public class DepoEventsDeleteOptions
        {
            public int[] ids { get; set; }
            public bool IsSelectedAll { get; set; }
            public bool? IsInDepo { get; set; }
        }

    }
}
