using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    /// <summary>
    /// Вагон
    /// </summary>
    public class Carriage : BaseEntity
    {
        /// <summary>
        /// Заводской номер
        /// </summary>
        public string Serial { get; set; }

        /// <summary>
        /// Номер вагона
        /// </summary>
        public int Number { get; set; }

        public int? TrainId { get; set; }

        public virtual Train Train { get; set; }

        public int ModelId { get; set; }

        public virtual Model Model { get; set; }
    }
}
