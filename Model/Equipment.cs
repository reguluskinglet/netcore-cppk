using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    /// <summary>
    /// Оборудование
    /// </summary>
    public class Equipment : BaseEntityName
    {
        public int CategoryId { get; set; }

        public virtual EquipmentCategory Category { get; set; }

        public virtual ICollection<EquipmentModel> EquipmentModels { get; set; }

        public virtual ICollection<FaultEquipment> FaultEquipments { get; set; }

        public virtual ICollection<EquipmentAct> EquipmentActs { get; set; }

    }
}
