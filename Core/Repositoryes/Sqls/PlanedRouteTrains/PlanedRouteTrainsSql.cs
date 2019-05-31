using System;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Core.Repositoryes.Sqls.PlanedRouteTrains
{
    public class PlanedRouteTrainsSql : ISqlQueryStorage
    {
        private const string Table = "PlanedRouteTrains";

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

        public string Add(PlanedRouteTrain input)
        {
            return $@"
            insert into {Table} 
            (CreateDate, Date, RouteId, TrainId, UserId) 
            values
            ('{input.CreateDate}', '{input.Date}', '{input.RouteId}' , '{input.TrainId}', '{input.TrainId}')
            SELECT SCOPE_IDENTITY()
            ";
        }

        //public string Update(Route input)
        //{
        //    if (input.Description == null)
        //        input.Description = "null";
        //    if (input.TurnoverId == null)
        //        return $@"
        //        update {Table} set Description = '{input.Description}', [Name] = '{input.Name}', Mileage = {input.Mileage} where id = {input.Id}
        //        ";
        //    return $@"
        //        update {Table} set Description = '{input.Description}', [Name] = '{input.Name}', [TurnoverId] = '{input.TurnoverId}', Mileage = {input.Mileage} where id = {input.Id}
        //    ";
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

        public string ByUserId(int id)
        {
            return $@"
                select * from {Table} where UserId = {id}                
            ";
        }


    }
}