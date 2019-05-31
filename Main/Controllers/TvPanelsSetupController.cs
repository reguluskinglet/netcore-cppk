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

namespace Rzdppk.Controllers
{
    public class TvPanelsSetupController : BaseController
    {
        private readonly ITvPanelSetupRepository _tvPanelRepository;

        public TvPanelsSetupController
        (
            ITvPanelSetupRepository tvPanelRepository,
            IDb db,
            IMemoryCache memoryCache
        )
        {
            base.Initialize(db, memoryCache);

            _tvPanelRepository = tvPanelRepository;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetAllBoxes(int skip, int limit, string filter)
        {
            await CheckPermission();

            var result = new TvPanelSetupPaging();

            if (filter != null)
                result = await _tvPanelRepository.GetAllBoxes(skip, limit, filter);
            else
                result = await _tvPanelRepository.GetAllBoxes(skip, limit);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpDelete]
        public async Task<JsonResult> DeleteBox(int boxId)
        {
            await CheckPermission();

            await _tvPanelRepository.DeleteBox(boxId);

            return Json("Ok");
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> GetTable([FromBody] TvPanelRequest input)
        {
            await CheckPermission();

            var list = await _tvPanelRepository.GetTable(input);

            return Json(list);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpGet]
        public async Task<JsonResult> GetAllScreenTypes()
        {
            await CheckPermission();

            var list = _tvPanelRepository.GetAllScreenTypes();

            return Json(list);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> AddPanel([FromBody]PanelAddDto addDto)
        {
            await CheckPermission();

            int panelId =   await _tvPanelRepository.AddPanel(addDto);

            return Json(new { Result = "Ok", Id =  panelId});
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> ChangePanelType([FromBody]PanelTypeUpdateDto updateDto)
        {
            await CheckPermission();

            await _tvPanelRepository.ChangePanelType(updateDto.Id, updateDto.Type);

            return Json("Ok");
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpDelete]
        public async Task<JsonResult> DeletePanel(int panelId)
        {
            await CheckPermission();

            await _tvPanelRepository.DeletePanel(panelId);

            return Json("Ok");
        }
    }
}
