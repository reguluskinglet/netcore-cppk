using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    /// <summary>
    /// Модуль поезда
    /// </summary>
    public class Model : BaseEntityName
    {
        /// <summary>
        /// Тип модели
        /// </summary>
        public ModelType ModelType { get; set; }

        public ICollection<EquipmentModel> EquipmentModels { get; set; }
    }
}
