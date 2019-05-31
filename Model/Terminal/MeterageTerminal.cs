using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.Terminal
{
    public class MeterageTerminal : BaseEntityTerminal
    {
        public DateTime Date { get; set; }

        public int? Value { get; set; }

        public Guid? InspectionId { get; set; }
        public InspectionTerminal Inspection { get; set; }

        public int? LabelId { get; set; }

        public Label Label { get; set; }
    }
}
