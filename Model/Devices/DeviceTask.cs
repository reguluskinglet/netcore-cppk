﻿using System;
using System.Collections.Generic;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    public class DeviceTask : BaseEntityRef
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int DeviceId { get; set; }
        public Device Device { get; set; }

        public DateTime CreateDate { get; set; }

        public string Description { get; set; }

        public int? DeviceFaultId { get; set; }
        public virtual DeviceFault DeviceFault { get; set; }

        public virtual ICollection<DeviceTaskComment> DeviceTaskComments { get; set; }
    }
}