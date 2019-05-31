using System;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Core.Repositoryes.Sqls
{
    public class CheckListEquipmentsSql : ISqlQueryStorage
    {
        private const string Table = "CheckListEquipments";

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

        public string Add(CheckListEquipment input)
        {

            return $@"
            insert into {Table} 
            (CheckListType, EquipmentModelId, FaultType, NameTask, Value, ValueType, TaskLevel) 
            values
            ('{(int)input.CheckListType}', '{input.EquipmentModelId}', '{(int)input.FaultType}', '{input.NameTask}', '{input.Value}', '{(int)input.ValueType}', '{input.TaskLevel}')
            SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(CheckListEquipment input)
        {

            return $@"
            update {Table} set 
            CheckListType = '{input.CheckListType}', 
            EquipmentModelId = '{input.EquipmentModelId}',
            FaultType = {input.FaultType} ,
            NameTask = {input.NameTask} , 
            Value = {input.Value}, 
            ValueType = {input.ValueType},
            TaskLevel = {input.TaskLevel}
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

        public string ByEquipmentModelId(int id)
        {
            return $@"
                select * from {Table} where EquipmentModelId = {id}                
            ";
        }

    }
}