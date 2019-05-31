using Rzdppk.Core.Other;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes.Sqls
{
    public static class CommonSql 
    {


        public static string GetAllPaging(string table, DevExtremeTableData.Paging paging)
        {
            return $@"
            select * from {table} 
            {SqlPagingSortByIdAsc(int.Parse(paging.Skip), int.Parse(paging.Limit))}
            ";
        }

        public static string GetAllPaging(string table, int skip, int limit)
        {
            return $@"
            select * from {table} 
            {SqlPagingSortByIdAsc(skip, limit)}
            ";
        }

        public static string Select(string table)
        {
            return $@"select * from {table} ";
        }

        public static string Count(string table)
        {
            return $@"select Count(*) from {table} ";
        }


        public static string Delete(string table, int id)
        {
            return $@"
                delete from {table} where id = {id}
            ";
        }

        public static string GetAll(string table)
        {
            return $@"select * from {table} ";
        }

        public static string GetAllSortByProperty(string table, string propertyName, string direction)
        {
            return $@"select * from {table} ORDER BY {propertyName} {direction}";
        }

        public static string ById(string table, int id)
        {
            return $@"
                select * from {table} where id = {id}                
            ";
        }

        public static string ByPropertyId(string table, string propertyName, int id)
        {
            return $@"
                select * from {table} where {propertyName} = {id}                
            ";
        }

    }
}