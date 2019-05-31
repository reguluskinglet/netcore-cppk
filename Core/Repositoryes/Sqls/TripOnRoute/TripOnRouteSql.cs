using System;
using Rzdppk.Core.Services.Interfaces;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes.Sqls.TripOnRoute
{
    public class TripOnRouteSql : ISqlQueryStorage
    {
        private const string Table = "TripOnRoutes";

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
            {SqlPagingSortByIdAsc(skip, limit)}
            ";
        }

        public string Add(int routeId, int tripId)
        {
            return $@"
                insert into {Table} (routeId, TripId) 
                values({routeId}, {tripId})
                SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(Rzdppk.Model.Raspisanie.TripOnRoute input)
        {
            return $@"
                update {Table} set routeId = {input.RouteId}, tripId = {input.TripId} where id = {input.Id}
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

        public string ByRouteAndTripId(int routeId, int tripId)
        {
            return $@"
                select * from {Table} where routeId = {routeId} and tripId = {tripId}                
            ";
        }

        public string ByRouteId(int routeId)
        {
            return $@"
                select * from {Table} where routeId = {routeId};            
            ";
        }


    }
}