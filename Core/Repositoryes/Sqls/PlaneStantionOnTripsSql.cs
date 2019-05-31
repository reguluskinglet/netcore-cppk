using System;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes.Sqls
{
    public class PlaneStantionOnTripsSql : ISqlQueryStorage
    {
        private const string Table = "PlaneStantionOnTrips";

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

        public string Add(PlaneStantionOnTrip input)
        {
            return $@"
                insert into {Table} 
                ([InTime], [OutTime], [StantionId], [TripId], PlanedRouteTrainId) 
                values
                ('{input.InTime}', '{input.OutTime}', {input.StantionId}, {input.TripId}, {input.PlanedRouteTrainId})
                SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(PlaneStantionOnTrip input)
        {
            return $@"
                update {Table} set 
                InTime = '{input.InTime}', 
                OutTime = '{input.OutTime}', 
                StantionId = {input.StantionId}, 
                TripId = {input.TripId},
                PlanedRouteTrainId = {input.PlanedRouteTrainId}
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

        public string ByPlanedRouteTrainId(int id)
        {
            return $@"
                select * from {Table} where PlanedRouteTrainId = {id}                
            ";
        }

        public string ByUserIdAndTimeRange(int id, DateTime startTime, DateTime endTime)
        {
            return $@"
                select pbt.* from PlaneBrigadeTrains  as pbt
                left join PlanedRouteTrains as prt ON pbt.PlanedRouteTrainId = prt.id
                left join {Table} as psot ON psot.PlanedRouteTrainId =  prt.id
                where pbt.UserId = {id} AND psot.InTime >= '{startTime}' AND psot.InTime <= '{endTime}'      
            ";
        }

        public string ByPlanedRouteTrainIdAndStationId(int planedRouteTrainId, int stationId)
        {
            return $@"
                select * from {Table} where PlanedRouteTrainId = {planedRouteTrainId} and  StantionId = stantionId;                
            ";
        }




    }
}