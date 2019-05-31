using System.Collections.Generic;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.Raspisanie
{
    public class Turnover : BaseEntity
    {
        public string Name { get; set; }

        public int DirectionId { get; set; }
        public virtual Direction Direction { get; set; }

        public virtual ICollection<Route> Routes { get; set; }
    }
}