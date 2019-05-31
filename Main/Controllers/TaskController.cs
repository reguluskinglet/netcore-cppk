using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DinkToPdf;
using DinkToPdf.Contracts;
using Invent.Core.GridModels.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.GridModels.Journals;
using Rzdppk.Core.Options;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using Rzdppk.Core.Services;
using Rzdppk.Model;
using Rzdppk.Model.Enums;
using static Rzdppk.Core.Other.Other;
using TaskStatus = Rzdppk.Model.Enums.TaskStatus;

namespace Rzdppk.Controllers
{
    public class TaskController : BaseController
    {
        private readonly IDb _db;
        private static IConverter _pdfConverter;
        private readonly ILogger _logger;

        public TaskController
        (
            IDb db, IConverter converter, ILogger<TaskController> logger
        )
        {
            base.Initialize();
            _db = db;
            _logger = logger;
            _pdfConverter = converter;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAll(int skip, int limit, string filter)
        {
            await CheckPermission();
            var tr = new TaskRepository(_logger);
            var permissions = (await GetCurrentUser()).Role.Permissions;
            var filters = new List<TaskCommon.FilterBody>();
            if (filter != null)
                filters = JsonConvert.DeserializeObject<TaskCommon.FilterBody[]>(filter).ToList();

            string sqlCustom = null;

            //Показывать задачи бригады локомативщиков 6
            var needPermissionBits = 32;
            var res = permissions & needPermissionBits;
            if (res == needPermissionBits)
            {
                sqlCustom = " BrigadeType = 0 ";
            }
            //Показывать задачи бригады депо 7
            needPermissionBits = 64;
            res = permissions & needPermissionBits;
            if (res == needPermissionBits)
            {
                sqlCustom = sqlCustom == null ? " BrigadeType = 1 " : $"{sqlCustom} OR BrigadeType = 1 ";
            }
            //Показывать задачи бригады приемщиков 8 
            needPermissionBits = 128;
            res = permissions & needPermissionBits;
            if (res == needPermissionBits)
            {
                sqlCustom = sqlCustom == null ? " BrigadeType = 2 " : $"{sqlCustom} OR BrigadeType = 2 ";
            }
            //Показывать задачи бригады приемщиков 8 
            needPermissionBits = 131072;
            res = permissions & needPermissionBits;
            if (res == needPermissionBits)
            {
                sqlCustom = "BrigadeType = 2 OR BrigadeType = 0 OR BrigadeType = 1";
            }

            if (sqlCustom != null)
                filters.Add(new TaskCommon.FilterBody {Filter = "Custom", Value = sqlCustom});
            //TODO костыль очередной. Недает никому смотреть задачи, если нет соответвующих прав
            else
                filters.Add(new TaskCommon.FilterBody { Filter = "Custom", Value = " BrigadeType = 999999 " });

            if (filters.Count > 0)
                filter = JsonConvert.SerializeObject(filters.ToArray());
            var result = await tr.GetAll(skip, limit, filter);

            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> GetJournalPdf([FromBody] JournalRequest req)
        {
            await CheckPermission();

            var html = @"<html><head>
<style type='text/css'>
.task {
    border-collapse: collapse;
    border: 1px solid black;
}
.task td {
    padding: 5px;
    border: 1px solid black;
}
.task th {
    padding: 5px;
    background-color: #909090;
    border: 1px solid black;
}
//.break {
//    page-break-after: always;
//}

hr.ex2 {
  border:none;
  border-top:3px dotted #aaa;
  color:#fff;
  background-color:#fff;
  height:1px;
  width:100%;
}

</style></head><body>";

            var tr = new TaskRepository(_logger);
            var serviceTask = new TaskService(_logger);
            if (req.Tasks.Length > 0)
            {
                var tasksDictionary = await tr.GetTrainTasksForPdf(req.Tasks);

                foreach (var tasks in tasksDictionary)
                {

                    html += "<table><tr><td>Поезд:</td><td>" + tasks.Key + "</td></tr></table>";
                    html += "<br /><h2>Задачи</h2>";
                    if (tasks.Value.Count > 0)
                    {
                        foreach (var task in tasks.Value)
                        {
                            html +=
                                "<table class='task'><tr><th>Вагон</th><th>Местоположение</th><th>Оборудование</th><th>Типовая неисправность</th><th>Описание</th><th>Метка</th><th>№ задачи</th><th>Время</th></tr>";
                            var history = await serviceTask.AddHistoryData(task.Id);
                            html += "<tr><td>" + task.Carriage + "</td><td>" + task.Location + "</td><td>" +
                                    task.Equipment + "</td><td>" + task.Fault + "</td><td>" + task.Description +
                                    "</td><td>" + task.Label + "</td><td>" + task.Id + "</td><td>" + task.Created +
                                    "</td></tr>";
                            html += "<tr><td colspan='8'>";
                            if (history.Count > 0)
                            {
                                html += "<b>Комментарии:</b><br />";
                                html += "";
                                foreach (var item in history)
                                {
                                    html +=
                                        $"{item.Date} {item.User} ({TaskRepository.BrigadeTypeToString((BrigadeType?) item.UserBrigadeType)}): ";
                                    switch (item.Type)
                                    {
                                        case "Comment":
                                            html += $"{item.Text}<br />";
                                            break;
                                        case "Status":
                                            html +=
                                                $"<b>Смена статуса:</b> {tr.StatusToString((TaskStatus?) item.OldStatus)} > {tr.StatusToString((TaskStatus?) item.NewStatus)}<br />";
                                            break;
                                        case "Executor":
                                            html +=
                                                $"<b>Смена исполнителя:</b> {TaskRepository.BrigadeTypeToString((BrigadeType?) item.OldExecutorBrigadeType)} > {TaskRepository.BrigadeTypeToString((BrigadeType?) item.NewExecutorBrigadeType)}<br />";
                                            break;
                                    }

                                    html += "";
                                }
                                html += "<br />";
                                html += "</td></tr>";
                                html += "</table>";
                                html += "<br />";
                                html += "<hr class=\"ex2\"></hr>";
                                html += "<br />";
                            }
                            

                        }
                        

                    }
                    html += "</table>";
                }
            }
            
            var i = 0;
            var count = req.Inspections.Count();
            foreach (var id in req.Inspections)
            {
                var insp = await GetInspectionHtmlForPdf(id);
                html += insp;

                if (++i != count) //not for last item
                    //html += "<div class='break'></div>";
                    html += "<hr class=\"ex2\"></hr>";

            }

            var output = _pdfConverter.Convert(new HtmlToPdfDocument
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            });
            //
            return File(output, "application/pdf", "journal_" + new Random().Next(0, 10000) + ".pdf");
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> GetJournalTablePdf([FromBody] JournalTableRequest req)
        {
            await CheckPermission();

            var html = @"<html><head>
                <style type='text/css'>
                .task {
                    width: 100%;
                    border-collapse: collapse;
                    border: 1px solid black;
                }
                .task td {
                    padding: 5px;
                    border: 1px solid black;
                }
                .task th {
                    padding: 5px;
                    background-color: #909090;
                    border: 1px solid black;
                }
                </style></head><body>";

            var tr = new TaskRepository(_logger);
            var ir = new InspectionRepository(_logger);
            var serviceTask = new TaskService(_logger);

            html += "<br /><h2>Задачи</h2>";

            var filter = new JournalGridFilter();

            filter.Init(req.Options.Filter);

            var grid = new ActionGrid<JournalGrid, JournalGridModel, JournalFilter>(_db.Connection, req.Options, filter);
            var gridResult = await grid.GetResultRows();            

            if (gridResult.Count() == 0)
            {
                html += "<br /><h4>Список задач отсутствует</h4>";
            }
            else
            {
                html += "<table class='task'><tr><th>№ задачи</th><th>Вагон</th><th>Местоположение</th><th>Оборудование</th><th>Типовая неисправность</th><th>Описание</th><th>Метка</th><th>Время</th><th>Автор</th><th>Комментарий</th></tr>";

                foreach (var task in gridResult)
                {
                    var history = await serviceTask.AddHistoryData(task.Id);
                    var trainTask = await tr.GetTrainTaskForPdf(task.Id);

                    if(trainTask != null)
                    {
                        string comments = null;
                        var taskComments = history.OrderBy(h => h.Date).Where(h => h.Type == "Comment" && h.Text != null && h.Text != trainTask.Description);

                        foreach (var comment in taskComments)
                        {
                            comments += $"<i>{comment.Date.ToString("MM.dd.yy")}</i>: {comment.Text}<br />";
                        }

                        html += "<tr><td>" + trainTask.Id + "</td><td>" + trainTask.CarriageSerial + "</td><td>" + trainTask.Location +
                                "</td><td>" + trainTask.Equipment + "</td><td>" + trainTask.Fault + "</td><td>" + trainTask.Description +
                                "</td><td>" + trainTask.Label + "</td><td>" + trainTask.Created +
                                "</td><td>" + trainTask.UserName + "</td></td><td>" + comments + "</td></tr>";
                    }
                }

                html += "</table>";
            }
 
            var output = _pdfConverter.Convert(new HtmlToPdfDocument
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Landscape,
                    PaperSize = PaperKind.A4,
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        PagesCount = true,
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" },
                        FooterSettings = { FontName = "Arial", FontSize = 9, Right = "[page]", Line = false },
                    }
                }
            });

