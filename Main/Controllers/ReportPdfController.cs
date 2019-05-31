using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using Rzdppk.Model.Enums;

namespace Rzdppk.Controllers
{
    public class ReportPdfController : BaseController
    {
        private readonly ILogger _logger;
        private static IConverter _pdfConverter;

        public ReportPdfController
        (
            IConverter converter,
            ILogger<ReportController> logger
        )
        {
            base.Initialize();
            _logger = logger;
            _pdfConverter = converter;
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        [HttpPost]
        public async Task<IActionResult> GetTu152Pdf([FromBody] Tu152Request input)
        {
            await CheckPermission();

            var rr = new ReportPdfRepository(_logger);
            var data = await rr.GetTu152PdfData(input.InspectionIds);
            var dataTasks = await rr.GetTu152PdfTasksData(input.TaskIds);

            var template = GetTemplate();

            var html = @"
<html>
<style type='text/css'>
* {
	font-size: 10pt;
}
table {
	border-collapse: collapse;
}
th, td {
	border: 1px solid black;
	padding: 5px;
}
span {
	border: 0px solid red;
	display: block;
	margin: 0 20px;
}

.background {
  width: 100px;
  height: 50px;
  padding: 0;
  margin: 0;
  border: 1px solid black;
}
.background>div {
  position: relative;
  height: 100%;
  width: 100%;
  top: 0;
  left: 0;
}

.bottom {
	text-align: left;
}

.top {
	text-align: right;
}
.break {
    page-break-after: always;
}
</style>
";
            var tblHeader = @"
<table>
<tr>
  <th rowspan='2'>
  Дата, время<br/>(час, мин),<br/>станция смены<br/>локомотивной<br/>бригады
  </th>
  <th colspan='2'>
  Фамилия машиниста
  </th>
  <th class='border' rowspan='2'>
	  <div>
		<span class='bottom'>Наличие топлива<br/>в момент при-<br/>емки в кг</span>
		<span class='top'>Показа-<br/>ния счетчика<br/>электроэнергии<br/>в момент приемки</span>
	</div>
  </th>
  <th rowspan='2'>
  Замечания и неисправности, обнаруженные в пути следования<br/>и при осмотрах (в конце записи подпись сдающего и принимающего)
  </th>
</tr>
<tr>
	<th>
	прибывшего (сдающе-<br/>го), станция, откуда<br/>прибыл и № поезда
	</th>
	<th>
	отправляю-<br/>щегося<br/>(принимающего)
	</th>
</tr>
<tr>
	<th>1</th><th>2</th><th>3</th><th>4</th><th>5</th>
</tr>
";
            int m = 0;
            foreach (var item in data)
            {
                html += $"<h2>{item.Train.Name}</h2>";
                html += tblHeader;

                var colData = new List<string>[7];

                colData[0] = new List<string>
                {
                    $"{item.Date:dd.MM.yyyy}",
                    $"{item.Date:HH:mm}"
                };

                var kwData = item.CarriageKwMeterages.Select(o => $"{o.Carriage.Serial} - {o.Value}").ToList();
                colData[3] = kwData;
                if (kwData.Count > 0)
                {
                    colData[3].Add("");
                }

                colData[3].Add($"Пробег: {item.KmPerShift}");
                colData[3].Add($"Общий: {item.KmTotal}");

                if (item.Type == CheckListType.Inspection)
                {
                    colData[1] = new List<string>
                    {
                        item.User.Name
                    };
                    colData[2] = new List<string>();

                    var list = new List<string>
                    {
                        "ТБ ("+string.Join(",", item.BrakeShoes)+")",
                    };
                    foreach (var taskValue in item.TaskValues)
                    {
                        list.Add(FormatTaskData(taskValue));
                    }

                    list.Add(template.SurrenderTemplate);
                    list.Add(item.NumberAct == null
                        ? "Акт технического состояния отсутствует"
                        : $"Акт технического состояния № {item.NumberAct} от {item.DateAct:dd.MM.yyyy}");
                    colData[4] = list;
                }
                else if (item.Type == CheckListType.Surrender)
                {
                    colData[1] = new List<string>();
                    colData[2] = new List<string>
                    {
                        item.User.Name
                    };
                    var list = new List<string>
                    {
                        "ТБ ("+string.Join(",", item.BrakeShoes)+")",
                    };
                    foreach (var taskValue in item.TaskValues)
                    {
                        list.Add(FormatTaskData(taskValue));
                    }

                    list.Add(template.SurrenderTemplate);
                    list.Add(item.NumberAct == null
                        ? "Акт технического состояния отсутствует"
                        : $"Акт технического состояния № {item.NumberAct} от {item.DateAct:dd.MM.yyyy}");

                    colData[4] = list;
                }
                else if (item.Type == CheckListType.TO1 || item.Type == CheckListType.TO2)
                {
                    colData[1] = new List<string>
                    {
                        item.User.Name
                    };
                    colData[2] = new List<string>();

                    var list = new List<string>();

                    foreach (var taskFault in item.TaskFaults)
                    {

                        list.Add($"И{taskFault.TaskId}; {taskFault.CarriageSerial}; {taskFault.EquipmentName}; {taskFault.Faults}");
                    }

                    list.Add(template.To1Template);
                    list.Add(item.NumberAct == null
                        ? "Акт технического состояния отсутствует"
                        : $"Акт технического состояния № {item.NumberAct} от {item.DateAct:dd.MM.yyyy}");

                    colData[4] = list;
                }
                else
                {
                    continue;
                }

                var cnt = Math.Max(colData[0].Count, colData[1].Count);
                cnt = Math.Max(cnt, colData[2].Count);
                cnt = Math.Max(cnt, colData[3].Count);
                cnt = Math.Max(cnt, colData[4].Count);

                for (int i = 0; i < cnt; i++)
                {
                    var cols = new string[5];
                    for (int l = 0; l < 5; l++)
                    {
                        if (colData[l] != null && colData[l].Count > i)
                        {
                            cols[l] = colData[l][i];
                        }
                        else
                        {
                            cols[l] = "";
                        }
                    }

                    html += $@"
                    <tr>
	                    <td>{cols[0]}</td><td>{cols[1]}</td><td>{cols[2]}</td><td>{cols[3]}</td><td>{cols[4]}</td>
                    </tr>";
                }

                //signature
                html += $@"<tr><td colspan='2'>{item.User.Name}</td><td colspan='3'>";
                if (item.Signature != null)
                {
                    html += $@"<img src='{item.Signature}' style='width:100px;' />";
                }

                html += "</td></tr>";

                html += "</table>";
                if (m < data.Count - 1)
                {
                    html += "<div class='break'></div>";
                }
                m++;
            }

            if (dataTasks.Any())
            {
                if (m > 0)
                {
                    html += "<div class='break'></div>";
                }

                m = 0;
                var groupData = dataTasks.GroupBy(o => o.TrainName);
                var cnt = groupData.Count();
                foreach (var group in groupData)
                {
                    html += $"<h2>{group.Key}</h2>";
                    html += tblHeader;

                    foreach (var item in group)
                    {
                        html += $@"
                    <tr>
	                    <td>{item.Date:dd.MM.yyyy HH:mm}</td><td>{item.UserName}</td><td></td><td></td><td>{$"И{item.TaskId}; {item.CarriageSerial}; {item.EquipmentName}; {item.Faults}"}</td>
                    </tr>";
                    }
                    html += "</table>";
                    if (m < cnt - 1)
                    {
                        html += "<div class='break'></div>";
                    }
                    m++;
                }
            }

            html += "</html>";

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
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            });

