using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    /// <summary>
    /// Категория оборудования
    /// </summary>
    public class EquipmentCategory :BaseEntityName
    {
        public virtual ICollection<Equipment> Equipments { get; set; }
    }
}
