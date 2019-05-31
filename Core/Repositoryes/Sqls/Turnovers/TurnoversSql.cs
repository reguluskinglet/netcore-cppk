using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rzdppk.Core.Services.Interfaces;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes.Sqls.Turnovers
{
    public class TurnoversSql : ISqlQueryStorage
    {
        private const string Table = "Turnovers";

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

        public string Add(int directionId, string name)
        {
            return $@"
                insert into {Table} (Name, DirectionId) values('{name}', {directionId})
                SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(int directionId, string name, int id)
        {
            return $@"
                update {Table} set Name = '{name}', DirectionId = {directionId} where id = {id}
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

        public string Select()
        {
            return $@"select * from {Table} ";
        }



    }
}