using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Terminal
{
    public class InspectionDataTerminal: BaseEntityTerminal
    {
        public InspectionDataType Type { get; set; }

        public int Value { get; set; }

        public Guid InspectionTerminalId { get; set; }

        public virtual InspectionTerminal InspectionTerminal { get; set; }

        public int? CarriageId { get; set; }

        public Carriage Carriage { get; set; }

        public string Text { get; set; }
    }
}
