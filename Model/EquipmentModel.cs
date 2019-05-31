using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    public class EquipmentModel : BaseEntity
    {
        public int ModelId { get; set; }

        /// <summary>
        /// Признак подлежит маркировке
        /// </summary>
        public bool? IsMark { get; set; }

        public Model Model { get; set; }

        public int EquipmentId { get; set; }

        public virtual Equipment Equipment { get; set; }

        public int? ParentId { get; set; }

        public virtual EquipmentModel Parent { get; set; }

        public virtual ICollection<Label> Label { get; set; }

        public virtual ICollection<TrainTask> TrainTasks { get; set; }
    }
}
