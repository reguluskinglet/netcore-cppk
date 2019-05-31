using System;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;

namespace Rzdppk.Core.Repositoryes.Sqls.PlaneBrigadeTrain
{
    public class PlaneBrigadeTrainSql : ISqlQueryStorage
    {
        private const string Table = "PlaneBrigadeTrains";

        public string Count()
        {
            return $@"select Count(*) from {Table} ";
        }

        public string Select()
        {
            return $@"select * from {Table} ";
        }

        public string GetAllPaging(int skip, int limit)
        {
            return $@"
            select * from {Table} 
            {Other.Other.SqlPagingSortByIdAsc(skip, limit)}
            ";
        }

        public string Add(Rzdppk.Model.PlaneBrigadeTrain input)
        {
            return $@"
            insert into {Table} 
            (PlanedRouteTrainId, StantionEndId, StantionStartId, UserId) 
            values
            ({input.PlanedRouteTrainId}, '{input.StantionEndId}', {input.StantionStartId} , '{input.UserId}')
            SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(Rzdppk.Model.PlaneBrigadeTrain input)
        {
            return $@"
                update {Table} set 
                PlanedRouteTrainId = '{input.PlanedRouteTrainId}', 
                StantionEndId = '{input.StantionEndId}', 
                StantionStartId = '{input.StantionStartId}', 
                UserId = {input.UserId} 
                where id = {input.Id}
            ";
        }


        public string Delete(int id)
        {
            return $@"
                delete from {Table} where id = {id}
            ";
        }

        public string ById(int id)
        {
            return $@"
                select * from {Table} where id = {id}                
            ";
        }

        public string ByPlanedRouteTrainId(int id)
        {
            return $@"
                select * from {Table} where PlanedRouteTrainId = {id}                
            ";
        }


        
    }
}