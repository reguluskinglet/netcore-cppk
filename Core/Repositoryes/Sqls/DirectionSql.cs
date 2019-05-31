using System;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;

namespace Rzdppk.Core.Repositoryes.Sqls.PlaneBrigadeTrain
{
    public class DirectionSql : ISqlQueryStorage
    {
        private const string Table = "Directions";

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

        public string Add(Rzdppk.Model.Raspisanie.Direction input)
        {
            return $@"
            insert into {Table} 
            (Name) 
            values
            ('{input.Name}')
            SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(Rzdppk.Model.Raspisanie.Direction input)
        {
            return $@"
                update {Table} set 
                Name = '{input.Name}'
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