using System.Collections.Generic;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Model
{
    public class PlaneBrigadeTrain:BaseEntity
    {
        public int StantionStartId { get; set; }
        public virtual PlaneStantionOnTrip StantionStart { get; set; }

        public int StantionEndId { get; set; }
        public virtual PlaneStantionOnTrip StantionEnd { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int PlanedRouteTrainId { get; set; }
        public virtual PlanedRouteTrain PlanedRouteTrain { get; set; }

        public virtual ICollection<ChangePlaneBrigadeTrain> ChangePlaneBrigadeTrains { get; set; }
    }
}