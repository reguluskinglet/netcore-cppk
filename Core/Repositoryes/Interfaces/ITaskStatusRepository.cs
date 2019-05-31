using Rzdppk.Model;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface ITaskStatusRepository
    {
        TrainTaskStatus[] GetByTaskId(int id);

        TaskStatus[] GetNextStatuses(TaskStatus? current, BrigadeType? executorBrigadeType = 0, int permissions = -1);
    }
}