using System;
using System.Collections.Generic;
using System.Text;

namespace Rzdppk.Model.Base
{
    public abstract class BaseEntityName : BaseEntity
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
