using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    public class FaultEquipment : BaseEntity
    {
        public int EquipmentId { get; set; }
        public virtual Equipment Equipment { get; set; }

        public int FaultId { get; set; }
        public virtual Fault Fault { get; set; }

        public TaskLevel TaskLevel { get; set; }
    }
}
