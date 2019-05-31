using System;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Model
{
    public class ChangePlaneBrigadeTrain : BaseEntity
    {
        public int PlaneBrigadeTrainId { get; set; }
        public virtual PlaneBrigadeTrain PlaneBrigadeTrain { get; set; }

        public int StantionStartId { get; set; }
        public virtual PlaneStantionOnTrip StantionStart { get; set; }

        public int StantionEndId { get; set; }
        public virtual PlaneStantionOnTrip StantionEnd { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int? ChangeUserId { get; set; }
        public virtual User ChangeUser { get; set; }

        public bool Droped { get; set; }

        public DateTime ChangeDate { get; set; }
    }
}