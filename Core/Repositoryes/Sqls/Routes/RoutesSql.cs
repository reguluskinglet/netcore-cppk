using System;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;

namespace Rzdppk.Core.Repositoryes.Sqls.Routes
{
    public class RoutesSql : ISqlQueryStorage
    {
        private const string Table = "Routes";

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

        public string Add(Route input)
        {
            if (input.Description == null)
                input.Description = "null";
            if (input.TurnoverId == null)
                return $@"
                insert into {Table} (Description, [Name], [Mileage]) 
                values({input.Description}, '{input.Name}', '{input.Mileage}')
                SELECT SCOPE_IDENTITY()
                ";
            return $@"
            insert into {Table} (Description, [Name], TurnoverId, [Mileage]) 
            values({input.Description}, '{input.Name}', {input.TurnoverId} , '{input.Mileage}')
            SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(Route input)
        {
            if (input.Description == null)
                input.Description = "null";
            if (input.TurnoverId == null)
                return $@"
                update {Table} set Description = '{input.Description}', [Name] = '{input.Name}', Mileage = {input.Mileage} where id = {input.Id}
                ";
            return $@"
                update {Table} set Description = '{input.Description}', [Name] = '{input.Name}', [TurnoverId] = '{input.TurnoverId}', Mileage = {input.Mileage} where id = {input.Id}
            ";
        }


        public string Delete(int id)
        {
            return $@"
                delete from {Table} where id = {id}
            ";
        }

        public string ClearTurnoverId(int id)
        {
            return $@"
                update {Table} set TurnoverId = null where id = {id}
            ";
        }

        public string ById(int id)
        {
            return $@"
                select * from {Table} where id = {id}                
            ";
        }

        public string WithoutTurnover()
        {
            return $@"
                select * from {Table} where TurnoverId IS NULL
            ";
        }

        public string GetByTurnoverIdPaging(int turnoverId, int skip, int limit)
        {
            return $@"
            select * from Routes
            where TurnoverId = {turnoverId}
            ORDER BY id
            OFFSET {skip} ROWS
            FETCH NEXT {limit} ROWS ONLY
            ";
        }

        public string ByTurnoverId(int turnoverId)
        {
            return $@"
            select * from Routes
            where TurnoverId = {turnoverId}
            ";
        }

        public string CountByTurnoverId(int turnoverId)
        {
            return $@"
            select Count(*) from Routes
            where TurnoverId = {turnoverId}
        ";
        }



    }
}