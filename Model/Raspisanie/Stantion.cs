using System.Collections.Generic;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Model
{
    /// <summary>
    /// Станции
    /// </summary>
    public class Stantion : BaseEntityName
    {
        public string ShortName { get; set; }

        public StantionType StantionType { get; set; }

        public virtual ICollection<Train> Trains { get; set; }

        public virtual ICollection<StantionOnTrip> StantionOnTrips { get; set; }

        public virtual ICollection<Parking> Parkings { get; set; }
    }
}
