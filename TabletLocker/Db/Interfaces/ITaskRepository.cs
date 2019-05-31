using System.Collections.Generic;
using TabletLocker.Model;

namespace TabletLocker.Db
{
    public interface ITaskRepository
    {
        void Add(TaskOut task);

        List<TaskOut> GetUnsentForDevice(int deviceId);

        List<TaskOut> GetAllUnsent();
    }
}