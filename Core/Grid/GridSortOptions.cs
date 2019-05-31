using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.QueryBuilders;

namespace Core.Grid
{
    public class GridSortOptions
    {
        public string Column { get; set; }
        public SortDirection Direction { get; set; }
    }
}
