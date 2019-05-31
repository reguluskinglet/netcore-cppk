using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Services;
using static Rzdppk.Core.Other.DevExtremeTableData;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Controllers
{
    public class ExcelGeneratorController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ExcelGeneratorController
        (
            ILogger<ExcelGeneratorController> logger,
            IMapper mapper,
            ITvPanelSetupRepository rep
        )
        {
            Initialize();
            _logger = logger;
            _mapper = mapper;
        }




        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> Create(int id)
        {
            var service = new EventsTableService(_logger, _mapper);
            var toExcel = await service.GetTaskAndInspections(new ReportRequest
            {
                Paging = new Paging { Skip = "0", Limit = Int32.MaxValue.ToString() }
            });

            string sWebRootFolder = @"c:\Temp";
            string sFileName = @"demo.xlsx";
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            var memory = new MemoryStream();
            using (var fs = new FileStream(Path.Combine(sWebRootFolder, sFileName), FileMode.Create, FileAccess.Write))
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("Demo");
                IRow row = excelSheet.CreateRow(0);

                for (int i = 0; i < toExcel.Columns.Count; i++)
                {
                    row.CreateCell(i).SetCellValue(toExcel.Columns[i].Title);
                }

                for (var index = 0; index < toExcel.Rows.Count; index++)
                {
                    var deRow = toExcel.Rows[index];
                    row = excelSheet.CreateRow(index + 1);
                    row.CreateCell(0).SetCellValue(deRow.Col0);
                    row.CreateCell(1).SetCellValue(deRow.Col1);
                    row.CreateCell(2).SetCellValue(deRow.Col2);
                    row.CreateCell(3).SetCellValue(deRow.Col3);
                    row.CreateCell(4).SetCellValue(deRow.Col4);
                    row.CreateCell(5).SetCellValue(deRow.Col5);
                    row.CreateCell(6).SetCellValue(deRow.Col6);
                    row.CreateCell(7).SetCellValue(deRow.Col7);
                    row.CreateCell(8).SetCellValue(deRow.Col8);
                    row.CreateCell(9).SetCellValue(deRow.Col9);
                    row.CreateCell(10).SetCellValue(deRow.Col10);
                }

                workbook.Write(fs);
            }

            using (var stream = new FileStream(Path.Combine(sWebRootFolder, sFileName), FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sFileName);
        }

        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> EscapeFromDepo()
        {
            return await EscapeFromDepo(new ExcelDataRequest {Date = DateTime.Now.Date});
        }


        [HttpPost]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> EscapeFromDepo([FromBody] ExcelDataRequest input)
        {
            if (input == null)
                throw new ValidationException("Не распарсилось");

            var service = new ReportTableService(_logger, _mapper);
            var toExcel = await service.EscapeFromDepoReport(input);
            var depo = "МохнатаяКокаина";

            //TODO хз хз...
            string sWebRootFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            string sFileName = $@"Отчет_выход_из_депо_{depo}_{input.Date.Date.ToStringDateOnly()}.xlsx";
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            var memory = new MemoryStream();
            using (var fs = new FileStream(Path.Combine(sWebRootFolder, sFileName), FileMode.Create, FileAccess.Write))
            {
                //TODO Уточнить
                
                var workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet($"Отчет_выход_из_депо_{depo}_{input.Date.Date.ToStringDateOnly()}");
                //Наверно дефолтная ширина колонок
                //excelSheet.SetColumnWidth(3, 256 * 40);


                ICellStyle styleAlingCenter = workbook.CreateCellStyle();
                styleAlingCenter.Alignment = HorizontalAlignment.Center;
                ICellStyle styleRedAlingCenter = workbook.CreateCellStyle();
                styleRedAlingCenter.Alignment = HorizontalAlignment.Center;

                XSSFFont redFont = (XSSFFont)workbook.CreateFont();
                redFont.Color = IndexedColors.Red.Index;

                styleRedAlingCenter.SetFont(redFont);

                IRow row = excelSheet.CreateRow(0);
                var cell = row.CreateCell(0);
                cell.CellStyle = styleAlingCenter;
                cell.SetCellValue($"Выход из депо {depo} на утро");
                //cell.CellStyle.WrapText = true;
                excelSheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));

                //2 строчка заголовка
                row = excelSheet.CreateRow(1);
                cell = row.CreateCell(0);
                cell.SetCellValue($"{DateTime.Now.ToStringDateOnly()}г. ({GetStringDayOfWeek(DateTime.Now.DayOfWeek)})");
                cell.CellStyle = styleRedAlingCenter;
                //cell.CellStyle.WrapText = true;
                excelSheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 4));
               
                //Заголовки для данных
                row = excelSheet.CreateRow(2);
                
                for (int i = 0; i < toExcel.Columns.Count; i++)
                {
                    row.CreateCell(i).SetCellValue(toExcel.Columns[i].Title);
                }

                var firstDataRowIndex = 3;
                for (var index = 0; index < toExcel.Rows.Count; index++)
                {
                    var deRow = toExcel.Rows[index];
                    row = excelSheet.CreateRow(index + firstDataRowIndex);
                    row.CreateCell(0).SetCellValue(deRow.Col0);
                    row.CreateCell(1).SetCellValue(deRow.Col1);
                    row.CreateCell(2).SetCellValue(deRow.Col2);
                    row.CreateCell(3).SetCellValue(deRow.Col3);
                    row.CreateCell(4).SetCellValue(deRow.Col4);
                    row.CreateCell(5).SetCellValue(deRow.Col5);
                    row.CreateCell(6).SetCellValue(deRow.Col6);
                    row.CreateCell(7).SetCellValue(deRow.Col7);
                    row.CreateCell(8).SetCellValue(deRow.Col8);
                    row.CreateCell(9).SetCellValue(deRow.Col9);
                    row.CreateCell(10).SetCellValue(deRow.Col10);
                }

                //Ширина столбцов
                for (int i = 0; i < 11; i++)
                {
                    excelSheet.SetColumnWidth(i, 5000);
                }
                
                

                workbook.Write(fs);
            }
            using (var stream = new FileStream(Path.Combine(sWebRootFolder, sFileName), FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sFileName);
        }
    }
}