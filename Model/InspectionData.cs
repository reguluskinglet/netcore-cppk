using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    public class InspectionData : BaseEntityRef
    {
        public InspectionDataType Type { get; set; }

        public int Value { get; set; }

        public int InspectionId { get; set; }

        public virtual Inspection Inspection { get; set; }

        public int? CarriageId { get; set; }

        public Carriage Carriage { get; set; }

        public string Text { get; set; }
    }
}
