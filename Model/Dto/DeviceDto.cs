using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Dto
{
    public class DeviceDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Serial { get; set; }

        public int CellNumber { get; set; }

        public int? OpenTasksCount { get; set; }

        public DeviceOperation? LastOperation { get; set; }

        public int? LastOperationUserId { get; set; }

        public DateTime? LastOperationDate { get; set; }

        public int? LastCharge { get; set; }

        public DateTime? LastChargeDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
