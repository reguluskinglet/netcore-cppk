using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
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
    public class DeviceValueRepository : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IDb _db;

        public DeviceValueRepository(ILogger logger)
        {
            _db = new Db();
            _logger = logger;
        }

        public DeviceValueRepository(IDb db)
        {
            _db = db;
        }

        public DeviceValueRepository()
        {
            _db = new Db();
        }

        public async Task<DevExtremeTableData.ReportResponse> GetLocationsTable(DeviceValueRequest input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = new DevExtremeTableData.ReportResponse();

                const string sql = "SELECT * FROM [DeviceValues] WHERE [DeviceId]=@DeviceId";

                var items = (await conn.QueryAsync<DeviceValue>(sql, new { DeviceId = input.DeviceId }))
                    .Select(o =>
                        new DeviceLocationItemDto
                        {
                            Id = o.Id,
                            Lat = o.Lat,
                            Lng = o.Lng,
                            Date = o.UpdateDate
                        }).ToList();

                result.Columns = new List<DevExtremeTableData.Column>
                {
                    new DevExtremeTableData.Column("col0", "Широта", "string"),
                    new DevExtremeTableData.Column("col1", "Долгота", "string"),
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
                            Col0 = item.Lat.ToString(CultureInfo.CurrentCulture),
                            Col1 = item.Lng.ToString(CultureInfo.CurrentCulture),
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

        public async Task<DevExtremeTableData.ReportResponse> GetChargesTable(DeviceValueRequest input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = new DevExtremeTableData.ReportResponse();

                const string sql = "SELECT * FROM [DeviceValues] WHERE [DeviceId]=@DeviceId";

                var items = (await conn.QueryAsync<DeviceValue>(sql, new { DeviceId = input.DeviceId }))
                    .Select(o =>
                        new DeviceChargeItemDto
                        {
                            Id = o.Id,
                            Charge = o.Value,
                            Date = o.UpdateDate
                        }).ToList();

                result.Columns = new List<DevExtremeTableData.Column>
                {
                    new DevExtremeTableData.Column("col0", "Уровень заряда", "string"),
                    new DevExtremeTableData.Column("col1", "Дата", "date"),
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
                            Col0 = item.Charge.ToString(CultureInfo.CurrentCulture),
                            Col1 = item.Date.ToStringDateTime()
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

        public void Dispose()
        {
            _db.Connection?.Close();
        }
    }

    public class DeviceValueRequest
    {
        public int DeviceId { get; set; }
        public DevExtremeTableData.Paging Paging { get; set; }
        public List<DevExtremeTableData.Filter> Filters { get; set; }
        public List<DevExtremeTableData.Sorting> Sortings { get; set; }
    }

    public class DeviceLocationItemDto
    {
        public int Id { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }

        public DateTime Date { get; set; }
    }

    public class DeviceChargeItemDto
    {
        public int Id { get; set; }

        public int Charge { get; set; }

        public DateTime Date { get; set; }
    }
}
