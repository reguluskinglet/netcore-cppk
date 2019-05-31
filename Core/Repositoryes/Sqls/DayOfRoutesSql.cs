using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes.Sqls
{
    public class DayOfRoutesSql : ISqlQueryStorage
    {
        private const string Table = "DayOfRoutes";

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

        public string StationsOnTripByTripId(int tripId)
        {
            return $@"
                SELECT  * FROM {Table}
                where TripId = {tripId}
            ";
        }

        public string Add(DayOfRoute input)
        {
            return $@"
                insert into {Table} 
                ([Day], [TurnoverId]) 
                values
                ('{(int)input.Day}', '{input.TurnoverId}')
                SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(DayOfRoute input)
        {
            return $@"
                update {Table} set 
                Day = '{(int)input.Day}', 
                TurnoverId = '{input.TurnoverId}', 
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

        public string DaysByTurnoverId(int id)
        {
            return $@"
                select * from {Table} where TurnoverId = {id}                
            ";
        }




    }
}