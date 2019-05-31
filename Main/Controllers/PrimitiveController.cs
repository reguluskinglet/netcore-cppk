using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Controllers
{
    public class PrimitiveController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public PrimitiveController
        (
            IMemoryCache memoryCache,
            ILogger<PrimitiveController> logger,
            IMapper mapper
        )
        {
            base.Initialize();
            _logger = logger;
            _mapper = mapper;
        }


        //[Authorize]
        //[Route("api/[controller]/[action]")]
        //public async Task<JsonResult> GetAll(int skip, int limit, string filter)
        //{
        //    var sqlR = new PrimitiveRepository(_logger);
        //    var result = await sqlR.GetAll<Equipment>(skip, limit);
        //    return Json(result);

        //}
        //private async Task<JsonResult> GetAllData<T>(int skip, int limit, string filter)
        //{
        //    var sqlR = new PrimitiveRepository<T>(_logger);
        //    var result = await sqlR.GetAll(skip, limit, filter);
        //    return Json(result);

        //}

    }
}
