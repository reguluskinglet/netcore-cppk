using System;

namespace Rzdppk.Model.Dto
{
    public class TaskOutDto
    {
        public int DeviceId { get; set; }

        public int UserId { get; set; }

        public int? DeviceFaultId { get; set; }

        public DateTime CreateDate { get; set; }

        public Guid RefId { get; set; }
    }
}
