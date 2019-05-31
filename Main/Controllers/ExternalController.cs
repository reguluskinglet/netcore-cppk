using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Interfaces;

namespace Rzdppk.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ExternalController : Controller
    {
        private readonly IExternalRepository _externalRepository;

        public ExternalController
            (
                IExternalRepository externalRepository
            )
        {
            _externalRepository = externalRepository;
        }

        public async Task<IActionResult> GetAllData([FromBody]DateTime? date)
        {
            return Ok(await _externalRepository.GetAllData(date));
        }

        [HttpPost]
        public IActionResult SaveTerminalResult([FromBody]ExternalRepository.TerminalResultDto model)
        {
            return Ok(_externalRepository.SaveTerminalResult(model));
        }

        [HttpPost]
        public IActionResult SaveDocument(List<ExternalRepository.UploadDocumentDto> model)
        {
            return Ok(_externalRepository.SaveDocuments(model));
        }

        [HttpPost]
        public async Task<IActionResult> SaveDeviceValue([FromBody]ExternalRepository.DeviceValueDto model)
        {
            return Ok(await _externalRepository.SaveDeviceValue(model));
        }
    }
}
