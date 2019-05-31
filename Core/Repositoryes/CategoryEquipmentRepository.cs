using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.CategoryEquipment;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using static System.Int32;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes
{
    public class CategoryEquipmentRepository : ICategoryEquipmentRepository, IDisposable
    {
        private readonly IDb _db;
        private string _sql;
        private string _sqlfilter;
        private string _sqlc;
        private readonly ILogger _logger;


        public CategoryEquipmentRepository(ILogger logger)
        {
            _db = new Db();
            _logger = logger;
        }


        //public async Task<EquipmentCategoryPaging> GetAll(int skip, int limit)
        //{
        //    var sql = Sql.SqlQueryCach["CategoryEquipment.All"];
        //    var result = await _db.Connection.QueryAsync<EquipmentCategory>(sql, new { skip = skip, limit = limit });
        //    var sqlc = Sql.SqlQueryCach["CategoryEquipment.CountAll"];
        //    var count = _db.Connection.ExecuteScalar<int>(sqlc);
        //    var output = new EquipmentCategoryPaging
        //    {
        //        Data = result.ToArray(),
        //        Total = count
        //    };

        //    return output;
        //}


        /// <summary>
        /// Получить категории оборудовани
        /// </summary>
        public async Task<EquipmentCategoryPaging> GetAll(int skip, int limit, FilterBody[] filters = null)
        {
            var crunchFilter = "equipmentname";
            var crunch = new List<FilterBody>();

            _sql = Sql.SqlQueryCach["CategoryEquipment.All"];
            _sqlc = Sql.SqlQueryCach["CategoryEquipment.CountAll"];

            #region Filter



            if (filters != null)
            {
                crunch = filters.Where(e => e.Filter.ToLower().Equals(crunchFilter)).ToList();
                //Категории можно фильтровать только по имени
                var filter = filters.Where(e => e.Filter.ToLower().Equals("name")).ToArray();
                if (filter.Any())
                {
                    CreateFilter(filter, out _sqlfilter, out _sql);
                    _sqlc = $"{CategoryEquipmentCommon.sqlCountCommon} {_sqlfilter}";
                }
            }

            #endregion

            List<EquipmentCategory> sqlResult;
            int count = 0;
            if (crunch.Any())
                sqlResult = (await _db.Connection.QueryAsync<EquipmentCategory>(_sql, new { skip = 0, limit = MaxValue })).ToList();
            else
                //using (var conn = new SqlConnection(AppSettings.ConnectionString))
                //{
                    sqlResult = (await _db.Connection.QueryAsync<EquipmentCategory>(_sql, new { skip = skip, limit = limit })).ToList();
                    count = _db.Connection.ExecuteScalar<int>(_sqlc);
                //}
                
            
            var result = new EquipmentCategoryPaging
            {
                Data = sqlResult.ToList(),
                Total = count
            };


            //Теперь мегокастыль. Проверяем есть ли фильтр на оборудование. Если есть то фильтруем по нему
            if (crunch.Any())
            {
                result.Data = new List<EquipmentCategory>();
                var eR = new EquipmentRepository(_logger);
                foreach (var equipmentCategory in sqlResult)
                {
                    var equipments = await eR.GetByCategory(equipmentCategory);
                    if (equipments.Data.Any(e => e.Name.ToLower().Contains(crunch.FirstOrDefault()?.Value.ToLower())))
                        result.Data.Add(equipmentCategory);
                }
                result.Total = result.Data.Count;
                result.Data = result.Data.TakeWhile((e, i) => i >= skip && i <= skip + limit).ToList();
            }

            
            return result;
        }

        //public async Task<EquipmentCategoryPaging> GetAllEquipmentFilter(int skip, int limit, FilterBody filter = null)
        //{
        //    var equipmentCategoryes = await GetAll(skip, limit, filter);
        //    var eR = new EquipmentRepository();

        //    foreach (var equipmentCategory in equipmentCategoryes.Data)
        //    {
        //        var equipments = await eR.GetByCategory(equipmentCategory);
        //        //var good = equipments.Data.Where(e => e.Name.Contains())
        //    }


        //    throw new NotImplementedException();
        //}


        private static void CreateFilter(FilterBody[] filters, out string sqlfilter, out string sql)
        {
            //var filters = JsonConvert.DeserializeObject<FilterBody[]>(filter);
            sqlfilter = "where ";
            for (var index = 0; index < filters.Length; index++)
            {
                var item = filters[index];
                sqlfilter = $"{sqlfilter} {item.Filter} like '%{item.Value}%' ";
                if (index < (filters.Length - 1))
                    sqlfilter = $"{sqlfilter} AND ";
            }
            sql = $"{CategoryEquipmentCommon.sqlCommon} {sqlfilter} {SqlQueryPagingEnd}";
        }

        public async Task<EquipmentCategory> GetById(int id)
        {
            var sql = Sql.SqlQueryCach["CategoryEquipment.ById"];
            var result = await _db.Connection.QueryAsync<EquipmentCategory>(sql, new { category_id = id });
            return result.FirstOrDefault();
        }

        public async Task Update(EquipmentCategory input)
        {
            await _db.Connection.ExecuteAsync(Sql.SqlQueryCach["CategoryEquipment.Update"],
                new { name = input.Name, description = input.Description, id = input.Id });
        }

        public async Task Add(EquipmentCategory equipmentCategory)
        {
            await _db.Connection.ExecuteAsync(Sql.SqlQueryCach["CategoryEquipment.Add"],new {name = equipmentCategory.Name, description = equipmentCategory.Description});
        }

        public async Task Delete(int id)
        {
            try
            { 
                await _db.Connection.ExecuteAsync(Sql.SqlQueryCach["CategoryEquipment.Delete"],new { id = id});
            }
            catch (Exception)
            {
                throw new Exception(DeleteException);
            }
        }

        public class EquipmentCategoryPaging
        {
            public List<EquipmentCategory> Data { get;set;}
            public int Total { get; set;}
        }

        public void Dispose()
        {
            _db.Connection.Close();
        }
    }
}
