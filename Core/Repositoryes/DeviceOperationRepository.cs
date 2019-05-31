using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public class DeviceOperationRepository : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IDb _db;

        public DeviceOperationRepository(ILogger logger)
        {
            _db = new Db();
            _logger = logger;
        }

        public DeviceOperationRepository(IDb db)
        {
            _db = db;
        }

        public DeviceOperationRepository()
        {
            _db = new Db();
        }

        public async Task<DevExtremeTableData.ReportResponse> GetTable(DeviceOperationRequest input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = new DevExtremeTableData.ReportResponse();

                const string sql = "SELECT * FROM [DeviceHistories] h LEFT JOIN [auth_users] u on u.Id=h.UserId WHERE h.[DeviceId]=@DeviceId";

                var items = (await conn.QueryAsync<DeviceHistory, User, DeviceOperationItemDto>(
                    sql, (history, historyUser) =>
                    {
                        return new DeviceOperationItemDto
                        {
                            Id = history.Id,
                            User = historyUser.Name,
                            Date = history.UpdateDate,
                            Operation = GetOperationString(history.Operation)
                        };
                    }, new { DeviceId = input.DeviceId })).ToList();


                result.Columns = new List<DevExtremeTableData.Column>
                {
                    new DevExtremeTableData.Column("col0", "Пользователь", "string"),
                    new DevExtremeTableData.Column("col1", "Операция", "string"),
                    new DevExtremeTableData.Column("col2", "Дата", "date"),
                };

                result.Rows = new List<DevExtremeTableData.Row>();

                if (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        result.Rows.Add(new DevExtremeTableData.Row
                        {
                            Id = new DevExtremeTableData.RowId { Id = item.Id, Type = 0 },
                            HasItems = false.ToString(),
                            Col0 = item.User,
                            Col1 = item.Operation,
                            Col2 = item.Date.ToStringDateTime()
                        });
                    }
                }

                result.Rows = DevExtremeTableUtils.DevExtremeTableFiltering(result.Rows, input.Filters);
                result.Rows = DevExtremeTableUtils.DevExtremeTableSorting(result.Rows, input.Sortings);
                result.Total = result.Rows.Count.ToString();
                result.Paging(input.Paging);

                return result;
            }
        }

        public static string GetOperationString(DeviceOperation oper)
        {
            var ret = "?";

            switch (oper)
            {
                case DeviceOperation.Issue:
                    ret = "выдано";
                    break;
                case DeviceOperation.Surrender:
                    ret = "возвращено";
                    break;
            }

            return ret;
        }

        public void Dispose()
        {
            _db.Connection?.Close();
        }
    }

    public class DeviceOperationRequest
    {
        public int DeviceId { get; set; }
        public DevExtremeTableData.Paging Paging { get; set; }
        public List<DevExtremeTableData.Filter> Filters { get; set; }
        public List<DevExtremeTableData.Sorting> Sortings { get; set; }
    }

    public class DeviceOperationItemDto
    {
        public int Id { get; set; }

        public string User { get; set; }

        public string Operation { get; set; }

        public DateTime Date { get; set; }
    }
}
