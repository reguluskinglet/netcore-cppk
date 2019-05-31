using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Services;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Controllers
{
    public class InspectionRoutesController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public InspectionRoutesController
        (
            ILogger<InspectionRoutesController> logger,
            IMapper mapper
        )
        {
            Initialize();
            _logger = logger;
            _mapper = mapper;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter = null)
        {
            await CheckPermission();
            var er = new InspectionRoutesRepository(_logger);
            var result = await er.GetAll(skip, limit, filter);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddOrUpdate([FromBody] InspectionRoute input)
        {
            await CheckPermission();
            var timeLineTimeStart = DateTime.MinValue.AddHours(3);
            if (input.Start < timeLineTimeStart && input.End > timeLineTimeStart)
                throw new ValidationException(Error.IncorrectCorrectTimeRange);
            var service = new ScheduleCycleService(_logger, _mapper);
            return Json(await service.AddOrUpdateInspectionOnRoute(input));
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task Delete(int id)
        {
            await CheckPermission();
            var sqlR = new InspectionRoutesRepository(_logger);
            sqlR.Delete(id);
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
}
