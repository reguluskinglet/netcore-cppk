using System;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Services;

namespace Rzdppk.Controllers.DepoEvents.UI
{
    public class DepoEventDtoUi
    {
        public int? Id { get; set; }

        public int ParkingId { get; set; }

        public int? InspectionId { get; set; }

        public int? UserId { get; set; }

        public string InspectionTxt { get; set; }

        public int TrainId { get; set; }

        public int? RouteId { get; set; }

        public DateTime InTime { get; set; }

        public DateTime? OutTime { get; set; }

        public DateTime? ParkingTime { get; set; }

        public DateTime? RepairStopTime { get; set; }

        public DateTime? TestStartTime { get; set; }

        public DateTime? TestStopTime { get; set; }

        public DepoEventDataSource DepoEventDataSource { get; set; }
    }
}