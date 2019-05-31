using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.Raspisanie
{
    public class PlanedRouteTrain : BaseEntity
    {
        public int RouteId { get; set; }
        public virtual Route Route { get; set; }

        public DateTime Date { get; set; }

        public int TrainId { get; set; }
        public virtual Train Train { get; set; }

        public DateTime CreateDate { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public ICollection<PlanedInspectionRoute> PlanedInspectionRoutes { get; set; }
    }
}
