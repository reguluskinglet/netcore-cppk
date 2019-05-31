using System.Collections.Generic;
using TabletLocker.Model;

namespace TabletLocker.Db
{
    public interface IDeviceFaultRepository
    {
        List<DeviceFault> GetAll();
    }
}