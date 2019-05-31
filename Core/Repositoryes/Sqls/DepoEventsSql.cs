using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes.Sqls
{
    public class DepoEventsSql : ISqlQueryStorage
    {
        private readonly string _table = "Carriages";

        private DepoEventsSql()
        {
        }

        public DepoEventsSql(string table)
        {
            _table = table;
        }


        public string GetAllPaging(int skip, int limit)
        {
            return $@"
            select * from {_table} 
            {SqlPagingSortByIdAsc(skip, limit)}
            ";
        }

        public string Count()
        {
            return $@"select Count(*) from {_table} ";
        }

        //public string Add(TrainTask input)
        //{
        //    return $@"
        //        insert into {_table} 
        //        ([Day], [TurnoverId]) 
        //        values
        //        ('{(int)input.Day}', '{input.TurnoverId}')
        //        SELECT SCOPE_IDENTITY()
        //    ";
        //}

        //public string Update(TrainTask input)
        //{
        //    return $@"
        //        update {_table} set 
        //        Day = '{(int)input.Day}', 
        //        TurnoverId = '{input.TurnoverId}', 
        //        where id = {input.Id}
        //    ";
        //}


        public string Delete(int id)
        {
            return $@"
                delete from {_table} where id = {id}
            ";
        }

        public string Select()
        {
            return $@"select * from {_table} ";
        }

        public string GetAll()
        {
            return $@"select * from {_table} ";
        }

        public string ById(int id)
        {
            return $@"
                select * from {_table} where id = {id}                
            ";
        }

    }
}