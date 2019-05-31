using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Terminal
{
    public class InspectionTerminal : BaseEntityTerminal
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

        public virtual ICollection<SignatureTerminal> Signatures { get; set; }

        public virtual ICollection<MeterageTerminal> Meterages { get; set; }

        public virtual ICollection<InspectionDataTerminal> InspectionDataTerminals { get; set; }
    }
}
