using System;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Core.Repositoryes.Sqls
{
    public class TrainTaskAttributeSql : ISqlQueryStorage
    {
        private const string Table = "TrainTaskAttributes";

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

//        public string Add(TrainTaskAttribute input)
//        {
//            return $@"
//            insert into {Table} 
//            (ChangeUserId, 
//            Droped, 
//            InTime, 
//            OutTime, 
//            PlaneStantionOnTripId,
//            TrainId
//            ) 
//            values
//            ('{input.ChangeUserId}',
//            '{input.Droped}', 
//            '{input.InTime}', 
//            '{input.OutTime}', 
//            '{input.PlaneStantionOnTripId}', 
//            '{input.TrainId}')
//            SELECT SCOPE_IDENTITY()
//            ";
//        }

//        public string Update(TrainTaskAttribute input)
//        {
////            if (input.CheckListType == null)
//                return $@"
//                update {Table} set 
//                ChangeUserId = '{input.ChangeUserId}',
//                Droped = '{input.Droped}',
//                InTime = {input.InTime} , 
//                OutTime = {input.OutTime} , 
//                PlaneStantionOnTripId = {input.PlaneStantionOnTripId}, 
//                TrainId = {input.TrainId} 
//                where id = {input.Id}
//                ";


//            //return $@"
//            //    update {Table} set 
//            //    ChangeDate = '{input.ChangeDate}', ChangeUserId = '{input.ChangeUserId}', CheckListType = {input.CheckListType},
//            //    Droped = {input.Droped} , End = {input.End} , PlanedInspectionRouteId = {input.PlanedInspectionRouteId}, Start = {input.Start} 
//            //    where id = {input.Id}
//            //    ";

//        }


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

        public string ByInspectionId(int id)
        {
            return $@"
                select * from {Table} where InspectionId = {id}                
            ";
        }

        public string ByTaskId(int id)
        {
            return $@"
                select * from {Table} where TrainTaskId = {id}                
            ";
        }

    }
}