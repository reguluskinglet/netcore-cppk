using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Grid;

namespace Invent.Core.GridModels.Base
{
    public class NameGridFilter : Filter<NameFilter>
    {
        public override void Apply()
        {
            AddCondition(x => x.Name, ConditionType.LikeAny, "Name");
        }

        public override Filter<NameFilter> Configure()
        {
            AddTextFilter(x => x.Name);

            return this;
        }
    }
}
