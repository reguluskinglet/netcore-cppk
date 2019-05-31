using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    public class DeviceValue : BaseEntity
    {
        public DeviceValueType Type { get; set; }

        public int Value { get; set; }

        public int DeviceId { get; set; }

        public virtual Device Device { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }
    }
}
