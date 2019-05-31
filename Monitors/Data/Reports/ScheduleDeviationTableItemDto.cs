namespace RzdMonitors.Data.Reports
{
    public class ScheduleDeviationTableItemDto
    {
        public string TrainName { get; set; }

        public string TripNumber { get; set; }

        public string StationName { get; set; }

        //public string ArriveTimePlan { get; set; }

        //public string DepartureTimePlan { get; set; }

        //public string ArriveDeviation { get; set; }

        //public string DepartureDeviation { get; set; }

        public string PlanTime { get; set; }

        public string FactTime { get; set; }

        public string StatusColor { get; set; }
    }
}
