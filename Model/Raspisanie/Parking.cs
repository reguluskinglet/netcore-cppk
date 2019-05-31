using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.Raspisanie
{
    public class Parking : BaseEntityName
    {
        public int StantionId { get; set; }
        public virtual Stantion Stantion { get; set; }

        public virtual ICollection<DepoEvent> DepoEvents { get; set; }
    }
}
