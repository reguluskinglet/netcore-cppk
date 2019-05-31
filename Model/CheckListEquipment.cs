using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    /// <summary>
    /// Привязка к Чек листам оборудование | Алгоритм проверки
    /// </summary>
    public class CheckListEquipment : BaseEntity
    {
        public CheckListType CheckListType { get; set; }

        public CheckListValueType ValueType { get; set; }

        /// <summary>
        /// Значение по дефолту
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Задача которая будет создана в случае неисправночти
        /// </summary>
        public TaskType FaultType { get; set; }

        /// <summary>
        /// Задача создается автоматически, с этим текстом
        /// </summary>
        public string NameTask { get; set; }

        public int EquipmentModelId { get; set; }
        public virtual EquipmentModel EquipmentModel { get; set; }

        public TaskLevel TaskLevel { get; set; }
    }
}
