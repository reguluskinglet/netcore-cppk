using System;
using System.Collections.Generic;
using System.Text;

namespace Rzdppk.Model.Dto
{
    public class BrigadeScheduleDeviationTableItemDto
    {
        public string RouteName { get; set; }

        public string TrainName { get; set; }

        public string UserPlan { get; set; }

        public string UserFact { get; set; }

        public string StantionsPlan { get; set; }

        public string StantionsFact { get; set; }

        public string BgColor { get; set; }

        public string FgColor { get; set; }
    }
}
