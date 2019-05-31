using Rzdppk.Model;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface IExecutorRepository
    {
        TrainTaskExecutor[] GetByTaskId(int id);
    }
}