using System.Threading.Tasks;
using Rzdppk.Model;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface ICommentRepository
    {
        Task<TrainTaskComment[]> GetByTaskId(int id);
    }
}