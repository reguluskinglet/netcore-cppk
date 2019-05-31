using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;

namespace Rzdppk.Core.Repositoryes.Sqls.Tasks
{
    public class TripsSql : ISqlQueryStorage
    {
        private const string Table = "trips";

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

        public string Add(Trip input)
        {
            if (input.Description == null)
                input.Description = "null";
            return $@"
                insert into {Table} (Description, [Name], TripType) 
                values({input.Description}, '{input.Name}', '{(int)input.TripType}')
                SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(Trip input)
        {
            if (input.Description == null)
                input.Description = "null";
            return $@"
                update {Table} set 
                Description = '{input.Description}', 
                [Name] = '{input.Name}',
                TripType = '{(int)input.TripType}',
                UpdateDate = CURRENT_TIMESTAMP
                where id = {input.Id}
            ";
        }


        public string Delete(int id)
        {
            return $@"
                delete from {Table} where id = {id}
            ";
        }



        public string TripsByRouteId(int routeId)
        {
            return $@"
                SELECT  trips.* FROM TripOnRoutes as tripOnRoutes
                LEFT JOIN Trips as trips ON tripOnRoutes.TripId = trips.Id
                where tripOnRoutes.RouteId = {routeId}
            ";
        }

        public string ById(int id)
        {
            return $@"SELECT  * FROM {Table} where Id = {id}";
        }

        //public string Count = @"
        //select Count(*) from Turnovers
        //";



    }
}