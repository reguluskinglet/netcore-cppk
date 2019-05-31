using System;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Terminal
{
    public class DeviceTaskCommentTerminal : BaseEntityRef
    {
        public Guid DeviceTaskId { get; set; }
        public virtual DeviceTaskTerminal DeviceTask { get; set; }

        public DateTime Date { get; set; }

        public string Text { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public DeviceTaskStatus Status { get; set; }
    }
}