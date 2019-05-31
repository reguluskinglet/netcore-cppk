using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Label;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;
using Newtonsoft.Json;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes.Base;

namespace Rzdppk.Core.Repositoryes
{
    public class LabelRepository : IDisposable
    {
        private readonly IDb _db;
        
        public LabelRepository()
        {
            _db = new Db();
        }

        public async Task<Label[]> GetByCarriageId (int id, int equipmentModelId)
        {
            var sql = Sql.SqlQueryCach["Label.GetByCarriageId"];
            var result = await _db.Connection.QueryAsync<Label>(sql, new {id = id, equipmentModelId = equipmentModelId});
            return result.ToArray();
        }


        /// <summary>
        /// Получить все задачи
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<TaskPrintsUiPaging> GetTaskPrints(int skip, int limit, string filter)
        {

            CreateFilter(filter, out string sqlfilter);
            var sql = $"{LabelCommon.SqlSelectGetAll} {LabelCommon.SqlCommon} {sqlfilter} {LabelCommon.SqlPagingEndSortDate}";
            
            var result = await _db.Connection.QueryAsync<TaskPrintsUi>(sql, new { skip = skip, limit = limit });

            var sqlc = $"{LabelCommon.SqlCount} {LabelCommon.SqlCommon} {sqlfilter}";
            var count = (await _db.Connection.QueryAsync<int>(sqlc)).FirstOrDefault();

            var taskPrintsUis = result as TaskPrintsUi[] ?? result.ToArray();
            foreach (var item in taskPrintsUis)
            {
                item.TaskPrintItemsCount = (await GetTaskPrintItemsByTaskPrintsId(item.Id)).Count;
            }

            var output = new TaskPrintsUiPaging
            {
                Data = taskPrintsUis,
                Total = count
            };

            return output;
        }

        private static void CreateFilter(string filter, out string sqlfilter)
        {
            sqlfilter = "";
            if (filter != null)
            {
                var filters = JsonConvert.DeserializeObject<LabelCommon.FilterBody[]>(filter);
                sqlfilter = " ";
                for (var index = 0; index < filters.Length; index++)
                {
                    var item = filters[index];
                    switch (item.Filter)
                    {
                        case "DateFrom":
                            sqlfilter += $" AND taskPrints.CreateDate >= '{item.Value}'";
                            break;

                        case "DateTo":
                            DateTime.TryParse(item.Value, out var dateParsed);
                            dateParsed = dateParsed.AddDays(1);
                            sqlfilter += $" AND taskPrints.CreateDate < '{dateParsed.ToString()}'";
                            break;
                    }

                    //if (index < (filters.Length - 1))
                    //    sqlfilter += " AND ";
                }
            }
        }


        /// <summary>
        /// Получить потрашки задачи
        /// </summary>
        /// <param name="taskPrintsId"></param>
        /// <returns></returns>
        public async Task<List<TaskPrintItem>> GetTaskPrintItemsByTaskPrintsId(int taskPrintsId)
        {
            var sql = Sql.SqlQueryCach["Label.GetTaskPrintItemsById"];
            var result = await _db.Connection.QueryAsync<TaskPrintItem>(sql, new { id = taskPrintsId});
            return result.ToList();
        }


        /// <summary>
        /// Получить потроошки для UI походу
        /// </summary>
        /// <param name="taskPrintId"></param>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<TaskPrintItemsByTaskPrintsIdUiPaging> GetTaskPrintItemsByTaskPrintsIdUi(int taskPrintId, int skip, int limit)
        {
            var sql = Sql.SqlQueryCach["Label.AllLabelsByTaskPrintId"];
            var result = (await _db.Connection.QueryAsync<TaskPrintItemsByTaskPrintsIdUi>(sql, new { id = taskPrintId, skip = skip, limit = limit })).ToList();
            sql = Sql.SqlQueryCach["Label.AllLabelsByTaskPrintIdCount"];
            var count = (await _db.Connection.QueryAsync<int>(sql, new { id = taskPrintId})).FirstOrDefault();

            foreach (var item in result)
            {
                if (item.TimePrinted != 0)
                {
                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
                    item.TimePrintedDateTime = epoch.AddSeconds(item.TimePrinted);
                }
                else
                    item.TimePrintedDateTime = null;
            }

            var output = new TaskPrintItemsByTaskPrintsIdUiPaging
            {
                Data = result,
                Total = count
            };
            return output;
        }
        
