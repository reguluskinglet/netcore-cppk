using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    public class Device : BaseEntity
    {
        public string Name { get; set; }

        /// <summary>
        /// Заводской номер (на android получаем номер программно)
        /// </summary>
        public string Serial { get; set; }

        public virtual ICollection<DeviceValue> DeviceValues { get; set; }

        public int CellNumber { get; set; }
    }
}
