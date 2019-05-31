using System;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Core.Repositoryes.Sqls
{
    public class ChangedPlanedInspectionRoutesSql : ISqlQueryStorage
    {
        private const string Table = "ChangedPlanedInspectionRoutes";

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


        public string GetAll()
        {
            return $@"
                select * from {Table}
            ";
        }

        public string Add(ChangedPlanedInspectionRoute input)
        {
            return $@"
            insert into {Table} 
            (ChangeDate, ChangeUserId, CheckListType, Droped, [End], PlanedInspectionRouteId, Start) 
            values
            ('{input.ChangeDate}', '{input.ChangeUserId}', '{(int)input.CheckListType}' , '{input.Droped}', '{
                    input.End
                }', '{input.PlanedInspectionRouteId}', '{input.Start}')
            SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(ChangedPlanedInspectionRoute input)
        {
            if (input.CheckListType == null)
                return $@"
                update {Table} set 
                ChangeDate = '{input.ChangeDate}', ChangeUserId = '{input.ChangeUserId}',
                Droped = {input.Droped} , End = {input.End} , PlanedInspectionRouteId = {input.PlanedInspectionRouteId}, Start = {input.Start}, UpdateDate = CURRENT_TIMESTAMP 
                where id = {input.Id}
                ";


            return $@"
                update {Table} set 
                ChangeDate = '{input.ChangeDate}', ChangeUserId = '{input.ChangeUserId}', CheckListType = {(int)input.CheckListType},
                Droped = '{input.Droped}' , [End] = '{input.End}' , PlanedInspectionRouteId = {input.PlanedInspectionRouteId}, Start = '{input.Start}' 
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

        public string ByPlanedInspectionRouteId(int id)
        {
            return $@"
                select * from {Table} where PlanedInspectionRouteId = {id}                
            ";
        }

        
    }
}