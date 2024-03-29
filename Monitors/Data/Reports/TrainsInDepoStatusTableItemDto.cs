﻿namespace RzdMonitors.Data.Reports
{
    public class TrainsInDepoStatusTableItemDto
    {
        public string TrainName { get; set; }

        public string ParkingName { get; set; }

        public string InspectionName { get; set; }

        public string InTime { get; set; }

        public int OpenTasksCountAtInTime { get; set; }

        public int OpenTasksCountNow { get; set; }
    }
}
