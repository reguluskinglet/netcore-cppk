using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Grid;

namespace Invent.Core.GridModels.Base
{
    public class NamedGridOptions : GridOptions, IFilterOptions<NameFilter>
    {
        public NameFilter Filter { get; set; }

        public NamedGridOptions()
        {
            Filter = new NameFilter();
        }
    }
}
