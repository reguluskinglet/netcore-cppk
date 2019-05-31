using System;
using System.Collections.Generic;
using RzdMonitors.Util;

namespace Rzdmonitors.Util
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
