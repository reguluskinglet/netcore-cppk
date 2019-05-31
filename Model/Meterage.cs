using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    public class Meterage : BaseEntity
    {
        public DateTime Date { get; set; }

        public int? Value { get; set; }

        public int? InspectionId { get; set; }
        public Inspection Inspection { get; set; }

        public int? LabelId { get; set; }

        public Label Label { get; set; }

        /// <summary>
        /// Признак того что метка была считана RFID оборудованием
        /// </summary>
        public bool IsRfidScaned { get; set; }

        public int? TaskStatusId { get; set; }
        public virtual TrainTaskStatus TaskStatus { get; set; }
    }
}
