using System;
using System.Collections.Generic;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Model
{
    /// <summary>
    /// Рейс
    /// </summary>
    public class Trip : BaseEntityName
    {
       public TripType TripType { get; set; }

       public virtual ICollection<TripOnRoute> TripOnRoutes { get; set; }
       public virtual ICollection<StantionOnTrip> StantionOnTrips { get; set; }
    }
}
