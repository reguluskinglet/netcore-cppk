using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Model
{
    /// <summary>
    /// Плановые мероприятия
    /// </summary>
    public class Inspection : BaseEntityRef
    {
        public CheckListType CheckListType { get; set; }

        /// <summary>
        /// Дата и время начала ТО
        /// </summary>
        public DateTime DateStart { get; set; }

        /// <summary>
        /// Дата и время окончания ТО
        /// </summary>
        public DateTime? DateEnd { get; set; }

        public InspectionStatus Status { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int TrainId { get; set; }
        public Train Train { get; set; }

        public ICollection<Signature> Signatures { get; set; }

        public ICollection<Meterage> Meterages { get; set; }

        public ICollection<InspectionData> InspectionDatas { get; set; }

        public virtual ICollection<DepoEvent> DepoEvents { get; set; }
    }
}
