using System;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Terminal
{
    public class TrainTaskAttributeTerminal : BaseEntityTerminal
    {
        public Guid TrainTaskId { get; set; }
        public TrainTaskTerminal TrainTask { get; set; }

        public Guid? InspectionId { get; set; }
        public InspectionTerminal Inspection { get; set; }

        public int? CheckListEquipmentId { get; set; }
        public CheckListEquipment CheckListEquipment { get; set; }

        public int? FaultId { get; set; }
        public Fault Fault { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int? Value { get; set; }

        public TaskLevel? TaskLevel { get; set; }

        public string Description { get; set; }
    }
}