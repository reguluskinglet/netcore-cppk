using System;
using System.Collections.Generic;
using System.Text;

namespace RzdMonitors.Data.Reports
{
    public class BrigadeScheduleDeviationTableDto
    {
        public int TrainInTripCount { get; set; }

        public int TrainDepoCount { get; set; }

        public int GraphViolationCount { get; set; }

        public BrigadeScheduleDeviationTableItemDto[] Items { get; set; }
    }
}