        /// <summary>
        /// Получить список выбранных задач с обновлением времени печати
        /// </summary>
        /// <param name="taskPrintId"></param>
        /// <param name="isSelectedAll"></param>
        /// <param name="selectedRows"></param>
        /// <returns></returns>
        public async Task<List<TaskPrintItemsByTaskPrintsIdUi>> GetTaskSelectedItemsByTaskPrintsIdUi(int taskPrintId, bool isSelectedAll, List<int> selectedRows)
        {
            var sql = Sql.SqlQueryCach["Label.SelectLabelsByTaskPrintId"];

            if(selectedRows.Count > 0)
                sql += $"{Environment.NewLine} AND (1={(isSelectedAll ? 1 : 0)} AND tpi.Id NOT IN ({string.Join(",", selectedRows)}) OR 0={(isSelectedAll ? 1 : 0)} AND tpi.Id IN ({string.Join(",", selectedRows)})) ORDER BY tpi.id ASC";

            var result = (await _db.Connection.QueryAsync<TaskPrintItemsByTaskPrintsIdUi>(sql, new { id = taskPrintId })).ToList();

            foreach (var item in result)
            {
                var timePrintedUnixtime = DateTimeOffset.Now.ToUnixTimeSeconds();
                await _db.Connection.ExecuteAsync(Sql.SqlQueryCach["Label.UpdateTimePrinted"], new { id = item.id, timePrinted = timePrintedUnixtime });
            }

            return result;
        }

        public class TaskPrintItemsByTaskPrintsIdUiPaging
        {
            public List<TaskPrintItemsByTaskPrintsIdUi> Data { get; set; }
            public int Total { get; set; }
        }


        /// <summary>
        /// Добавить или обновить задачу
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public async Task<int> AddOrUpdateTaskPrints(TaskPrint task)
        {
            var sql = Sql.SqlQueryCach["Label.AddTaskPrints"];

            if (task.Id == 0)
            {
                var id = await _db.Connection.QueryAsync<int>(sql, new {name = task.Name, userId = task.User.Id, labelType = task.LabelType, templateLabelId = task.TemplateLabelId });
                return id.FirstOrDefault();
            }
            sql = Sql.SqlQueryCach["Label.UpdateTaskPrints"];
            await _db.Connection.QueryAsync<int>(sql, new { name = task.Name, userId = task.User.Id, labelType = task.LabelType, templateLabelId = task.TemplateLabelId, id = task.Id});
            return task.Id;
        }


        /// <summary>
        /// Получить все шаблоны
        /// </summary>
        /// <returns></returns>
        public async Task<List<TemplateLabel>> GetAllTemplateLabels()
        {
            var sql = Sql.SqlQueryCach["Label.AllTemplateLabels"];
            var result = await _db.Connection.QueryAsync<TemplateLabel>(sql);
            return result.ToList();
        }


        /// <summary>
        /// удалить таску по ид
        /// </summary>
        /// <param name="id"></param>
        public async Task DeleteTaskPrints(int id)
        {
            await _db.Connection.ExecuteAsync(Sql.SqlQueryCach["Label.DeleteTaskPrints"], new { id = id });
        }


        public async Task<int> AddLabelWithTaskPtintsItem(LabelWithTaskPrintId input, User user)
        {
            var userId = user.Id;
            var timePrinted = 0;

            //проверяем есть ли такая метка, привязана ли она к этой задаче. если да то ничо не делаем.
            var sql = Sql.SqlQueryCach["Label.GetLabelByCIdAndEMId"];
            var label = (await _db.Connection.QueryAsync<LabelWithTaskPrintId>(sql,
                new {carriageId = input.CarriageId, equipmentModelId = input.EquipmentModelId, taskPrintId = input.TaskPrintId})).ToArray();

            if (label.Length > 0)
            {
                foreach (var value in label)
                {
                    if (input.TaskPrintId == value.TaskPrintId)
                        return value.TaskPrintItemsId;
                }

                var item = label.First();
                sql = Sql.SqlQueryCach["Label.AddTaskPrintItems"];
                var id = await _db.Connection.QueryAsync<int>(sql,
                    new
                    {
                        labelId = item.Id,
                        taskPrintId = input.TaskPrintId,
                        timePrinted = timePrinted,
                        UserId = userId
                    });
                
                return id.FirstOrDefault();
            }

            //хуяк и метки нет. делаем новую.
            _db.Transaction.BeginTransactionIso(IsolationLevel.Serializable);
            var rfid = await GenerateRfid(user, _db.Transaction.Transaction);
            try
            {
                sql = Sql.SqlQueryCach["Label.AddLabel"];
                var labelId = (await _db.Connection.QueryAsync<int>(sql,
                    new
                    {
                        carriageId = input.CarriageId,
                        equipmentModelId = input.EquipmentModelId,
                        labelType = input.LabelType,
                        //rfid = $"1N{DateTimeOffset.Now.ToUnixTimeSeconds()}"
                        rfid = rfid,
                    }, _db.Transaction.Transaction)).FirstOrDefault();

                sql = Sql.SqlQueryCach["Label.AddTaskPrintItems"];
                var id = await _db.Connection.QueryAsync<int>(sql,
                    new
                    {
                        labelId = labelId,
                        taskPrintId = input.TaskPrintId,
                        timePrinted = timePrinted,
                        UserId = userId
                    }, _db.Transaction.Transaction);
                _db.Transaction.Transaction.Commit();
                return id.FirstOrDefault();
            }

            catch (Exception)
            {
                _db.Transaction.RollBackTransaction();
                throw new Exception("Ошибка добавления меток");
            }
        }

        

