using RzdMonitors.Data.Reports;

namespace RzdPpk.Data.Reports
{
    public class ScheduleDeviationTableDto
    {
        public int TrainInTripCount { get; set; }

        public int TrainDepoCount { get; set; }

        public int GraphViolationCount { get; set; }

        public ScheduleDeviationTableItemDto[] Items { get; set; }
    }
}
