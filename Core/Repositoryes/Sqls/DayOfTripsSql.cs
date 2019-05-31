using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes.Sqls
{
    public class DayOfTripsSql : ISqlQueryStorage
    {
        private const string Table = "DayOfTrips";

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

        public string Add(DayOfTrip input)
        {
            return $@"
                insert into {Table} 
                ([Day], [TripId]) 
                values
                ('{(int)input.Day}', '{input.TripId}')
                SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(DayOfTrip input)
        {
            return $@"
                update {Table} set 
                Day = '{(int)input.Day}', 
                TurnoverId = '{input.TripId}', 
                where id = {input.Id}
            ";
        }


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

        public string ById(int id)
        {
            return $@"
                select * from {Table} where id = {id}                
            ";
        }

        public string DaysByTripId(int id)
        {
            return $@"
                select * from {Table} where TripId = {id}                
            ";
        }




    }
}