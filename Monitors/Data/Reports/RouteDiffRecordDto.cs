using System.Collections.Generic;
using RzdMonitors.Data.Reports;

namespace RzdMonitors.Data.Reports
{
    public class RouteDiffRecordDto
    {
        public string RouteName { get; set; }

        public string TrainName { get; set; }

        public string BgColor { get; set; }

        public List<TimedTaskDto> EventsPlan { get; set; }

        public List<TimedTaskDto> EventsFact { get; set; }
    }
}
