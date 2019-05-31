using System;
using System.Collections.Generic;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Raspisanie
{
    public class PlanedInspectionRoute : BaseEntity
    {
        public int PlanedRouteTrainId { get; set; }
        public virtual PlanedRouteTrain PlanedRouteTrain { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public CheckListType? CheckListType { get; set; }

        public virtual ICollection<ChangedPlanedInspectionRoute> ChangedPlanedInspectionRoutes { get; set; }
    }
}