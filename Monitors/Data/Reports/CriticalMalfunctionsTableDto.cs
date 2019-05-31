namespace RzdMonitors.Data.Reports
{
    public class CriticalMalfunctionsTableDto
    {
        public int TrainInTripCount { get; set; }

        public int TrainDepoCount { get; set; }

        public int GraphViolationCount { get; set; }

        public CriticalMalfunctionsTableItemDto[] Items { get; set; }
    }
}
