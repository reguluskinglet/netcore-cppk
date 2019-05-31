using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    public class DeviceHistory : BaseEntityRef
    {
        public DeviceOperation Operation { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int DeviceId { get; set; }
        public virtual Device Device { get; set; }
    }
}