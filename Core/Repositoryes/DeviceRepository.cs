using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Helpers;
using Rzdppk.Core.Options;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Dto;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public class DeviceRepository : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IDb _db;
        private int _maxCellNum = 27;

        public DeviceRepository(ILogger logger)
        {
            _db = new Db();
            _logger = logger;
        }

        public DeviceRepository(IDb db)
        {
            _db = db;
        }

        public DeviceRepository()
        {
            _db = new Db();
        }

        public async Task<DevExtremeTableData.ReportResponse> GetTable(DeviceRequest input)
        {
            var result = new DevExtremeTableData.ReportResponse();

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = @"select d.*,LastStatus.*,LastCharge.*,u.*,
                (SELECT Count(*) FROM DeviceTasks t OUTER APPLY(SELECT TOP 1 * FROM DeviceTaskComments c WHERE t.Id = c.DeviceTaskId ORDER BY c.Date DESC) as LastComment WHERE LastComment.Status <> 2 And t.DeviceId = d.id) as Id
                from [Devices] d
                OUTER APPLY(SELECT TOP 1 * FROM DeviceHistories h WHERE h.DeviceId = d.Id ORDER BY h.UpdateDate DESC) as LastStatus
                OUTER APPLY(SELECT TOP 1 * FROM DeviceValues v WHERE v.DeviceId = d.Id ORDER BY v.UpdateDate DESC) as LastCharge
                left join [auth_users] u on u.Id=LastStatus.UserId";

                var items = (await conn.QueryAsync<Device, DeviceHistory, DeviceValue, User, int, DeviceTableItemDto>(
                    sql, (device, history, value, historyUser, openTaskCnt) =>
                    {
                        var item = new DeviceTableItemDto
                        {
                            Id = device.Id,
                            Cell = device.CellNumber > 0 ? $"{device.CellNumber}" : "-",
                            Serial = device.Serial,
                            Model = device.Name,
                            ChargePercent = value?.Value,
                            IsDeleted = device.CellNumber <= 0,
                            InRepair = openTaskCnt > 0,
                        };

                        string status = "";
                        DateTime? dateIssued = null;
                        string lastUser = null;

                        switch (history?.Operation)
                        {
                            case DeviceOperation.Surrender:
                                status = "на месте";
                                lastUser = GetShortFio(historyUser?.Name);
                                break;

                            case DeviceOperation.Issue:
                                dateIssued = history.UpdateDate;
                                status = "отсутствует";
                                lastUser = GetShortFio(historyUser?.Name);
                                break;
                        }

                        item.Status = status;
                        item.DateIssued = dateIssued;
                        item.LastUser = lastUser;

                        return item;
                    })).ToList();

                result.Columns = new List<DevExtremeTableData.Column>
                {
                    new DevExtremeTableData.Column("col0", "Ячейка", "number"),
                    new DevExtremeTableData.Column("col1", "Модель", "string"),
                    new DevExtremeTableData.Column("col2", "Серийный номер", "string"),
                    new DevExtremeTableData.Column("col3", "Статус", "string"),
                    new DevExtremeTableData.Column("col4", "Посл.пользователь", "string"),
                    new DevExtremeTableData.Column("col5", "В ремонте", "boolean"),
                    new DevExtremeTableData.Column("col6", "Дата выдачи", "date"),
                    new DevExtremeTableData.Column("col7", "Процент заряда", "string"),
                    new DevExtremeTableData.Column("col8", "Удалено", "boolean"),
                };

                result.Rows = new List<DevExtremeTableData.Row>();

                if (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        result.Rows.Add(new DevExtremeTableData.Row
                        {
                            Id = new DevExtremeTableData.RowId {Id = item.Id, Type = 0},
                            HasItems = false.ToString(),
                            Col0 = item.Cell,
                            Col1 = item.Model,
                            Col2 = item.Serial,
                            Col3 = item.Status,
                            Col4 = item.LastUser,
                            Col5 = item.InRepair.BoolToStringRussian(),
                            Col6 = item.DateIssued?.ToStringDateTime(),
                            Col7 = item.ChargePercent.ToString(),
                            Col8 = item.IsDeleted.BoolToStringRussian()
                        });
                    }
                }
            }

            result.Rows = DevExtremeTableUtils.DevExtremeTableFiltering(result.Rows, input.Filters);
            result.Rows = DevExtremeTableUtils.DevExtremeTableSorting(result.Rows, input.Sortings);
            result.Total = result.Rows.Count.ToString();
            result.Paging(input.Paging);

            return result;
        }

        public async Task<Device> Add(Device input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                if (input.CellNumber < 1 || input.CellNumber > _maxCellNum)
                {
                    throw new Exception($"Номер ячейки должен быть между 1 и {_maxCellNum}");
                }

                var dev = await ByCellNumber(input.CellNumber);

                if (dev != null)
                {
                    throw new Exception($"ячейка {input.CellNumber} уже занята");
                }

                var dev1 = await BySerial(input.Serial);

                if (dev1 != null)
                {
                    throw new Exception($"серийный номер {input.Serial} уже занят");
                }

                const string sql = "INSERT INTO [Devices] ([Name],[Serial],[CellNumber]) VALUES(@Name, @Serial, @CellNumber) SELECT SCOPE_IDENTITY()";

                var id = await conn.QueryFirstOrDefaultAsync<int>(sql, new { Name = input.Name, Serial = input.Serial, CellNumber = input.CellNumber });

                return await ById(id);
            }
        }

        public async Task<Device> Update(Device input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var dev = await BySerial(input.Serial);

                if (dev.Id != input.Id)
                {
                    throw new Exception($"серийный номер {input.Serial} уже занят");
                }

                const string sql = "UPDATE [Devices] SET [Name]=@Name, [Serial]=@Serial, [UpdateDate]=GETDATE() WHERE id=@Id";

                await conn.ExecuteAsync(sql, new { Name = input.Name, Serial = input.Serial, Id = input.Id });

                return await ById(input.Id);
            }
        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                const string sql = "UPDATE [Devices] SET CellNumber=-1, [UpdateDate]=GETDATE() WHERE id=@Id";

                await conn.ExecuteAsync(sql, new {Id = id});
            }
        }

        public async Task<Device> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                const string sql = "SELECT * FROM [Devices] WHERE id=@Id";

                return await conn.QueryFirstOrDefaultAsync<Device>(sql, new { Id = id });
            }
        }

        public async Task<Device> ByCellNumber(int num)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                const string sql = "SELECT * FROM [Devices] WHERE CellNumber=@CellNumber";

                return await conn.QueryFirstOrDefaultAsync<Device>(sql, new { CellNumber = num });
            }
        }

        public async Task<Device> BySerial(string serial)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                const string sql = "SELECT * FROM [Devices] WHERE Serial=@Serial AND CellNumber>0";

                return await conn.QueryFirstOrDefaultAsync<Device>(sql, new { Serial = serial });
            }
        }

        public async Task<DeviceDto[]> GetAllForSync()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                const string sql = @"select d.*, 
                                   (SELECT Count(*) FROM DeviceTasks t OUTER APPLY(SELECT TOP 1 * FROM DeviceTaskComments c WHERE t.Id = c.DeviceTaskId ORDER BY c.Date DESC) as LastComment WHERE LastComment.Status <> 2 And t.DeviceId = d.id) as OpenTasksCount,
                                   LastStatus.Operation as LastOperation,LastStatus.UpdateDate as LastOperationDate,LastStatus.UserId as LastOperationUserId,
                                   LastCharge.Value as LastCharge,LastCharge.UpdateDate as LastChargeDate
                                       from devices d
                                   OUTER APPLY(SELECT TOP 1 * FROM DeviceHistories h WHERE h.DeviceId = d.Id ORDER BY h.UpdateDate DESC) as LastStatus
                                       OUTER APPLY(SELECT TOP 1 * FROM DeviceValues v WHERE v.DeviceId = d.Id ORDER BY v.UpdateDate DESC) as LastCharge";
                var result = (await conn.QueryAsync<DeviceDto>(sql, new {})).ToArray();

                return result;
            }
        }

        public async Task CreateOperation(OperationOutDto oper)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var SqlCreateRemoteOperation = "INSERT INTO [DeviceHistories] ([DeviceId],[Operation],[UserId],[RefId]) VALUES (@DeviceId,@Operation,@UserId,@RefId)";

                await conn.ExecuteAsync(SqlCreateRemoteOperation,
                    new
                    {
                        oper.DeviceId,
                        oper.Operation,
                        oper.UserId,
                        oper.RefId
                    });
            }
        }

        private static string GetShortFio(string fio)
        {
            var ret = "?";
            if (fio != null)
            {
                var tokens = fio.Split(' ');
                if (tokens.Length > 0)
                {
                    ret = tokens[0]; //фамилия
                    if (tokens.Length > 1) //инициалы
                    {
                        ret += $" {tokens[1].Substring(0, 1).ToUpper()}.";
                        if (tokens.Length > 2)
                        {
                            ret += $"{tokens[2].Substring(0, 1).ToUpper()}.";
                        }
                    }
                }
            }

            return ret;
        }

        public void Dispose()
        {
            _db.Connection?.Close();
        }
    }

    public class DeviceRequest
    {
        public DevExtremeTableData.Paging Paging { get; set; }
        public List<DevExtremeTableData.Filter> Filters { get; set; }
        public List<DevExtremeTableData.Sorting> Sortings { get; set; }
    }

    public class DeviceTableItemDto
    {
        public int Id { get; set; }

        public string Cell { get; set; }

        public string Model { get; set; }

        public string Serial { get; set; }

        public string Status { get; set; }

        public string LastUser { get; set; }

        public bool InRepair { get; set; }

        public DateTime? DateIssued { get; set; }

        public int? ChargePercent { get; set; }

        public bool IsDeleted { get; set; }
    }
}
