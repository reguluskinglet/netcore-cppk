using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.Raspisanie
{
    public class ChangePlaneStantionOnTrip : BaseEntity
    {
        public int PlaneStantionOnTripId { get; set; }
        public virtual PlaneStantionOnTrip PlaneStantionOnTrip { get; set; }

        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }

        public bool Droped { get; set; }

        public int? TrainId { get; set; }
        public virtual Train Train { get; set; }

        public int? ChangeUserId { get; set; }
        public virtual User ChangeUser { get; set; }
    }
}
