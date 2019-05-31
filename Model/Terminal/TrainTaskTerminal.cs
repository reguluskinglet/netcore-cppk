using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Terminal
{
    public class TrainTaskTerminal : BaseEntityTerminal
    {
        public int? TaskNumber { get; set; }

        public DateTime CreateDate { get; set; }

        public string Description { get; set; }

        public TaskType TaskType { get; set; }

        public int CarriageId { get; set; }

        public virtual Carriage Carriage { get; set; }

        public int EquipmentModelId { get; set; }

        public virtual EquipmentModel EquipmentModel { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public virtual ICollection<TrainTaskStatusTerminal> TrainTaskStatuses { get; set; }

        public virtual ICollection<TrainTaskCommentTerminal> TrainTaskComments { get; set; }

        public virtual ICollection<TrainTaskExecutorTerminal> TrainTaskExecutors { get; set; }

        public virtual ICollection<TrainTaskAttributeTerminal> TrainTaskAttributes { get; set; }
    }
}
