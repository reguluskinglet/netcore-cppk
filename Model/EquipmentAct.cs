using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    public class EquipmentAct : BaseEntity
    {
        public int EquipmentId { get; set; }
        public virtual Equipment Equipment { get; set; }

        public int ActCategoryId { get; set; }
        public virtual ActCategory ActCategory { get; set; }
    }
}
