using System;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.Raspisanie
{
    public class DepoEvent : BaseEntity
    {
        public int ParkingId { get; set; }
        public virtual Parking Parking { get; set; }

        public int? InspectionId { get; set; }
        public virtual Inspection Inspection { get; set; }

        public string InspectionTxt { get; set; }

        public int TrainId { get; set; }
        public virtual Train Train { get; set; }

        public int? RouteId { get; set; }
        public virtual Route Route { get; set; }

        public DateTime InTime { get; set; }
        public DateTime? ParkingTime { get; set; }
        public DateTime? RepairStopTime { get; set; }
        public DateTime? TestStartTime { get; set; }
        public DateTime? TestStopTime { get; set; }
        public DateTime? OutTime { get; set; }

        public int? UserId { get; set; }
        public virtual User User { get; set; }
    }
}