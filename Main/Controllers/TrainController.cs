using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using Rzdppk.Model.Enums;

namespace Rzdppk.Controllers
{
    public class TrainController : BaseController
    {
        private readonly ILogger _logger;

        public TrainController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<TrainController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter)
        {
            await CheckPermission();
            var sr = new TrainRepository(_logger);
            var result = new TrainRepository.TrainPaging();
            if (filter != null)
                result = await sr.GetAll(skip, limit, filter);
            else
                result = await sr.GetAll(skip, limit);
            sr.Dispose();
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Add([FromBody]Train train)
        {
            await CheckPermission();
            if (string.IsNullOrEmpty(train?.Name) || train.StantionId == null || train.StantionId < 1)
                throw new ValidationException(Error.NotFilledOptionalField);
            var sqlr = new TrainRepository(_logger);
            if (train.Id != 0)
                return (Json(await sqlr.Update(train)));
            return Json(await sqlr.Add(train));

        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Delete([FromBody]Train train)
        {
            await CheckPermission();
            var cer = new TrainRepository(_logger);
            await cer.Delete(train.Id);

            return Json(new { message = "Delete OK" });
        }
    }
}