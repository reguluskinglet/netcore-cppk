using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.OData.Query.SemanticAst;
using Newtonsoft.Json.Linq;
using Rzdppk.Api.Dto.EventTable;
using Rzdppk.Core.Helpers;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model.Enums;
using static Rzdppk.Api.Dto.EventTable.InspectionByIdDto;

namespace Rzdppk.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ServiceController : Controller
    {
        private readonly IDb _db;

        public ServiceController
            (
                IDb db
            )
        {
            _db = db;
        }

        public IActionResult GetTask()
        {
            var pathResource = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates", "Task.json");

            var json = System.IO.File.ReadAllText(pathResource, Encoding.GetEncoding(1251));

            return Json(JObject.Parse(json));
        }


        public IActionResult GetTimeRange()
        {
            var pathResource = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates", "GetTimeRange.json");

            var json = System.IO.File.ReadAllText(pathResource, Encoding.GetEncoding(1251));

            return Json(JObject.Parse(json));
        }

        public IActionResult GetTimeRangeData([FromBody]TimeRangeDataRequest data)
        {
            var pathResource = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates", $"GetTimeRangeData_{data.Type}.json");

            var json = System.IO.File.ReadAllText(pathResource, Encoding.GetEncoding(1251));

            return Json(JObject.Parse(json));
        }

        public IActionResult GetInspectionById([FromBody] int inspectionId)
        {
            var result = new InspectionByIdDto
            {
                Id = "M" + inspectionId,
                Type = CheckListType.TO1.GetDescription(),
                TrainName = "Поезд 1",
                Status = InspectionStatus.Create.GetDescription(),
                Author = "Сидоров Г.П.",
                BrigadeName = BrigadeType.Locomotiv.GetDescription(),
                Date = $"{DateTime.Now.AddDays(-1):g} - {DateTime.Now:g}",
                TaskCount = 152,
                Signature =
                    "data:image/png;base64,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",
                Labels = new List<InspectionLabelDto>
                {
                    new InspectionLabelDto
                    {
                        CarriageName = "Вагон 1",
                        Date = DateTime.Now.ToString("g"),
                        EquipmentName = "Оборудование 1",
                        Label = "1N1"
                    },
                    new InspectionLabelDto
                    {
                        CarriageName = "Вагон 2",
                        Date = DateTime.Now.ToString("g"),
                        EquipmentName = "Оборудование 2",
                        Label = "1N2"
                    },
                },
                InspectionDataCarriages = new List<InspectionDataCarriageDto>
                {
                    new InspectionDataCarriageDto
                    {
                        CarriageName = "вагон 1",
                        Value = 120
                    },
                    new InspectionDataCarriageDto
                    {
                        CarriageName = "вагон 2",
                        Value = 999
                    }
                },
                InspectionDataUis = new List<InspectionDataDto>
                {
                    new InspectionDataDto
                    {
                        Type = InspectionDataType.BrakeShoes.GetDescription(),
                        Value = "123/569/236/454"
                    },
                    new InspectionDataDto
                    {
                        Type = InspectionDataType.KmPerShift.GetDescription(),
                        Value = "1234"
                    },
                    new InspectionDataDto
                    {
                        Type = InspectionDataType.KmTotal.GetDescription(),
                        Value = "999"
                    },
                }
            };

            return Ok(result);
        }
    }

    public class TimeRangeDataRequest
    {
        public int Id { get; set; }
        public int Type { get; set; }
    }

    //public class InspectionUI
    //{
    //    /// <summary>
    //    /// ИД мероприятия с префиксом М
    //    /// </summary>
    //    public string Id { get; set; }

    //    /// <summary>
    //    /// Тип ТО-1
    //    /// </summary>
    //    public string Type { get; set; }

    //    public string TrainName { get; set; }
    //    public string Status { get; set; }

    //    /// <summary>
    //    /// Кто создал
    //    /// </summary>
    //    public string Author { get; set; }

    //    /// <summary>
    //    /// Какая Бригада выполнила работы
    //    /// </summary>
    //    public string BrigadeName { get; set; }

    //    /// <summary>
    //    /// Дата начала и окночания мероприятия, если DateEnd=NUll - пишем только дату начала
    //    /// </summary>
    //    public string Date { get; set; }

    //    public int TaskCount { get; set; }

    //    public List<InspectionLabelUI> Labels { get; set; }=new List<InspectionLabelUI>();
    //    public List<InspectionDataCarriageUI> InspectionDataCarriages { get; set; }=new List<InspectionDataCarriageUI>();
    //    public List<InspectionDataUI> InspectionDataUis { get; set; } = new List<InspectionDataUI>();

    //    /// <summary>
    //    /// Подпись при приемке\сдаче поезда  Table - Signatures поле CaptionImage
    //    /// </summary>
    //    public string Signature { get; set; }
    //}

    ////Данные по считанным меткам
    //public class InspectionLabelUI
    //{
    //    public string CarriageName { get; set; }
    //    public string EquipmentName { get; set; }
    //    public string Date { get; set; }
    //    public string Label { get; set; }
    //}

    ////Данные по Квт ч, приходят по вагонам
    //public class InspectionDataCarriageUI
    //{
    //    public string CarriageName { get; set; }
    //    public int Value { get; set; }
    //}

    ////Остальные доп. данные по инспекции (все кроме InspectionDataType=KwHours)
    //public class InspectionDataUI
    //{
    //    public string Type { get; set; }
    //    public string Value { get; set; }
    //}
}
