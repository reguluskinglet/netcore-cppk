using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Model.Dto;

namespace Rzdppk.Controllers
{
    public class TvPanelsController : BaseExternalController
    {
        private readonly ITvPanelRepository _tvPanelRepository;

        public TvPanelsController
        (
            ITvPanelRepository tvPanelRepository,
            IDb db,
            IMemoryCache memoryCache
        )
        {
            base.Initialize(db, memoryCache);

            _tvPanelRepository = tvPanelRepository;
        }

        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> RegisterBox([FromBody]TvBoxRegisterDto box)
        {
            CheckApiKey();

            var id = await _tvPanelRepository.RegisterBox(box);

            return Json(id);
        }

        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetBoxPanels(int boxId)
        {
            CheckApiKey();

            var list = await _tvPanelRepository.GetBoxPanels(boxId);

            return Json(list);
        }

        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> AddBoxPanels([FromBody]TvBoxPanelsDto dto)
        {
            CheckApiKey();

            await _tvPanelRepository.AddBoxPanels(dto);

            return Json("OK");
        }

        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> DeleteBoxPanels([FromBody]TvBoxPanelsDto dto)
        {
            CheckApiKey();

            await _tvPanelRepository.DeleteBoxPanels(dto);

            return Json("OK");
        }

        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetDepoName(int depoStantionId)
        {
            CheckApiKey();

            var name = await _tvPanelRepository.GetDepoName(depoStantionId);

            return Json(name);
        }

        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetScheduleDeviationTable()
        {
            CheckApiKey();

            var res = await _tvPanelRepository.GetScheduleDeviationTable();

            return Json(res);
        }

        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetScheduleDeviationGraph(DateTime start, DateTime end)
        {
            CheckApiKey();

            var res = await _tvPanelRepository.GetScheduleDeviationGraphData(start, end);

            return Json(res);
        }

        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetBrigadeScheduleDeviationTable()
        {
            CheckApiKey();

            var res = await _tvPanelRepository.GetBrigadeScheduleDeviationTable();

            return Json(res);
        }

        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetToDeviationTable()
        {
            CheckApiKey();

            var res = await _tvPanelRepository.GetToDeviationTable();

            return Json(res);
        }

        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetCriticalMalfunctionsTable()
        {
            CheckApiKey();

            var res = await _tvPanelRepository.GetCriticalMalfunctionsTable();

            return Json(res);
        }

        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetTrainsInDepoDepoMalfunctionsTable(int depoStantionId)
        {
            CheckApiKey();

            var res = await _tvPanelRepository.GetTrainsInDepoDepoMalfunctionsTable(depoStantionId);

            return Json(res);
        }

        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetTrainsInDepoStatusTable(int depoStantionId)
        {
            CheckApiKey();

            var res = await _tvPanelRepository.GetTrainsInDepoStatusTable(depoStantionId);

            return Json(res);
        }

        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetJournalsTable(int depoStantionId)
        {
            CheckApiKey();

            var res = await _tvPanelRepository.GetJournalsTable(depoStantionId);

            return Json(res);
        }
    }
}
