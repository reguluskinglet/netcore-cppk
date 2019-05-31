using System;
using System.Collections.Generic;
using System.Text;
using Rzdmonitors.Util;

namespace Rzdppk.Model.Dto
{
    public class ScheduleDeviationGraphDto
    {
        public int TrainInTripCount { get; set; }

        public int TrainDepoCount { get; set; }

        public int GraphViolationCount { get; set; }

        public RouteDiffRecordDto[] Items { get; set; }
    }
}
