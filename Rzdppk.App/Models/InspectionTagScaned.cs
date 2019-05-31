using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;
using Rzdppk.Model.Terminal;

namespace Rzdppk.App.Models
{
    public class InspectionTagScaned : BaseEntity
    {
        public SendEntityStatus SendStatus { get; set; }

        public int LabelId { get; set; }

        public Label Label { get; set; }

        public Guid? InspectionId { get; set; }

        public InspectionTerminal Inspection { get; set; }

        public bool IsRfidScaned { get; set; }

        public Guid? TaskStatusId { get; set; }
        public TrainTaskStatusTerminal TaskStatus { get; set; }
    }
}
