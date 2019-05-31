using System;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Core.Repositoryes.Sqls
{
    public class EquipmentsSql : ISqlQueryStorage
    {
        private const string Table = "Equipments";

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

        //public string Add(EquipmentModel input)
        //{
        //    var nullableParentId = "null";
        //    if (input.ParentId != null)
        //        nullableParentId = Convert.ToInt32(input.ParentId).ToString();

        //    var nullableIsMark = "null";
        //    if (input.IsMark != null)
        //        nullableIsMark = Convert.ToInt32(input.IsMark).ToString();

        //    return $@"
        //    insert into {Table} 
        //    (EquipmentId, ModelId, ParentId, IsMark) 
        //    values
        //    ('{input.EquipmentId}', '{input.ModelId}', {nullableParentId}, {nullableIsMark})
        //    SELECT SCOPE_IDENTITY()
        //    ";
        //}

        //public string Update(EquipmentModel input)
        //{
        //    var nullableParentId = "null";
        //    if (input.ParentId != null)
        //        nullableParentId = Convert.ToInt32(input.ParentId).ToString();
        //    var nullableIsMark = "null";
        //    if (input.IsMark != null)
        //        nullableIsMark = Convert.ToInt32(input.IsMark).ToString();

        //    return $@"
        //    update {Table} set 
        //    EquipmentId = '{input.EquipmentId}', 
        //    ModelId = '{input.ModelId}',
        //    ParentId = {nullableParentId} ,
        //    IsMark = {nullableIsMark} 
        //    where id = {input.Id}
        //    ";


        //    //return $@"
        //    //    update {Table} set 
        //    //    ChangeDate = '{input.ChangeDate}', ChangeUserId = '{input.ChangeUserId}', CheckListType = {input.CheckListType},
        //    //    Droped = {input.Droped} , End = {input.End} , PlanedInspectionRouteId = {input.PlanedInspectionRouteId}, Start = {input.Start} 
        //    //    where id = {input.Id}
        //    //    ";

        //}


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
    }
}