            return File(output, "application/pdf", "tu152" + "-" + DateTime.Now.ToString("yyyyMMdd") + ".pdf");
        }

        private static Tu152Template GetTemplate()
        {
            var sWebRootFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var text = System.IO.File.ReadAllText(sWebRootFolder + Path.DirectorySeparatorChar + "wwwroot" + Path.DirectorySeparatorChar + "templates" + Path.DirectorySeparatorChar + "Tu152.json", Encoding.UTF8);
            return JsonConvert.DeserializeObject<Tu152Template>(text);
        }

        private static string FormatTaskData(ReportPdfRepository.Tu152TaskValue taskValue)
        {
            if (taskValue.ValueType == CheckListValueType.Bool)
            {
                var formattedVal = taskValue.ValueFact == 1 ? "Да" : "Нет";
                return $"И{taskValue.TaskId}; {taskValue.CarriageSerial}; {taskValue.EquipmentName}; {taskValue.NameChecklist}: {formattedVal}";
            }
            if (taskValue.ValueType == CheckListValueType.Int)
            {
                return $"И{taskValue.TaskId}; {taskValue.CarriageSerial}; {taskValue.EquipmentName}; отсутствует {taskValue.ValueNorm - taskValue.ValueFact} из {taskValue.ValueNorm}";
            }

            return "";
        }
    }

    public class Tu152Template
    {
        public string SurrenderTemplate { get; set; }

        public string To1Template { get; set; }
    }

    public class Tu152Request
    {
        public int[] InspectionIds { get; set; }

        public int[] TaskIds { get; set; }
    }
}