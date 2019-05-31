using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    public class Brigade : BaseEntityName
    {
        public BrigadeType BrigadeType { get; set; }
    }
}
