using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Model;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes
{
    public class CheckListEquipmentsRepository
    {

        private readonly ILogger _logger;
        private readonly CheckListEquipmentsSql _sql;

        public CheckListEquipmentsRepository(ILogger logger)
        {
            _logger = logger;
            _sql = new CheckListEquipmentsSql();
        }

        public async Task<CheckListEquipmentPaging> GetAll(int skip, int limit, string filter)
        {
            CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, _sql);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<CheckListEquipment>(sqlQueryData);
                var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
                var output = new CheckListEquipmentPaging()
                {
                    Data = result.ToList(),
                    Total = count
                };

                return output;
            }
        }

        public class CheckListEquipmentPaging
        {
            public List<CheckListEquipment> Data { get; set; }
            public int Total { get; set; }
        }

        public async Task<List<CheckListEquipment>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<CheckListEquipment>(_sql.GetAll());
                return result.ToList();
            }
        }

        public async Task<CheckListEquipment> Add(CheckListEquipment input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
                return await ById(id);
            }
        }

        public async Task<CheckListEquipment> Update(CheckListEquipment input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Update(input));
                return await ById(input.Id);
            }
        }

        public async Task<List<CheckListEquipment>> ByEquipmentModelId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return (await conn.QueryAsync<CheckListEquipment>(_sql.ByEquipmentModelId(id))).ToList();
            }
        }

        public async Task<CheckListEquipment> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<CheckListEquipment>(_sql.ById(id));
            }
        }

        public async void Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Delete(id));
            }
        }

    }
}