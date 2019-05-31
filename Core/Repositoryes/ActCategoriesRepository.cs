using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Brigade;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes 
{
    public class ActCategoriesRepository : IActCategoriesRepository, IDisposable
    {
        private readonly IDb _db;
        
        public ActCategoriesRepository()
        {
            _db = new Db();
        }

        public async Task<List<ActCategory>> GetAll()
        {
            var sql = Sql.SqlQueryCach["ActCategories.All"];
            var result = await _db.Connection.QueryAsync<ActCategory>(sql);
            _db.Connection.Close();
            return result.ToList();
        }

        public async Task<List<ActCategory>> GetByEquipmentId(int id)
        {
            var sql = Sql.SqlQueryCach["ActCategories.ByEquipmentId"];
            var result = await _db.Connection.QueryAsync<ActCategory>(sql, new { equipmentId = id});
            
            return result.ToList();
        }

        public async Task AddEquipmentToAct(int actCategoryId, int equipmentId)
        {
            var sql = Sql.SqlQueryCach["ActCategories.AddEquipmentToAct"];
            await _db.Connection.ExecuteAsync(sql, new { @actCategoryId = actCategoryId, equipmentId = equipmentId });
        }

        public async Task UpdateEquipmentToAct(int actCategoryId, int equipmentId)
        {
            var sql = Sql.SqlQueryCach["ActCategories.UpdateEquipmentToAct"];
            await _db.Connection.ExecuteAsync(sql, new { @actCategoryId = actCategoryId, equipmentId = equipmentId });
        }

        public async Task<List<EquipmentActsPdf>> GetAllForPdf(string paragraph, int count)
        {
            var sql = Sql.SqlQueryCach["ActCategories.All"];
            var result = await _db.Connection.QueryAsync<ActCategory>(sql);

            var output = new List<EquipmentActsPdf>();

            var actCategories = result as IList<ActCategory> ?? result.ToList();
            var sanitar = actCategories.Where(e => e.Description.Equals(paragraph));

            count = ConvertToEquipmentActsPdf(output, sanitar, count);

            //var sqlc = Sql.SqlQueryCach["Brigade.CountAll"];
            //var count = _db.Connection.ExecuteScalar<int>(sqlc);
            //var output = new BrigadePaging()
            //{
            //    Data = result.ToArray(),
            //    Total = count
            //};

            return output;
        }

        public static int ConvertToEquipmentActsPdf(List<EquipmentActsPdf> output, IEnumerable<ActCategory> sanitar, int count)
        {
            var countInternal = 1;
            foreach (var item in sanitar)
            {
                var toAdd = new EquipmentActsPdf
                {
                    Id = item.Id,
                    Number = $"{count}.{countInternal}",
                    ActName = item.Name
                };

                output.Add(toAdd);

                countInternal++;
            }
            count++;
            return count;
        }

        public string CreateCarriageStrintToPdf(List<int> carriages)
        {
            var uniq = carriages.Distinct();
            string output = null;
            foreach (var item in uniq)
            {
                output += $" {CreateCarriageNameWithOutTrain(item)}";
            }

            return output;
        }


        public class EquipmentActsPdf
        {
            public int Id { get; set; }
            public string Number { get; set; }
            public string ActName { get; set; }
            public int? GoodR1 { get; set; }
            public int? BadR1 { get; set; }
            public List<int> CarriageNumR1 { get; set; }
            public int? GoodR2 { get; set; }
            public int? BadR2 { get; set; }
            public List<int> CarriageNumR2 { get; set; }
            public int? GoodR3 { get; set; }
            public int? BadR3 { get; set; }
            public List<int> CarriageNumR3 { get; set; }
        }

        public void Dispose()
        {
            _db.Connection.Close();
        }

}
}

//        /// <summary>
        //        /// Получить все с пагинацией
        //        /// </summary>
        //        /// <param name="skip"></param>
        //        /// <param name="limit"></param>
        //        /// <returns></returns>
        //        public BrigadePaging GetAllSync(int skip, int limit)
        //        {
        //            var sql = Sql.SqlQueryCach["Brigade.All"];
        //            var result = _db.Connection.Query<Brigade>(sql, new { skip = skip, limit = limit });
        //            var sqlc = Sql.SqlQueryCach["Brigade.CountAll"];
        //            var count = _db.Connection.ExecuteScalar<int>(sqlc);
        //            var output = new BrigadePaging()
        //            {
        //                Data = result.ToArray(),
        //                Total = count
        //            };

        //            return output;
        //        }

        //        public async Task<BrigadePaging> GetAll(int skip, int limit, string filter)
        //        {
        //            string sqlfilter, sql;
        //            CreateFilter(filter, out sqlfilter, out sql);
        //            var result = await _db.Connection.QueryAsync<Brigade>(sql, new { skip = skip, limit = limit });
        //            var sqlc = $"{BrigadeCommon.sqlCountCommon} {sqlfilter}";
        //            var count = _db.Connection.ExecuteScalar<int>(sqlc);
        //            var output = new BrigadePaging()
        //            {
        //                Data = result.ToArray(),
        //                Total = count
        //            };

        //            return output;
        //        }

        //        private static void CreateFilter(string filter, out string sqlfilter, out string sql)
        //        {
        //            var filters = JsonConvert.DeserializeObject<Other.Other.FilterBody[]>(filter);
        //            sqlfilter = "where ";
        //            for (var index = 0; index < filters.Length; index++)
        //            {
        //                var item = filters[index];
        //                sqlfilter = $"{sqlfilter} {item.Filter} like '%{item.Value}%' ";
        //                if (index < (filters.Length - 1))
        //                    sqlfilter = $"{sqlfilter} AND ";

        //            }

        //            sql = $"{BrigadeCommon.sqlCommon} {sqlfilter} {Other.Other.SqlQueryPagingEnd}";
        //        }

        //        public Brigade ByIdWithStations(int id)
        //        {
        //            var sql = Sql.SqlQueryCach["Brigade.ById"];
        //            var result = _db.Connection.Query<Brigade>(sql, new { brigade_id = id });
        //            return result.FirstOrDefault();
        //        }

        //        public void Update(Brigade input)
        //        {
        //            _db.Connection.Execute(Sql.SqlQueryCach["Brigade.Update"],
        //                new { name = input.Name, description = input.Description, brigadeType = (int)input.BrigadeType, id = input.Id});
        //        }


        //        public void Add(Brigade brigade)
        //        {
        //            var result = _db.Connection.Execute(Sql.SqlQueryCach["Brigade.Add"],
        //                new { name = brigade.Name, description = brigade.Description, brigadeType = (int)brigade.BrigadeType });
        //        }

        //        public void Delete(int id)
        //        {
        //            try
        //            { 
        //                _db.Connection.Execute(Sql.SqlQueryCach["Brigade.Delete"], new { id = id });
        //            }

        //            catch (Exception e)
        //            {
        //                throw new Exception(Other.Other.DeleteException);
        //            }
        //}

        //        public class BrigadePaging
        //        {
        //            public Brigade[] Data{ get; set; }
        //            public int Total { get; set; }
        //        }
        //    }
    
