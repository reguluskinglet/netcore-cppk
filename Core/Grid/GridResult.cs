using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Grid
{
    public class GridResult
    {
        public LazyPagination Pager { get; set; }
        public IEnumerable<string> VisibleColumns { get; set; }
        public IEnumerable<dynamic> Rows { get; set; }
    }
}
