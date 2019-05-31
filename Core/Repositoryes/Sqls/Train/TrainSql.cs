using System;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;

namespace Rzdppk.Core.Repositoryes.Sqls.Train
{
    public class TrainSql : ISqlQueryStorage
    {
        private const string Table = "Trains";

        public string Count()
        {
            return $@"select Count(*) from {Table} ";
        }

        public string Select()
        {
            return $@"select * from {Table} ";
        }

        public string All()
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

        //public string Add(Route input)
        //{
        //    if (input.Description == null)
        //        input.Description = "null";
        //    if (input.TurnoverId == null)
        //        return $@"
        //        insert into {Table} (Description, [Name], [Mileage]) 
        //        values({input.Description}, '{input.Name}', '{input.Mileage}')
        //        SELECT SCOPE_IDENTITY()
        //        ";
        //    return $@"
        //    insert into {Table} (Description, [Name], TurnoverId, [Mileage]) 
        //    values({input.Description}, '{input.Name}', {input.TurnoverId} , '{input.Mileage}')
        //    SELECT SCOPE_IDENTITY()
        //    ";
        //}

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

        public string ById(int? id)
        {
            return $@"
                select * from {Table} where id = {id}                
            ";
        }
        public string ByPlaneStationId(int id)
        {
            return $@"
                select prt.TrainId from PlaneStantionOnTrips as pst
                left join PlanedRouteTrains as prt ON pst.PlanedRouteTrainId = prt.Id
                where pst.Id = {id}
            ";
        }



    }
}