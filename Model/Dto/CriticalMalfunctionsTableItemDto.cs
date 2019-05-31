using System;
using System.Collections.Generic;
using System.Text;

namespace Rzdppk.Model.Dto
{
    public class CriticalMalfunctionsTableItemDto
    {
        public string TrainName { get; set; }

        public string RouteName { get; set; }

        public int CriticalCount { get; set; }

        public int TotalCount { get; set; }
    }
}
