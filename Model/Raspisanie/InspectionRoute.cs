using System;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Raspisanie
{
    public class InspectionRoute:BaseEntity
    {
        public int RouteId { get; set; }
        public virtual Route Route { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public CheckListType? CheckListType { get; set; }
    }
}