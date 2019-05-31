using System;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    public class StantionOnTrip : BaseEntity
    {
        public int TripId { get; set; }
        public Trip Trip { get; set; }

        public int StantionId { get; set; }
        public Stantion Stantion { get; set; }

        
        public DateTime InTime { get; set; }

        public DateTime OutTime { get; set; }
    }
}
