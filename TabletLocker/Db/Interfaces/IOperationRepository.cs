using System.Collections.Generic;
using TabletLocker.Model;

namespace TabletLocker.Db.Interfaces
{
    public interface IOperationRepository
    {
        void Add(OperationOut oper);

        List<OperationOut> GetAllUnsentForDevice(int deviceId);

        List<OperationOut> GetAllUnsent();

        OperationOut GetLastUnsentForDevice(int deviceId);
    }
}