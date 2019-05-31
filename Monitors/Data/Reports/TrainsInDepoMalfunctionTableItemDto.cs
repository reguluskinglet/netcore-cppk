namespace RzdMonitors.Data.Reports
{
    public class TrainsInDepoMalfunctionTableItemDto
    {
        public string TrainName { get; set; }

        public string DepoInTime { get; set; }

        public int TotalCount { get; set; }

        public int CriticalCount { get; set; }
    }
}
