using System;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    public class DeviceTaskComment : BaseEntityRef
    {
        public DateTime Date { get; set; }

        public string Text { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int DeviceTaskId { get; set; }
        public virtual DeviceTask DeviceTask { get; set; }

        public DeviceTaskStatus Status { get; set; }
    }
}