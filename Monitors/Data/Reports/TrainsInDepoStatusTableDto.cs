namespace RzdMonitors.Data.Reports
{
    public class TrainsInDepoStatusTableDto
    {
        public int TrainInTripCount { get; set; }

        public int TrainDepoCount { get; set; }

        public int GraphViolationCount { get; set; }

        public TrainsInDepoStatusTableItemDto[] Items { get; set; }
    }
}
