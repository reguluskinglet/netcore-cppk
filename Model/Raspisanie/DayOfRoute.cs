using System;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.Raspisanie
{
    public class DayOfRoute : BaseEntity
    {
        public DayOfWeek Day { get; set; }

        public int TurnoverId { get; set; }
        public virtual Turnover Turnover { get; set; }
    }
}