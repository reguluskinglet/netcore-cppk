using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rzdppk.Model.Enums;

namespace TabletLocker.Model
{
    public class Device
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Serial { get; set; }

        public int CellNumber { get; set; }

        public int? OpenTasksCount { get; set; }

        public DeviceOperation? LastOperation { get; set; }

        public int? LastOperationUserId { get; set; }

        public virtual User LastOperationUser { get; set; }

        public DateTime? LastOperationDate { get; set; }

        public int? LastCharge { get; set; }

        public DateTime? LastChargeDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}