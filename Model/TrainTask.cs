using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    /// <summary>
    /// Задачи\Инциденты
    /// </summary>
    public class TrainTask : BaseEntityRef
    {
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата редактирования изменения \ пользователем
        /// </summary>
        public DateTime? EditDate { get; set; }

        public string Description { get; set; }

        public TaskType TaskType { get; set; }

        public int CarriageId { get; set; }

        public virtual Carriage Carriage { get; set; }

        public int EquipmentModelId { get; set; }

        public virtual EquipmentModel EquipmentModel { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public virtual ICollection<TrainTaskStatus> TrainTaskStatuses { get; set; }

        public virtual ICollection<TrainTaskComment> TrainTaskComments { get; set; }

        public virtual ICollection<TrainTaskExecutor> TrainTaskExecutors { get; set; }

        public virtual ICollection<TrainTaskAttribute> TrainTaskAttributes { get; set; }

    }
}

