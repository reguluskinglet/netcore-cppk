using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.Raspisanie
{
    public class PlaneStantionOnTrip : BaseEntity
    {
        public int StantionId { get; set; }
        public Stantion Stantion { get; set; }

        public int TripId { get; set; }
        public Trip Trip { get; set; }

        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }

        public int PlanedRouteTrainId { get; set; }
        public virtual PlanedRouteTrain PlanedRouteTrain { get; set; }

        public ICollection<PlaneBrigadeTrain> StantionStarts { get; set; }
        public ICollection<PlaneBrigadeTrain> StantionEnds { get; set; }

        public ICollection<ChangePlaneBrigadeTrain> ChangePlaneBrigadeTrainStantionStarts { get; set; }
        public ICollection<ChangePlaneBrigadeTrain> ChangePlaneBrigadeTrainStantionEnds { get; set; }
    }
}
