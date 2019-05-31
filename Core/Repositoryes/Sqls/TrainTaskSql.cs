using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes.Sqls
{
    public class TrainTaskSql : ISqlQueryStorage
    {
        private const string Table = "TrainTasks";

        public string GetAllPaging(int skip, int limit)
        {
            return $@"
            select * from {Table} 
            {SqlPagingSortByIdAsc(skip, limit)}
            ";
        }

        public string Count()
        {
            return $@"select Count(*) from {Table} ";
        }

        //public string Add(TrainTask input)
        //{
        //    return $@"
        //        insert into {Table} 
        //        ([Day], [TurnoverId]) 
        //        values
        //        ('{(int)input.Day}', '{input.TurnoverId}')
        //        SELECT SCOPE_IDENTITY()
        //    ";
        //}

        //public string Update(TrainTask input)
        //{
        //    return $@"
        //        update {Table} set 
        //        Day = '{(int)input.Day}', 
        //        TurnoverId = '{input.TurnoverId}', 
        //        where id = {input.Id}
        //    ";
        //}


        public string Delete(int id)
        {
            return $@"
                delete from {Table} where id = {id}
            ";
        }

        public string Select()
        {
            return $@"select * from {Table} ";
        }

        public string GetAll()
        {
            return $@"select * from {Table} ";
        }

        public string UpdateEditDate(int taskId)
        {
            return $@"update TrainTasks set EditDate = GETDATE() where id = {taskId}";
        }

        

        public string ById(int id)
        {
            return $@"
                select * from {Table} where id = {id}                
            ";
        }

        public string CriticalTasksByTrainIdAndDate(int id, DateTime date)
        {
            return $@"
                select trainTasks.*,trainTaskAttributes.UpdateDate as AttribUpdateDate  from TrainTasks as trainTasks
                left join Carriages as carriages ON trainTasks.CarriageId = carriages.Id
                left join trains as trains ON trains.Id = carriages.TrainId
                left join TrainTaskAttributes as trainTaskAttributes ON trainTaskAttributes.TrainTaskId = trainTasks.Id
                where TrainTaskAttributes.TaskLevel = 3 and trainTaskAttributes.UpdateDate > '{date}' and trainTaskAttributes.UpdateDate < '{date.AddHours(23).AddMinutes(59).AddSeconds(59)}'
                and trains.Id = {id}          
            ";
        }

        public string ByTrainId(int id)
        {
            return $@"
                select * from TrainTasks as tt
                left join carriages as c ON tt.CarriageId = c.Id
                where c.TrainId = {id}
            ";

        }

    }
}