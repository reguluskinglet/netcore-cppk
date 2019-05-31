using System;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Core.Repositoryes.Sqls
{
    public class PlanedInspectionRoutesSql : ISqlQueryStorage
    {
        private const string Table = "PlanedInspectionRoutes";

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

        public string Add(PlanedInspectionRoute input)
        {
            return $@"
            insert into {Table} 
            (CheckListType, [End], PlanedRouteTrainId, [Start]) 
            values
            ({(int)input.CheckListType}, 
            '{input.End}', 
            {input.PlanedRouteTrainId} , 
            '{input.Start}'
            )
            SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(PlanedInspectionRoute input)
        {
            return $@"
                update {Table} set 
                CheckListType = '{(int)input.CheckListType}', 
                [End] = '{input.End}', 
                PlanedRouteTrainId = '{input.PlanedRouteTrainId}', 
                Start = '{input.Start}'
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