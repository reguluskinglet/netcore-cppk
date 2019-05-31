using System;
using System.Collections.Generic;

namespace Rzdppk.Api
{
    public class ScheludeChangedDtos
    {

        public class ChangePlaneStantionOnTripDto
        {
            public int? TrainId { get; set; }
            public DateTime StartFact { get; set; }
            public DateTime EndFact { get; set; }
            public bool Canceled { get; set; }
            public int PlaneStationOnTripId { get; set; }
        }

        public class ChangedTo2OrCtoDto
        {

            public int Id { get; set; }
            public string RouteName { get; set; }
            public string TrainName { get; set; }
            public Planned Plan { get; set; }
            public Actual Fact { get; set; }

            public class Planned
            {
                public DateTime DateStart { get; set; }
                public DateTime DateEnd { get; set; }
            }

            public class Actual
            {
                public DateTime? DateStart { get; set; }
                public DateTime? DateEnd { get; set; }
                public bool? Canseled { get; set; }
            }

        }

        public class ChangeTimeRangeUserDto
        {
            public int StartId { get; set; }
            public int EndId { get; set; }
            public int UserId { get; set; }
            public int Id { get; set; }
            public string User { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public int PlaneBrigadeTrainId { get; set; }
            public bool Canseled { get; set; }
        }

        public class DataSourceDto
        {
            public int Value { get; set; }
            public string Text { get; set; }
        }

        public class DataSource
        {
            public List<DataSourceDto> Stantions { get; set; }
            public List<DataSourceDto> Users { get; set; }
            public List<DataSourceDto> Trains { get; set; }
        }

        public class ChangeTimeRangeBrigadeDto
        {
            public List<ChangeTimeRangeUserDto> Users { get; set; }
            public DataSource DataSource { get; set; }
        }





        public class TimeRangeBrigadeUsersDto
        {
            public int PlanedBrigadeId { get; set; }
            //public int P
            // //id: number
            //startId: number //Прибытие
            //endId: number
            //userId: number
            //planeBrigadeTrainId: number

            //user: string
            //start: string
            //end: string
            //canseled: boolean
        }

        public class ChangeTripDto
        {
            public string TripName { get; set; }
            public List<TimeRangeTripStationsDto> Stantions { get; set; }
            public int PlaneStationOnTripId { get; set; }
        }

        public class TimeRangeTripStationsDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime StartPlan { get; set; }
            public DateTime EndPlan { get; set; }
            public int TrainId { get; set; }
            public DateTime? StartFact { get; set; }
            public DateTime? EndFact { get; set; }
            public bool? Canseled { get; set; }
            public int PlaneStantionOnTripId { get; set; }
        }

        public class RouteInformationTableStantion
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Train { get; set; }
            public string StartPlan { get; set; }
            public string EndPlan { get; set; }
            public int TrainId { get; set; }
            public DateTime? StartFact { get; set; }
            public DateTime? EndFact { get; set; }
            public bool? Canceled { get; set; }
            public int PlaneStationOnTripId { get; set; }
        }

        public class RouteInformationTableTrip
        {
            public string Trip { get; set; }
            public List<RouteInformationTableStantion> Stantions { get; set; }
            public DataSource DataSource { get; set; }
        }


        public class ChangePlaneBrigadeTrainDto
        {

            public int Id { get; set; }
            public int StartId { get; set; }
            public int EndId { get; set; }
            public int UserId { get; set; }
            public int PlaneBrigadeTrainId { get; set; }
            public bool Canseled { get; set; }

        }



    }
}
