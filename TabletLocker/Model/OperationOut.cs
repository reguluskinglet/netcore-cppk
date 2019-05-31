using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rzdppk.Model.Enums;

namespace TabletLocker.Model
{
    public class OperationOut
    {
        public int Id { get; set; }

        public DeviceOperation Operation { get; set; }

        public int DeviceId { get; set; }

        public virtual Device Device { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsSent { get; set; }

        public Guid RefId { get; set; }
    }
}