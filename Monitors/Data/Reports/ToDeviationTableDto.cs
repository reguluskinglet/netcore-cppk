using System;
using System.Collections.Generic;
using System.Text;

namespace RzdMonitors.Data.Reports
{
    public class ToDeviationTableDto
    {
        public int TrainInTripCount { get; set; }

        public int TrainDepoCount { get; set; }

        public int GraphViolationCount { get; set; }

        public ToDeviationTableItemDto[] Items { get; set; }
    }
}
