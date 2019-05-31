using System;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Core.Repositoryes.Sqls
{
    public class ChangePlaneBrigadeTrainsSql : ISqlQueryStorage
    {
        private const string Table = "ChangePlaneBrigadeTrains";

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

        public string Add(ChangePlaneBrigadeTrain input)
        {
            return $@"
            insert into {Table} 
            (ChangeDate, ChangeUserId, Droped, PlaneBrigadeTrainId, StantionEndId, StantionStartId, UserId) 
            values
            ('{input.ChangeDate}', '{input.ChangeUserId}', '{input.Droped}', '{input.PlaneBrigadeTrainId}', '{input.StantionEndId}', '{input.StantionStartId}', '{input.UserId}')
            SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(ChangePlaneBrigadeTrain input)
        {

            return $@"
            update {Table} set 
            ChangeDate = '{input.ChangeDate}', 
            ChangeUserId = '{input.ChangeUserId}',
            Droped = '{input.Droped}' ,
            PlaneBrigadeTrainId = {input.PlaneBrigadeTrainId} , 
            StantionEndId = {input.StantionEndId}, 
            StantionStartId = {input.StantionStartId},
            UserId = {input.UserId},
            UpdateDate = CURRENT_TIMESTAMP
            where id = {input.Id}
            ";


            //return $@"
            //    update {Table} set 
            //    ChangeDate = '{input.ChangeDate}', ChangeUserId = '{input.ChangeUserId}', CheckListType = {input.CheckListType},
            //    Droped = {input.Droped} , End = {input.End} , PlanedInspectionRouteId = {input.PlanedInspectionRouteId}, Start = {input.Start} 
            //    where id = {input.Id}
            //    ";

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

        public string ByPlaneBrigadeTrainId(int id)
        {
            return $@"
                select * from {Table} where PlaneBrigadeTrainId = {id}                
            ";
        }


    }
}