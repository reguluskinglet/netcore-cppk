using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    public class DeviceFault: BaseEntity
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }
}