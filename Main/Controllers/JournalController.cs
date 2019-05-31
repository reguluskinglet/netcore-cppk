using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Services;
using Rzdppk.Model;

namespace Rzdppk.Controllers
{
    public class JournalController : BaseController
    {
        private readonly ILogger _logger;

        public JournalController
        (
            ILogger<JournalController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter = null)
        {
            await CheckPermission();
            var service = new JournalService(_logger);
            var result = await service.GetJournalInspectionAndTasks(skip, limit, filter);
            //var result = new ModelRepository.ModelPaging();
            //if (filter != null)
            //    result = await er.GetAll(skip, limit, filter);
            //else
            //    result = await er.GetAll(skip, limit);
            return Json(result);
        }
    }
}