using System;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes.Sqls.StantionOnTrips
{
    public class StantionOnTripsSql : ISqlQueryStorage
    {
        private const string Table = "StantionOnTrips";

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
            return  $@"
                SELECT  * FROM StantionOnTrips
                where TripId = {tripId}
            ";
        }

        public string Add(StantionOnTrip input)
        {
            return $@"
                insert into {Table} ([InTime], [OutTime], [StantionId], [TripId]) values('{input.InTime}', '{input.OutTime}', {input.StantionId}, {input.TripId})
                SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(StantionOnTrip input)
        {
            return $@"
                update {Table} set InTime = '{input.InTime}', OutTime = '{input.OutTime}', StantionId = {input.StantionId}, TripId = {input.TripId} where id = {input.Id}
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


    }
}