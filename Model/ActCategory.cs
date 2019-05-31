using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    public class ActCategory:BaseEntityName
    {
        public virtual ICollection<EquipmentAct> EquipmentActs { get; set; }
    }
}
