using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Services;
using static Rzdppk.Core.Other.DevExtremeTableData;



namespace Rzdppk.Controllers
{
    public class EventsTableController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        //private readonly ITvPanelSetupRepository _rep;

        public EventsTableController
        (
            ILogger<EventsTableController> logger,
            IMapper mapper, 
            ITvPanelSetupRepository rep
        )
        {
            base.Initialize();
            _logger = logger;
            _mapper = mapper;
            //_rep = rep;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Reports()
        {
            await CheckPermission();

            var result = new List<ReportList>
            {
                new ReportList {Id = (int)ReportsTableEnum.Inspections, Name = "События"},
                new ReportList {Id = (int)ReportsTableEnum.TaskReport, Name = "Отчет по задачам"},
                new ReportList {Id = (int)ReportsTableEnum.HistoryEquipmentsFault, Name = "История неисправности оборудования"},
                new ReportList {Id = (int)ReportsTableEnum.Tasks, Name = "Инциденты"},
            };
            return Json(result);
        }

        [HttpGet]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> InspectionByIdForEventTable(int id)
        {
            await CheckPermission();
            var service = new EventsTableService(_logger, _mapper);
            var result = await service.InspectionByIdForEventTable(id);
            return Json(result);
        }



        [HttpPost]
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> EventTable([FromBody] ReportRequest input)
        {
            if (!int.TryParse(input.ReportId, out var reportId))
                throw new ValidationException("Id отчета не int");

            await CheckPermission();
            switch (reportId)
            {
                case (int) ReportsTableEnum.Inspections:
                {
                    var service = new EventsTableService(_logger, _mapper);
                    return Json(await service.GetTaskAndInspections(input));
                }
                case (int)ReportsTableEnum.TaskReport:
                {
                    var service = new ReportTableService(_logger, _mapper);
                    return Json(await service.GetReportTasks(input));
                }
                case (int)ReportsTableEnum.Tasks:
                {
                    var service = new EventsTableService(_logger, _mapper);
                    return Json(await service.GetTaskAndInspections(input));
                }
                case (int)ReportsTableEnum.TaskAndInspections:
                {
                    var service = new EventsTableService(_logger, _mapper);
                    input.ReportId = "1";
                    return Json(await service.GetTaskAndInspections(input));
                }
                default:
                    throw new ValidationException($"Неизвестный Id отчета: {reportId}");
            }
        }




        public class ReportList
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}