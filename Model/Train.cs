using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Model
{
    /// <summary>
    /// Поезд
    /// </summary>
    public class Train : BaseEntityName, IEquatable<Train>
    {
        public int? StantionId { get; set; }

        public Stantion Stantion { get; set; }

        public int? DirectionId { get; set; }

        public virtual Direction Direction { get; set; }

        public virtual ICollection<DepoEvent> DepoEvents { get; set; }

        public bool Equals(Train other)
        {
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
