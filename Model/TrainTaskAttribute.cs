using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    public class TrainTaskAttribute : BaseEntityRef
    {
        public int TrainTaskId { get; set; }
        public TrainTask TrainTask { get; set; }

        public int? InspectionId { get; set; }
        public Inspection Inspection { get; set; }

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
