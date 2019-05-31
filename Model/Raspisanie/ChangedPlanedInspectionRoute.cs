using System;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Raspisanie
{
    public class ChangedPlanedInspectionRoute : BaseEntity
    {
        public int PlanedInspectionRouteId { get; set; }
        public virtual PlanedInspectionRoute PlanedInspectionRoute { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public CheckListType? CheckListType { get; set; }

        public bool Droped { get; set; }

        public DateTime ChangeDate { get; set; }

        public int ChangeUserId { get; set; }
        public User ChangeUser { get; set; }
    }
}