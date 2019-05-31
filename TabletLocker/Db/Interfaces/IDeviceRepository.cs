using System.Collections.Generic;
using TabletLocker.Model;

namespace TabletLocker.Db
{
    public interface IDeviceRepository
    {
        List<Device> GetAll();

        List<Device> GetDevicesByBarcode(string barcode);

        Device GetDeviceByCellNumber(int cell);

        List<Device> GetAllCharged();
    }
}