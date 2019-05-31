using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    /// <summary>
    /// Неисправность
    /// </summary>
    public class Fault : BaseEntityName
    {
        public TaskType FaultType { get; set; }

        public virtual ICollection<FaultEquipment> FaultEquipments { get; set; }
    }
}
