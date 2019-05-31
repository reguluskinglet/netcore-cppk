using System;
using Rzdppk.Model.Enums;

namespace TabletLocker.Model.Dto
{
    public class OperationOutDto
    {
        public DeviceOperation Operation { get; set; }

        public int DeviceId { get; set; }

        public int UserId { get; set; }

        public DateTime CreateDate { get; set; }

        public Guid RefId { get; set; }
    }
}