        /// <summary>
        /// Удалить связку по ид
        /// </summary>
        /// <param name="id"></param>
        public async Task DeleteTaskPrintItem(int id)
        {
            var sql = Sql.SqlQueryCach["Label.DeleteTaskPrintItem"];
            await _db.Connection.ExecuteAsync(sql, new { id = id });
        }


        /// <summary>
        /// Обновить время печати
        /// </summary>
        /// <param name="input"></param>
        public async Task UpdateTimePrinted(TaskPrintItemWithTimePrintedDateTime input)
        {
            var timePrintedUnixtime = DateTimeOffset.Now.ToUnixTimeSeconds();
            await _db.Connection.ExecuteAsync(Sql.SqlQueryCach["Label.UpdateTimePrinted"], new { id = input.Id, timePrinted = timePrintedUnixtime });
        }



        /// <summary>
        /// Возвращает частично внутренности таска
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PrintTaskUi> GetPrintTaskById(int id)
        {
            var sql = Sql.SqlQueryCach["Label.GetPrintTaskById"];
            var result = await _db.Connection.QueryAsync<PrintTaskUi>(sql, new {id = id});
            return result.FirstOrDefault();
        }

        public async Task<string> GenerateRfid(User user, SqlTransaction tran)
        {
            var sql = "SELECT top(1) * from labels WITH (UPDLOCK, ROWLOCK) order by id DESC";
            var result = await _db.Connection.QueryAsync<Label>(sql, null, tran);
            int nextId = 1;
            var lastLabel = result.FirstOrDefault();
            if (lastLabel != null)
            {
                var lastRfid = lastLabel.Rfid;
                var lastId = Int32.Parse(lastRfid.Split("N")[1]); // userid#id = 1N1, ...
                nextId = lastId + 1;
            }

            var rfid = $"{user.Id}N{nextId}";
            return rfid;
        }

        //для ящика
        public async Task<DevExtremeTableData.ReportResponse> GetUserLabels(UserLabelsRequest input)
        {
            var result = new DevExtremeTableData.ReportResponse
            {
                Rows = new List<DevExtremeTableData.Row>(),
                Columns = new List<DevExtremeTableData.Column>
                {
                    new DevExtremeTableData.Column("col0", "ФИО", "string"),
                    new DevExtremeTableData.Column("col1", "Должность", "string"),
                    new DevExtremeTableData.Column("col2", "Табельный номер", "string"),
                    new DevExtremeTableData.Column("col3", "Бригада", "string"),
                }
            };

            const string sql = "select * from [auth_users] u left join [Brigades] b on b.Id=u.BrigadeId where u.Isblocked=0 AND PersonPosition is not null and personnumber is not null";

            var list = (await _db.Connection.QueryAsync<User, Brigade, UserLabelDto>(
                sql, (user, brigade) =>
                {
                    return new UserLabelDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Position = user.PersonPosition,
                        Barcode = user.PersonNumber,
                        BrigadeType = brigade == null? null : TaskRepository.BrigadeTypeToString(brigade.BrigadeType)
                    };
                })).ToList();

            foreach (var item in list)
            {
                result.Rows.Add(new DevExtremeTableData.Row
                {
                    Id = new DevExtremeTableData.RowId { Id = item.Id, Type = 0 },
                    ParentId = null,
                    HasItems = false.ToString(),
                    Col0 = item.Name,
                    Col1 = item.Position,
                    Col2 = item.Barcode,
                    Col3 = item.BrigadeType
                });
            }

