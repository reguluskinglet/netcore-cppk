using Rzdppk.Model.Base;

namespace Rzdppk.Model.Raspisanie
{
    public class TripOnRoute : BaseEntity
    {
        public int RouteId { get; set; }
        public virtual Route Route { get; set; }

        public int TripId { get; set; }
        public virtual Trip Trip { get; set; }
    }
}