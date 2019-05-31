using System;
using System.Collections.Generic;
using System.Text;

namespace RzdMonitors.Data.Reports
{
    public class ToDeviationTableItemDto
    {
        public string RouteName { get; set; }

        public string TrainName { get; set; }

        public string Type { get; set; }

        public string PlanTime { get; set; }

        public string FactTime { get; set; }

        public string BgColor { get; set; }

        public string FgColor { get; set; }
    }
}
