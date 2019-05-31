using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public class BrigadeTypeRepository : IBrigadeTypeRepository, IDisposable
    {
        private readonly IDb _db;
        
        public BrigadeTypeRepository()
        {
            _db = new Db();
        }


        //public async Task<BrigadeTypePaging> GetAll(int skip, int limit)
        //{
        //    var sql = Sql.SqlQueryCach["BrigadeType.All"];
        //    var result = await _db.Connection.QueryAsync<BrigadeType>(sql, new { skip = skip, limit = limit });
        //    var sqlc = Sql.SqlQueryCach["BrigadeType.CountAll"];
        //    var count = _db.Connection.ExecuteScalar<int>(sqlc);
        //    var output = new BrigadeTypePaging
        //    {
        //        Data = result.ToArray(),
        //        Total = count
        //    };

        //    return output;
        //}

        //public BrigadeType ByIdWithStations(int id)
        //{
        //    var sql = Sql.SqlQueryCach["BrigadeType.ById"];
        //    var result = _db.Connection.Query<BrigadeType>(sql, new { brigadeTypes_id = id });
        //    return result.FirstOrDefault();
        //}

        //public void Add(EquipmentCategory equipmentCategory)
        //{
        //    _db.Connection.Execute(Sql.SqlQueryCach["CategoryEquipment.Add"],new {name = equipmentCategory.Name, description = equipmentCategory.Description});
        //}


        //public void Delete(int id)
        //{

        //    _db.Connection.Execute(Sql.SqlQueryCach["CategoryEquipment.Delete"],new { id = id});
        //}

        //public class BrigadeTypePaging
        //{
        //    public BrigadeType[] Data { get;set;}
        //    public int Total { get; set;}
        //}

        public void Dispose()
        {
            _db.Connection.Close();
        }
    }
}
