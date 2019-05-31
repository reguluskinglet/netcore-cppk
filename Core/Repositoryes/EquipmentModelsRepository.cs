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
    public class EquipmentModelsRepository
    {

        private readonly ILogger _logger;
        private readonly EquipmentModelsRepositorySql _sql;

        public EquipmentModelsRepository(ILogger logger)
        {
            _logger = logger;
            _sql = new EquipmentModelsRepositorySql();
        }

        public async Task<EquipmentModelPaging> GetAll(int skip, int limit, string filter)
        {
            CreateSqlFilterQuery(skip, limit, filter, out var sqlQueryData, out var sqlQueryCount, _sql);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                var result = await conn.QueryAsync<EquipmentModel>(sqlQueryData);
                var count = await conn.ExecuteScalarAsync<int>(sqlQueryCount);
                var output = new EquipmentModelPaging()
                {
                    Data = result.ToList(),
                    Total = count
                };

                return output;
            }
        }

        public class EquipmentModelPaging
        {
            public List<EquipmentModel> Data { get; set; }
            public int Total { get; set; }
        }

        public async Task<List<EquipmentModel>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<EquipmentModel>(_sql.GetAll());
                return result.ToList();
            }
        }

        public async Task<EquipmentModel> Add(EquipmentModel input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryFirstOrDefaultAsync<int>(_sql.Add(input));
                return await ById(id);
            }
        }

        public async Task<EquipmentModel> Update(EquipmentModel input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Update(input));
                return await ById(input.Id);
            }
        }

        public async Task<EquipmentModel> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<EquipmentModel>(_sql.ById(id));
            }
        }

        public async void Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(_sql.Delete(id));
            }
        }

        public async Task<List<EquipmentModel>> ByModelId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return (await conn.QueryAsync<EquipmentModel>(_sql.ByModelId(id))).ToList();
            }
        }

    }
}