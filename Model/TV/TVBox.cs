using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.TV
{
    public class TVBox : BaseEntity
    {
        public string Name { get; set; }
        public virtual IEnumerable<TVPanel> TVPanels { get; set; }
    }
}
