using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Newtonsoft.Json;
using Rzdppk.Core.Options;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Model;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Core.Repositoryes
{
    public class ModelRepository : IModelRepository
    {
        private static ILogger _logger;
        

        public ModelRepository(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<EquipmentModel> AddEquipmentToModel(EquipmentModel equipmentModel)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Model.AddEquipmentToModel"];
                var id = (await conn.QueryAsync<int>(sql, new
                {
                    equipmentId = equipmentModel.EquipmentId,
                    modelId = equipmentModel.ModelId,
                    parentId = equipmentModel.ParentId == 0 ? null : equipmentModel.ParentId,
                    equipmentModel.IsMark
                })).FirstOrDefault();

                equipmentModel.Id = id;
                return equipmentModel;
            }
        }

        public async Task UpdateEquipment(EquipmentModel equipmentModel)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Model.UpdateEquipment"];
                await conn.ExecuteAsync(sql,
                    new {id = equipmentModel.Id, equipment_id = equipmentModel.EquipmentId, equipmentModel.IsMark});
            }
        }

        public async Task DeleteEquipmentFromModel(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Model.DeleteEquipmentFromModel"], new {id = id});
            }
        }

        public async Task<EquipmentModelExt[]> GetEquipmentByModel(Model.Model model)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Model.GetEquipmentByModel"];
                var result = await conn.QueryAsync<EquipmentModelExt, Equipment, EquipmentModelExt>(
                    sql,
                    (em, equipment) =>
                    {
                        em.Equipment = equipment;
                        return em;
                    }, new {model_id = model.Id});

                return result.ToArray();
            }
        }

        /*public async Task<LocationModelExt[]> GetLocationsByModel(Model.Model model)
        {
            var sql = Sql.SqlQueryCach["Model.GetLocationsByModel"];
            var result = await conn.QueryAsync<LocationModelExt, Equipment, LocationModelExt>(
                sql,
                (em, equipment) =>
                {
                    em.Location = equipment;
                    return em;
                }, new { model_id = model.Id });

            return result.ToArray();
        }*/

        public async Task<List<Model.Model>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Model.All"];
                var result = await conn.QueryAsync<Model.Model>(sql, new {skip = 0, limit = int.MaxValue});

                return result.ToList();
            }
        }

        public async Task<ModelPaging> GetAll(int skip, int limit)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Model.All"];
                var result = (await conn.QueryAsync<Model.Model>(sql, new {skip = skip, limit = limit}))
                    .ToArray();
                var sqlc = Sql.SqlQueryCach["Model.CountAll"];
                var count = (await conn.QueryAsync<int>(sqlc)).FirstOrDefault();
                var output = new ModelPaging
                {
                    Data = result,
                    Total = count
                };

                return output;
            }
        }

        public async Task<ModelPaging> GetAll(int skip, int limit, string filter)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                string sqlfilter, sql;
                CreateFilter(filter, out sqlfilter, out sql);
                var result = (await conn.QueryAsync<Model.Model>(sql, new {skip = skip, limit = limit}))
                    .ToArray();
                var sqlc = $"{ModelCommon.sqlCountCommon} {sqlfilter}";
                var count = (await conn.QueryAsync<int>(sqlc)).FirstOrDefault();
                var output = new ModelPaging
                {
                    Data = result,
                    Total = count
                };

                return output;
            }
        }

        private static void CreateFilter(string filter, out string sqlfilter, out string sql)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var filters = JsonConvert.DeserializeObject<Other.Other.FilterBody[]>(filter);
                sqlfilter = "where ";
                for (var index = 0; index < filters.Length; index++)
                {
                    var item = filters[index];
                    sqlfilter = $"{sqlfilter} {item.Filter} like '%{item.Value}%' ";
                    if (index < (filters.Length - 1))
                        sqlfilter = $"{sqlfilter} AND ";

                }
                sql = $"{ModelCommon.sqlCommon} {sqlfilter} {Other.Other.SqlQueryPagingEnd}";
            }
        }

        public async Task<Model.Model> GetById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Model.ById"];
                var result = await conn.QueryAsync<Model.Model>(sql, new {model_id = id});
                return result.FirstOrDefault();
            }
        }

        public async Task Update(Model.Model input)
        {
            var all = await GetAll();
            if (all.Any(x => x.Name.Equals(input.Name)))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Model.Update"],
                    new
                    {
                        name = input.Name,
                        description = input.Description,
                        modeltype = (int) input.ModelType,
                        id = input.Id
                    });
            }
        }

        public async Task<Model.Model> Add(Model.Model model)
        {
            var all = await GetAll();
            if (all.Any(x => x.Name.Equals(model.Name)))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Model.Add"];
                var id = await conn.QueryAsync<int>(sql,
                    new {name = model.Name, description = model.Description, model_type = (int) model.ModelType});

                model.Id = id.First();

                return model;
            }
        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Model.Delete"], new {id = id});
            }
        }

        public class ModelPaging
        {
            public Model.Model[] Data{ get; set; }
            public int Total { get; set; }
        }

        /*public class LocationModelExt
        {
            public int Id { get; set; }
            public int ModelId { get; set; }
            public Equipment Location { get; set; }
            public bool CanDelete { get; set; }
        }*/

        public class EquipmentModelExt
        {
            public int Id { get; set; }
            public int ModelId { get; set; }
            public Equipment Equipment { get; set; }
            public int ParentId { get; set; }
            public bool CanDelete { get; set; }
            public bool IsMark { get; set; }
        }

        public class EquipmentTmp
        {
            public int EquipmentId { get; set; }
            public int EquipmentModelId { get; set; }
            public string EquipmentName { get; set; }
            public bool IsMark { get; set; }
            public int ParentId { get; set; }
            public int Id { get; set; }
        }

    }
}
