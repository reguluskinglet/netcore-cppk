using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes.Sqls
{
    public class InspectionSql : ISqlQueryStorage
    {
        private readonly string _table = "Carriages";

        public InspectionSql(string table)
        {
            _table = table;
        }


        
        public string ByIdForEventTable(int id)
        {
            return $@"
                select 

                insp.id as InspectionId,
                insp.CheckListType as TypeInt,
                trains.Name as TrainName,
                insp.Status as StatusInt,
                users.Name as Author,
                br.BrigadeType as BrigadeTypeInt,
                insp.DateStart,
                insp.DateEnd,
                sig.CaptionImage as Signature
                 
                from {_table} as insp

                LEFT JOIN trains AS trains ON trains.Id = insp.TrainId
                LEFT JOIN auth_users AS users ON users.Id = insp.UserId
                LEFT JOIN Brigades AS br ON br.Id = users.BrigadeId
                LEFT JOIN Signatures AS sig ON sig.InspectionId = insp.Id

                where insp.id = {id}
            ";
        }


        public string Count()
        {
            return $@"select Count(*) from {_table} ";
        }

        public string GetAllPaging(int skip, int limit)
        {
            return $@"
            select * from {_table} 
            {Other.Other.SqlPagingSortByIdAsc(skip, limit)}
            ";
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