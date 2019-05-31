using System;
using Microsoft.EntityFrameworkCore.Internal;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;

namespace Rzdppk.Core.Repositoryes.Sqls.Primitive
{
    public class PrimitiveSql<T> : ISqlQueryStorage
    {
        private readonly string _table;

        public PrimitiveSql()
        {
            _table = typeof(T).ShortDisplayName()+ "s";
            var properties = typeof(T).GetProperties();
            var type = typeof(T);
        }
        
        public string Count()
        {
            return $@"select Count(*) from {_table} ";
        }

        public string Select()
        {
            return $@"select * from {_table} ";
        }

        public string GetAllPaging(int skip, int limit)
        {
            return $@"
            select * from {_table} 
            {Other.Other.SqlPagingSortByIdAsc(skip, limit)}
            ";
        }

        //public string Add(<T> input)
        //{
        //    return $@"
        //    insert into {Table} 
        //    (PlanedRouteTrainId, StantionEndId, StantionStartId, UserId) 
        //    values
        //    ({input.PlanedRouteTrainId}, '{input.StantionEndId}', {input.StantionStartId} , '{input.UserId}')
        //    SELECT SCOPE_IDENTITY()
        //    ";
        //}

        //public string Update(Rzdppk.Model.PlaneBrigadeTrain input)
        //{
        //    return $@"
        //        update {Table} set 
        //        PlanedRouteTrainId = '{input.PlanedRouteTrainId}', 
        //        StantionEndId = '{input.StantionEndId}', 
        //        StantionStartId = '{input.StantionStartId}', 
        //        UserId = {input.UserId} 
        //        where id = {input.Id}
        //    ";
        //}


        public string Delete(int id)
        {
            return $@"
                delete from {_table} where id = {id}
            ";
        }

        public string ById(int id)
        {
            return $@"
                select * from {_table} where id = {id}                
            ";
        }

    }
}