            return File(output, "application/pdf", "journal_" + new Random().Next(0, 10000) + ".pdf");
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> GetTrainTasksPdf([FromBody] List<int> ids)
        {
            await CheckPermission();

            var tr = new TaskRepository(_logger);
            var tasksDictionary = await tr.GetTrainTasksForPdf(ids.ToArray());
            if (tasksDictionary == null)
                throw new Exception("no tasks has found");

            var html = @"<html><head>
<style type='text/css'>
.task {
    border-collapse: collapse;
    border: 1px solid black;
}
.task td {
    padding: 5px;
    border: 1px solid black;
}
.task th {
    padding: 5px;
    background-color: #909090;
    border: 1px solid black;
}
</style></head><body>";
            foreach (var tasks in tasksDictionary)
            {


                html += "<table><tr><td>Поезд:</td><td>" + tasks.Key + "</td></tr></table>";
                if (tasks.Value.Count > 0)
                {
                    html +=
                        "<br /><h2>Задачи</h2><table class='task'><tr><th>Вагон</th><th>Местоположение</th><th>Оборудование</th><th>Типовая неисправность</th><th>Описание</th><th>Метка</th><th>№ задачи</th><th>Время</th></tr>";
                    foreach (var task in tasks.Value)
                    {
                        html += "<tr><td>" + task.Carriage + "</td><td>" + task.Location + "</td><td>" +
                                task.Equipment + "</td><td>" + task.Fault + "</td><td>" + task.Description +
                                "</td><td>" + task.Label + "</td><td>" + task.Id + "</td><td>" + task.Created +
                                "</td></tr>";
                    }
                    html += "</table>";
                }
            }
            html += "</body></html>";

            //
            var output = _pdfConverter.Convert(new HtmlToPdfDocument
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            });
            //
            return File(output, "application/pdf", Transliterate(String.Join("-",tasksDictionary.Keys)) + "_task_" + new Random().Next(0, 10000) + ".pdf");
        }

