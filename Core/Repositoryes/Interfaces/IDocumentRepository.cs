using System.Threading.Tasks;
using Rzdppk.Model;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface IDocumentRepository
    {
        Task<Document[]> Add(Document[] docs);

        Task<Document> GetById(int id);

        Task<Document[]> GetByTaskId(int id);

        Task Delete(int id);
    }
}
