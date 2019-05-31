using System.Collections.Generic;
using System.Threading.Tasks;
using Rzdppk.Core.Services;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskRepository.TrainTaskPaging> GetAll(int skip, int limit, string filter);

        //Task<TaskService.TaskDetail> GetTask(int id);

        List<TaskService.TrainTaskHistoryUi> GetHistoryById(int id);
    }
}
