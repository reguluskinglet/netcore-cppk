using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.Auth
{
    /// <summary>
    /// Пользователь системы
    /// </summary>
    public class User : BaseEntity
    {
        /// <summary>
        /// Логин или Имя компьютера
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// ФИО
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Хэш пароля
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Заблокирован
        /// </summary>
        public bool IsBlocked { get; set; }

        public int RoleId { get; set; }

        public UserRole Role { get; set; }

        public int? BrigadeId { get; set; }

        public Brigade Brigade { get; set; }

        /// <summary>
        /// Должность
        /// </summary>
        public string PersonPosition { get; set; }

        /// <summary>
        /// Табельный номер
        /// </summary>
        public string PersonNumber { get; set; }

        public virtual ICollection<Signature> SignatureUsers { get; set; }

        public virtual ICollection<TrainTaskComment> TrainTaskComments { get; set; }

        public virtual ICollection<TrainTaskExecutor> TrainTaskExecutors { get; set; }

        public virtual ICollection<TrainTaskStatus> TrainTaskStatuses { get; set; }

        public virtual ICollection<TrainTaskAttribute> TrainTaskAttributes { get; set; }

        public virtual ICollection<PlaneBrigadeTrain> PlaneBrigadeTrains { get; set; }

        public virtual ICollection<ChangePlaneBrigadeTrain> ChangePlaneBrigadeTrains { get; set; }
    }
}
