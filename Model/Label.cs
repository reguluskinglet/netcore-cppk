using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    /// <summary>
    /// Метка
    /// </summary>
    public class Label : BaseEntity
    {
        /// <summary>
        /// ИД метки
        /// </summary>
        public string Rfid { get; set; }

        /// <summary>
        /// Тип метки
        /// </summary>
        public LabelType LabelType { get; set; }

        public int CarriageId { get; set; }

        public virtual Carriage Carriage { get; set; }

        public int? EquipmentModelId { get; set; } //Если необходимо удалять все метки после удаления оборудования указать в конфигурации ON DELETE NO ACTION

        public virtual EquipmentModel EquipmentModel { get; set; }

        public ICollection<Meterage> Meterages { get; set; }
    }
}