            result.Rows = DevExtremeTableUtils.DevExtremeTableFiltering(result.Rows, input.Filters);
            result.Rows = DevExtremeTableUtils.DevExtremeTableSorting(result.Rows, input.Sortings);
            result.Total = result.Rows.Count.ToString();
            result.Paging(input.Paging);

            return result;
        }

        //для ящика
        public async Task<DevExtremeTableData.ReportResponse> GetDeviceLabels(DeviceLabelsRequest input)
        {
            var result = new DevExtremeTableData.ReportResponse
            {
                Rows = new List<DevExtremeTableData.Row>(),
                Columns = new List<DevExtremeTableData.Column>
                {
                    new DevExtremeTableData.Column("col0", "Модель", "string"),
                    new DevExtremeTableData.Column("col1", "Серийный номер", "string"),
                    new DevExtremeTableData.Column("col2", "Номер ячейки", "string")
                }
            };

            const string sql = "select * from [Devices] where CellNumber>0";

            var devices = await _db.Connection.QueryAsync<Device>(sql);

            var list =  devices.Select(o => new DeviceLabelDto
            {
                Id = o.Id,
                Model = o.Name,
                Barcode = o.Serial,
                CellNumber = o.CellNumber
            }).ToList();

            foreach (var item in list)
            {
                result.Rows.Add(new DevExtremeTableData.Row
                {
                    Id = new DevExtremeTableData.RowId { Id = item.Id, Type = 0 },
                    ParentId = null,
                    HasItems = false.ToString(),
                    Col0 = item.Model,
                    Col1 = item.Barcode,
                    Col2 = item.CellNumber.ToString()
                });
            }

            result.Rows = DevExtremeTableUtils.DevExtremeTableFiltering(result.Rows, input.Filters);
            result.Rows = DevExtremeTableUtils.DevExtremeTableSorting(result.Rows, input.Sortings);
            result.Total = result.Rows.Count.ToString();
            result.Paging(input.Paging);

            return result;
        }



        public class PrintTaskUi
        {
            public int id { get; set; }
            public int LabelType { get; set; }
            public string TemplateLabelsName { get; set; }
            public int TemplateLabelsId { get; set; }
            public string Name { get; set; }
            public string UserName { get; set; }
            public DateTime CreateDate { get; set; }
        }

        



        public class TaskPrintItemWithTimePrintedDateTime : TaskPrintItem
        {
            public DateTime TimePrintedDateTime { get; set; }
        }



        public class LabelWithTaskPrintId : Label
        {
            public int TaskPrintId { get; set; }
            public int TaskPrintItemsId { get; set; }
        }


        public class TaskPrintItemsByTaskPrintsIdUi
        {
            public int id { get; set; }
            public int LabelsId { get; set; }
            public string Rfid { get; set; }
            public string EquipmentName { get; set; }
            public long TimePrinted { get; set; }
            public DateTime? TimePrintedDateTime { get; set; }
            public string Template { get; set; }
            public string ModelName { get; set; }
            public string ParentName { get; set; }
            public int CarriageNumber { get; set; }
            public int ModelType { get; set; } 
            public string TrainName { get; set; }
            public string CarriageSerialNumber { get; set;}
        }




        public class TaskPrintsUi : TaskPrint
        {
            public int TaskPrintItemsCount { get; set;}
            public string TemplateLabelsName { get; set; }
            public string UserName { get; set; }

        }


        public class TaskPrintsUiPaging
        {
            public TaskPrintsUi[] Data { get; set; }
            public int Total { get; set; }
        }

        public void Dispose()
        {
            _db.Connection.Close();
        }

        public class UserLabelDto
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string BrigadeType { get; set; }

            public string Position { get; set; }

            public string Barcode { get; set; }
        }

        public class DeviceLabelDto
        {
            public int Id { get; set; }

            public string Model { get; set; }

            public string Barcode { get; set; }

            public int CellNumber { get; set; }
        }

        public class DeviceLabelsRequest
        {
            public DevExtremeTableData.RowId ParentId { get; set; }
            public DevExtremeTableData.Paging Paging { get; set; }
            public List<DevExtremeTableData.Filter> Filters { get; set; }
            public List<DevExtremeTableData.Sorting> Sortings { get; set; }
        }

        public class UserLabelsRequest
        {
            public DevExtremeTableData.RowId ParentId { get; set; }
            public DevExtremeTableData.Paging Paging { get; set; }
            public List<DevExtremeTableData.Filter> Filters { get; set; }
            public List<DevExtremeTableData.Sorting> Sortings { get; set; }
        }
    }
}
