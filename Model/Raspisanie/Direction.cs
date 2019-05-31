using System.Collections.Generic;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.Raspisanie
{
    public class Direction : BaseEntity
    {
        public string Name { get; set; }

        public virtual ICollection<Train> Trains { get; set; }
        public virtual ICollection<Turnover> Turnovers { get; set; }
    }
}