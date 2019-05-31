using System;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.Raspisanie
{
    public class DayOfTrip : BaseEntity
    {
        public int TripId { get; set; }
        public virtual Trip Trip { get; set; }

        public DayOfWeek Day { get; set; }
    }
}