        private static string Transliterate(string str)
        {
            string[] lat_up = { "A", "B", "V", "G", "D", "E", "Yo", "Zh", "Z", "I", "Y", "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "F", "Kh", "Ts", "Ch", "Sh", "Shch", "\"", "Y", "'", "E", "Yu", "Ya" };
            string[] lat_low = { "a", "b", "v", "g", "d", "e", "yo", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "kh", "ts", "ch", "sh", "shch", "\"", "y", "'", "e", "yu", "ya" };
            string[] rus_up = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я" };
            string[] rus_low = { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я" };
            for (var i = 0; i <= 32; i++)
            {
                str = str.Replace(rus_up[i], lat_up[i]);
                str = str.Replace(rus_low[i], lat_low[i]);
            }
            return str;
        }

        //[Authorize]
        //[Route("api/[controller]/[action]")]
        private async Task<string> GetInspectionHtmlForPdf(int id)
        {
            //await CheckPermission();
            var ir = new InspectionRepository(_logger);
            var tr = new TaskRepository(_logger);
            var ur = new UserRepository(_logger);
            var trr = new TrainRepository(_logger);

            var inspection = await ir.ById(id);

            if (inspection == null)
                return "";
                //throw new Exception("инспекция не найдена");

            inspection.User = await ur.ById(inspection.UserId);
            inspection.Train = await trr.ByIdWithStations(inspection.TrainId);

            var res = await tr.GetInspectionTasksForPdf(id);
            var tasks = res.FirstOrDefault();

            string checkListTypeRussian = null;
            if (inspection.CheckListType == CheckListType.TO1)
                checkListTypeRussian = InspectionRussianName.To1;
            if (inspection.CheckListType == CheckListType.TO2)
                checkListTypeRussian = InspectionRussianName.To2;
            if (inspection.CheckListType == CheckListType.Inspection)
                checkListTypeRussian = InspectionRussianName.PriemkaPoezda;
            if (inspection.CheckListType == CheckListType.Surrender)
                checkListTypeRussian = InspectionRussianName.SdachaPoezda;

            var html = "";/*"@"<html><head>
<style type='text/css'>
.task {
    border-collapse: collapse;
    border: 1px solid black;
}
.task td {
    padding: 5px;
    border: 1px solid black;
}
.task th {
    padding: 5px;
    background-color: #909090;
    border: 1px solid black;
}
</style></head><body>";*/
            
            html += "<table><tr><td>Мероприятие:</td><td>" + checkListTypeRussian+ "</td></tr>";
            html += "<tr><td>Поезд:</td><td>" + inspection.Train.Name + "</td></tr>";
            html += "<tr><td>Начато:</td><td>" + inspection.DateStart + "</td></tr>";
            html += "<tr><td>Закончено:</td><td>" + inspection.DateEnd + "</td></tr>";
            html += "<tr><td>Выполнил:</td><td>" + inspection.User.Name + "</td></tr></table>";
            if (tasks != null && tasks.Tasks.Length > 0)
            {
                html += 
                    "<br /><h2>Задачи</h2><table class='task'><tr><th>Вагон</th><th>Местоположение</th><th>Оборудование</th><th>Типовая неисправность</th><th>Описание</th><th>Метка</th><th>№ задачи</th><th>Время</th></tr>";
                foreach (var task in tasks.Tasks)
                {
                    html += "<tr><td>"+task.CarriageSerialNum+"</td><td>"+task.Location+"</td><td>"+task.Equipment+"</td><td>"+task.Fault+"</td><td>"+task.Description+"</td><td>"+task.Label+"</td><td>"+task.Id+"</td><td>"+task.Created+"</td></tr>";
                }
                html += "</table><br /><br />";

                if (tasks.Labels.Any())
                {
                    html += "<h2>Метки</h2><table class='task'><tr><th>Вагон</th><th>Оборудование</th><th>Время</th><th>ИД метки</th></tr>";
                    foreach (var label in tasks.Labels)
                    {
                        html += "<tr><td>" + label.CarriageName + "</td><td>" + label.EquipmentName + "</td><td>" + label.TimeStamp + "</td><td>" + label.LabelSerial + "</td></tr>";
                    }
                    html += "</table><br /><br />";
                }

                if (tasks.Temperatures.Any())
                {
                    html += "<h2>Температура</h2><table class='task'><tr><th>Температура</th><th>Время</th><th>Температура</th><th>Время</th><th>Температура</th><th>Время</th><th>Температура</th><th>Время</th></tr>";
                    var cnt = 0;
                    foreach (var temp in tasks.Temperatures)
                    {
                        if (cnt % 4 == 0)
                        {
                            html += "<tr>";
                        }
                        html += "<td>" + temp.Value + "</td><td>" + temp.TimeStamp + "</td>";
                        if ((cnt + 1) % 4 == 0)
                        {
                            html += "</tr>";
                        }
                        cnt++;
                    }
                    var remain = 4 - (cnt % 4);
                    if (remain > 0)
                    {
                        for (var i = 0; i < remain; i++)
                        {
                            html += "<td></td><td></td>";
                        }
                        html += "</tr>";
                    }
                    html += "</table><br /><br />";
                }
            }
            else
            {
                html += "<h2>Не найдено задач</h2>";
            }

            //html += "</body></html>";
            //
            //var output = _pdfConverter.Convert(new HtmlToPdfDocument
            //{
            //    GlobalSettings = {
            //        ColorMode = ColorMode.Color,
            //        Orientation = Orientation.Portrait,
            //        PaperSize = PaperKind.A4,
            //    },
            //    Objects =
            //    {
            //        new ObjectSettings
            //        {
            //            HtmlContent = html,
            //            WebSettings = { DefaultEncoding = "utf-8" }
            //        }
            //    }
            //});
            ////
            //return File(output, "application/pdf", inspection.Id+"_"+inspection.DateStart+"_"+inspection.CheckListType+".pdf");
            return html;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Update([FromBody]TaskService.UpdateTaskData data)
        {
            await CheckPermission();
            var serviceTask = new TaskService(_logger);
            var user = await GetCurrentUser();
            await serviceTask.UpdateTask(data, user, true);
            return Json("Updated");
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<string> Add([FromBody]TaskRepository.TrainTaskaddUi data)
        {
            await CheckPermission();
            var user = await GetCurrentUser();
            //var permissions = ().Role.Permissions;
            var taskService = new TaskService(_logger);
            var result = await taskService.TrainTaskAdd(data, user);
            return result.IsUpdated ? $"Данные по инциденту на оборудование {result.EquipmentName} успешно обновлены" : "";
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetTaskById(int id)
        {
            await CheckPermission();
            //временно что бы отображались все статусы: 
            var permissions = Int32.MaxValue;//(await GetCurrentUser()).Role.Permissions;
            var taskService = new TaskService(_logger);
            var result = await taskService.GetTaskById(id, permissions);
            return Json(result);
        }

        //шняга для новых задач
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAvaibleExecutors()
        {
            await CheckPermission();
            var permissions = (await GetCurrentUser()).Role.Permissions;
            var sqlR = new ExecutorRepository(_logger);
            var result = sqlR.GetAvaibleExecutors(permissions);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAvailableTaskTypes()
        {
            await CheckPermission();

            var sqlR = new TaskRepository(_logger);
            var result = sqlR.GetAvailableTaskTypes();

            return Json(result);
        }

        //шняга для новых задач
        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> GetAvaibleStatuses(int taskStatus, BrigadeType executorBrigadeType)
        {
            await CheckPermission();
            var permissions = (await GetCurrentUser()).Role.Permissions;
            var sqlR = new TaskStatusRepository(_logger);
            var result = sqlR.GetAvailableStatusesNewTask((TaskStatus)taskStatus, executorBrigadeType, permissions);
            return Json(result);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> GetActPdf([FromBody]int trainId)
        {
            await CheckPermission();

            if (trainId == 0)
                throw new ValidationException($"Необходимо выбрать Номер состава.");

            var sqlRAct = new ActCategoriesRepository();
            var html = @"<html><head>
<style type='text/css'>
.task {
    border-collapse: collapse;
    border: 1px solid black;
}
.task td {
    padding: 5px;
    border: 1px solid black;
}
.task th {
    padding: 5px;
    border: 1px solid black;
    font-size: 80%;
}
.rotate {
    text-align: center;
    white-space: nowrap;
    vertical-align: middle;
    width: 1.5em;
}
.rotate div {
    -webkit-transform: rotate(-90.0deg);
    margin-left: -10em;
    margin-right: -10em;
}
 .break {
    page-break-after: always;
 }
</style></head><body>";

            var sqlRTrain = new TrainRepository(_logger);
            var train = await sqlRTrain.ByIdWithStations(trainId);
            var sqlRCarriage = new CarriageRepository(_logger);
            var carriages = await sqlRCarriage.GetByTrain(train);
            string carriagesString = null;

            carriages = carriages.OrderBy(e => e.Number).ToArray();
            foreach (var carriage in carriages)
            {
                if (carriage.Number == 9)
                    continue;
                carriagesString += $"{CreateCarriageNameWithOutTrain(carriage.Number)} ";
            }
            carriagesString += $"{CreateCarriageNameWithOutTrain(9)} ";

            //html += "<p align=\"center\"><font size=\"5\"><b>Акт сдачи - приемки транспорта</b></p>";
            html += "<p align=\"center\"><font size=\"4\"><b>Акт сдачи - приемки Транспорта</b></font></p>";
            html += "<p align=\"center\"><font size=\"4\"><b> в депо </b> Нахабино</font></p>";
            html += "<p><b> N </b>1</p>" +
                    $"<p> Дата {DateTime.Now.ToShortDateString()} Время {DateTime.Now.ToShortTimeString()} Номер эл/ п {train.Name}; Составность {carriages.Length} Причина проведения осмотра ТО-_____ </p>" +
                    $"<p> Нумерация вагонов в подвижном составе {carriagesString} </ p >";

            //шапка 
            html += "<table class='task break'><tr><th colspan=\"3\" rowspan=\"2\"></th><th colspan=\"3\">Раздел I</th><th colspan=\"3\">Раздел II</th><th colspan=\"3\">Раздел III</th></tr>";
            html += "<tr><th>Испр</th><th>Неиспр</th><th>№ вагона</th><th>Испр</th><th>Неиспр</th><th>№ вагона</th><th>Испр</th><th>Неиспр</th><th>№ вагона</th></tr>";

            //1. Санитарное состояние
            //надо получить число записей и вставить в <th rowspan = 4>

            var sqlrTask = new TaskRepository(_logger);
            CultureInfo us = new CultureInfo("en-US");
            var currentDateForFuckingSql = DateTime.Now.ToString(@"yyyy-MM-dd", us);

            //var filter =
                //$"[{{\"filter\":\"DateFrom\",\"value\":\"{currentDateForFuckingSql}\"}},{{\"filter\":\"DateTo\",\"value\":\"{currentDateForFuckingSql}\"}}]";
            var tasks = await sqlrTask.GetAllForPdf(0, Int32.MaxValue, trainId);
            var contentWithValues = new List<ActCategoriesRepository.EquipmentActsPdf>();
            var summaryContentWithValues = new List<ActCategoriesRepository.EquipmentActsPdf>();
            //var sqlRAct = new ActCategoriesRepository();

            var badR1 = 0;
            var goodR2 = 0;
            var badR3 = 0;

            var paragraphs = new List<string>
            {
                "Санитарное состояние",
                "Электрооборудование",
                "Кузовное оборудование",
                "Прочее"
            };

            var count = 1;
            
            List<ActCategoriesRepository.EquipmentActsPdf> content;
            foreach (var paragraph in paragraphs)
            {
                content = await sqlRAct.GetAllForPdf(paragraph, count);
                var rowspanPlus = 2;
                if (count == 3)
                    rowspanPlus = 3;

                html += $@"<tr><th class='rotate' rowspan='{content.Count + rowspanPlus}'>" +
                        "<div>" +
                        $"{count}. {paragraph}" +
                        $"" +
                        "</div>" +
                        "</th></tr>";

                var countInternal = 1;
                foreach (var item in content)
                {
                    item.CarriageNumR1 = new List<int>();
                    item.CarriageNumR2 = new List<int>();
                    item.CarriageNumR3 = new List<int>();

                    if (countInternal == 12 && count == 3)
                    {
                        html += "</table><table class='task'>";
                        html += "<tr><th colspan=\"3\" rowspan=\"2\"></th><th colspan=\"3\">Раздел I</th><th colspan=\"3\">Раздел II</th><th colspan=\"3\">Раздел III</th></tr>";
                        html += "<tr><th>Испр</th><th>Неиспр</th><th>№ вагона</th><th>Испр</th><th>Неиспр</th><th>№ вагона</th><th>Испр</th><th>Неиспр</th><th>№ вагона</th></tr>";
                        html += $@"<tr><th class='rotate' rowspan='{content.Count - countInternal + rowspanPlus}'>" +
                                "<div>" +
                                $"{count}. {paragraph}" +
                                $"" +
                                "</div>" +
                                "</th></tr>";
                    }

                    await TaskFucker(sqlRAct, tasks, item);

                    contentWithValues.Add(item);
                    html += $"<tr><th>{item.Number}</th>" +
                            $"<th>{item.ActName}</th>" +
                            $"<th>{item.GoodR1}</th>" +
                            $"<th>{item.BadR1}</th>" +
                            $"<th>{sqlRAct.CreateCarriageStrintToPdf(item.CarriageNumR1)}</th>" +

                            $"<th>{item.GoodR2}</th>" +
                            $"<th>{item.BadR2}</th>" +
                            $"<th>{sqlRAct.CreateCarriageStrintToPdf(item.CarriageNumR2)}</th>" +

                            $"<th>{item.GoodR3}</th>" +
                            $"<th>{item.BadR3}</th>" +
                            $"<th>{sqlRAct.CreateCarriageStrintToPdf(item.CarriageNumR3)}</th>" +
                            $"</tr>";

                    countInternal++;
                }
                //както надо посчитать колво замечаний в каждом столбце блядь
                badR1 = 0;
                goodR2 = 0;
                badR3 = 0;
                foreach (var item in contentWithValues)
                {
                    if (item.BadR1 != null)
                        badR1 = badR1 + item.BadR1.Value;
                    if (item.GoodR2 != null)
                        goodR2 = goodR2 + item.GoodR2.Value;
                    if (item.BadR3 != null)
                        badR3 = badR3 + item.BadR3.Value;
                }

                string s1 = null;
                string s2 = null;
                string s3 = null;
                if (badR1 != 0) s1 = badR1.ToString();
                if (goodR2 != 0) s2 = goodR2.ToString();
                if (badR3 != 0) s3 = badR3.ToString();


                html +=
                    $"<tr><th colspan=\"2\"><b>Всего замечаний</b></th><th></th><th>{s1}</th><th></th><th>{s2}</th><th></th><th></th><th></th><th>{s3}</th><th></th></tr>";
                    summaryContentWithValues.AddRange(contentWithValues);
                    contentWithValues.Clear();
                count++;

                if (count == 4)
                {
                    html +=
                        "<tr><th></th><th>4.</th><th><b>Составность не соответствует заявке</b></th><th></th><th></th><th></th><th></th><th></th><th></th><th></th><th></th><th></th></tr>";
                    count++;
                }
            }

            //ИТОГО

            badR1 = 0;
            goodR2 = 0;
            badR3 = 0;
            foreach (var item in summaryContentWithValues)
            {
                if (item.BadR1 != null)
                    badR1 = badR1 + item.BadR1.Value;
                if (item.GoodR2 != null)
                    goodR2 = goodR2 + item.GoodR2.Value;
                if (item.BadR3 != null)
                    badR3 = badR3 + item.BadR3.Value;

            }

            string ss1 = null;
            string ss2 = null;
            string ss3 = null;
            if (badR1 != 0) ss1 = badR1.ToString();
            if (goodR2 != 0) ss2 = goodR2.ToString();
            if (badR3 != 0) ss3 = badR3.ToString();

            html += $"<tr><th></th><th colspan=\"2\"><b>ИТОГО по санитарно - техническому состоянию</b></th><th></th><th>{ss1}</th><th></th><th>{ss2}</th><th></th><th>" +
                    $"</th><th></th><th>{ss3}</th><th></th></tr>";

            html += "</table><br /><br />";

            //надписи под таблицей

            var checkBoxHtml = "<input maxlength = \"1\" size = \"1\" type = \"text\">";
            var probel = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            var probel2 = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            var probel3 = "&nbsp;";


            html += $"<p><font size=\"2\">{probel}Эл.поезд эксплуатационным нормам безопасности движения {checkBoxHtml}соответствует {checkBoxHtml}не соответсвует</font></p>";

            html += $"<p><font size=\"2\">На основании дейтв. нормативных документов эксплуатации поезда {checkBoxHtml}разрешена{probel3}{checkBoxHtml}запрещена</font></p>";

            html += $"<p><font size=\"2\">Поезд к первоочередн. устранению {checkBoxHtml}      Повторная проверка проведена: _______________      Время Выхода из депо________</font></p>";
            html += $"<p><font size=\"2\">по результатам повторной проверки эксплуатация поезда {checkBoxHtml}разрешена   {checkBoxHtml}запрещена       <b>Выдан на линию воопреки запрета в</b>________</font></p>";

            html += $"<p align=\"right\"><font size=\"2\"><b>Приёмку подвижного состава до постановки на ТО(ТР) произвели</b></font>{probel2}{probel2}</p>";

            html += $"<p align=\"center\"><font size=\"2\"><b>Представитель ПАО \"ЦППК\" - приёмщик ООО \"Профлайн\"_________________{probel2}{probel2}/_______________/</b></font></p>";
            html += $"<p align=\"center\"><font size=\"2\"><b>Представитель депо <u>{probel2}{probel2}Нахабино{probel2}{probel2}бригадир{probel2}{probel2}{probel2}</u>{probel2}{probel2}/_______________/</b></font></p>";

            html += $"<p align=\"right\"><font size=\"2\"><b>Приёмку подвижного состава после проведения ТО(ТР) произвели</b></font>{probel2}{probel2}</p>";
            html += $"<p align=\"center\"><font size=\"2\"><b>Представитель ПАО \"ЦППК\" - приёмщик ООО \"Профлайн\"_________________{probel2}{probel2}/_______________/</b></font></p>";
            html += $"<p align=\"center\"><font size=\"2\"><b>Представитель депо <u>{probel2}{probel2}Нахабино{probel2}{probel2}бригадир{probel2}{probel2}{probel2}</u>{probel2}{probel2}/_______________/</b></font></p>";

            html += "<br /><br /><body></html>";
            //
            var output = _pdfConverter.Convert(new HtmlToPdfDocument
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            });
            //

            
            return File(output, "application/pdf", "1111" + "_" + "2222" + "_" + "3333" + ".pdf");
        }

        public async Task TaskFucker(ActCategoriesRepository sqlRAct, List<TaskRepository.TrainTaskWithStatus> tasks, ActCategoriesRepository.EquipmentActsPdf item)
        {
            var sqlRTs = new TaskStatusRepository(_logger);

            foreach (var task in tasks)
            {
                var iWantSleep = await sqlRAct.GetByEquipmentId(task.EquipmentModel.Equipment.Id);

                if (iWantSleep.Count > 0)
                {
                    var value = iWantSleep.FirstOrDefault();
                    if (value.Name.Equals(item.ActName))
                    {
                        if (item.BadR1 == null)
                            item.BadR1 = 0;
                        item.BadR1++;

                        item.CarriageNumR1.Add(task.Carriage.Number);//$" {other.CreateCarriageNameWithOutTrain(task.Carriage.Number)};";

                        //выполненые
                        var taskStatuses = await sqlRTs.ByTaskId(task.Id);
                        var isDone = false;
                        foreach (var status in taskStatuses)
                        {
                            if (status.Status == TaskStatus.Closed)
                            {
                                if (item.GoodR2 == null)
                                    item.GoodR2 = 0;
                                item.GoodR2++;
                                item.CarriageNumR2.Add(task.Carriage.Number);
                                isDone = true;
                                break;
                            }
                        }

                        if (!isDone)
                        {
                            if (item.BadR3 == null)
                                item.BadR3 = 0;
                            item.BadR3++;
                            item.CarriageNumR3.Add(task.Carriage.Number);
                        }
                    }
                }
            }

        }

        public class JournalRequest
        {
            public int[] Tasks { get; set; }
            public int[] Inspections { get; set; }
            public Array Filter { get; set; }
        }

        public class JournalTableRequest
        {
            public JournalGridOptions Options { get; set; }
        }

        public class OkResponse
        {
            public string Message { get; set; }
        }
    }
}