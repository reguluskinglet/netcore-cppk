using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Model
{
    /// <summary>
    /// Маршрут
    /// </summary>
    public class Route : BaseEntityName, IEquatable<Route>
    {
        public virtual ICollection<TripOnRoute> TripOnRoutes { get; set; }
        public virtual ICollection<PlanedRouteTrain> PlanedRouteTrains { get; set; }
        public virtual ICollection<DepoEvent> DepoEvents { get; set; }

        public int? TurnoverId { get; set; }
        public virtual Turnover Turnover { get; set; }

        public double Mileage { get; set; }

        public bool Equals(Route other)
        {
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
