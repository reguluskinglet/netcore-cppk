using System;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;

namespace Rzdppk.Core.Repositoryes.Sqls.PlaneBrigadeTrain
{
    public class ParkingSql : ISqlQueryStorage
    {
        private const string Table = "Parkings";

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

        public string Add(Rzdppk.Model.Raspisanie.Parking input)
        {
            return $@"
            insert into {Table} 
            (Name, Description, StantionId) 
            values
            ('{input.Name}', '{input.Description}', '{input.StantionId}')
            SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(Rzdppk.Model.Raspisanie.Parking input)
        {
            return $@"
                update {Table} set 
                Name = '{input.Name}',
                Description = '{input.Description}',
                StantionId = '{input.StantionId}'
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

       
    }
}