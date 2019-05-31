using System;
using System.Collections.Generic;
using System.Text;

namespace Rzdppk.Model.Dto
{
    public class ScheduleDeviationTableDto
    {
        public int TrainInTripCount { get; set; }

        public int TrainDepoCount { get; set; }

        public int GraphViolationCount { get; set; }

        public ScheduleDeviationTableItemDto[] Items { get; set; }
    }
}
