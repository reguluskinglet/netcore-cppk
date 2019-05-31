using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Grid.Filter
{
    public class FilterInfo
    {
        public FilterType FilterType { get; set; }

        public string ConditionName { get; set; }

        public dynamic Data { get; set; }
    }
}
