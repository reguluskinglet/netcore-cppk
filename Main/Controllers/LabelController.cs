using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;

namespace Rzdppk.Controllers
{
    public class LabelController : BaseController
    {

        private readonly ILogger _logger;

        public LabelController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<LabelController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }


        /// <summary>
        /// Получить задачи
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetTaskPrints(int skip, int limit, string filter)
        {
            await CheckPermission();
            var sqlR = new LabelRepository();
            var result = await sqlR.GetTaskPrints(skip, limit, filter);
            sqlR.Dispose();
            return Json(result);
        }


        /// <summary>
        /// получить потроха задачи
        /// </summary>
        /// <param name="taskPrintId"></param>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAllLabelsByTaskPrintId(int taskPrintId,int skip, int limit)
        {
            await CheckPermission();
            var sqlR = new LabelRepository();
            var result = await sqlR.GetTaskPrintItemsByTaskPrintsIdUi(taskPrintId, skip, limit);
            sqlR.Dispose();
            return Json(result);
        }

        /// <summary>
        /// Получить список выбранных задач с обновлением времени печати
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetSelectedLabelsByTaskPrintId([FromBody] TaskPrintSelectLabels req)
        {
            await CheckPermission();
            var sqlR = new LabelRepository();
            var result = await sqlR.GetTaskSelectedItemsByTaskPrintsIdUi(req.Id, req.IsSelectedAll, req.SelectedRows);
            sqlR.Dispose();
            return Json(result);
        }

        /// <summary>
        /// обновить задачу или добавить новую
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddOrUpdateTaskPrints([FromBody]TaskPrint input)
        {
            var user = await CheckPermission();
            input.User = user;
            var sqlr = new LabelRepository();
            var id = await sqlr.AddOrUpdateTaskPrints(input);
            sqlr.Dispose();
            return Json(new {id = id});
        }


        /// <summary>
        /// получить все шаблоны (Серьезно???)
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAllTemplateLabels()
        {
            await CheckPermission();
            var sqlR = new LabelRepository();
            var result = await sqlR.GetAllTemplateLabels();
            sqlR.Dispose();
            return Json(result);
        }


        /// <summary>
        /// Удалить по ID
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task DeleteTaskPrints([FromBody]TaskPrint input)
        {
            await CheckPermission();
            var sqlR = new LabelRepository();
            await sqlR.DeleteTaskPrints(input.Id);
            sqlR.Dispose();
        }


        /// <summary>
        /// Добавить метку
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> AddLabelWithTaskPtintsItem([FromBody]LabelRepository.LabelWithTaskPrintId input)
        {
            var user = await CheckPermission();
            var sqlR = new LabelRepository();
            var id = await sqlR.AddLabelWithTaskPtintsItem(input, user);
            sqlR.Dispose();
            return Json(new { id = id });
        }


        /// <summary>
        /// Удалить связку по ИД
        /// </summary>
        /// <param name="input"></param>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task DeleteTaskPrintItem([FromBody]TaskPrint input)
        {
            await CheckPermission();
            var sqlR = new LabelRepository();
            await sqlR.DeleteTaskPrintItem(input.Id);
            sqlR.Dispose();
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task UpdateTimePrinted([FromBody]LabelRepository.TaskPrintItemWithTimePrintedDateTime input)
        {
            await CheckPermission();
            var sqlR = new LabelRepository();
            await sqlR.UpdateTimePrinted(input);
            sqlR.Dispose();
        }

        /// <summary>
        /// Возвращает частично потраха таска
        /// </summary>
        /// <param name="taskPrintId"></param>
        /// <returns></returns>
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetPrintTaskByIdGetPrintTaskById(int taskPrintId)
        {
            await CheckPermission();
            var sqlR = new LabelRepository();
            var result = await sqlR.GetPrintTaskById(taskPrintId);
            sqlR.Dispose();
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> GetUserLabels([FromBody] LabelRepository.UserLabelsRequest input)
        {
            await CheckPermission();
            using (var sqlR = new LabelRepository())
            {
                var result = await sqlR.GetUserLabels(input);

                return Json(result);
            }
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<JsonResult> GetDeviceLabels([FromBody] LabelRepository.DeviceLabelsRequest input)
        {
            await CheckPermission();
            using (var sqlR = new LabelRepository())
            {
                var result = await sqlR.GetDeviceLabels(input);

                return Json(result);
            }
        }

        public class TaskPrintSelectLabels
        {
            public int Id { get; set; }
            public bool IsSelectedAll { get; set; }
            public List<int> SelectedRows { get; set; }
        }
    }
}
