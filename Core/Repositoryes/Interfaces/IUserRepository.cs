using System.Threading.Tasks;
using Rzdppk.Model.Auth;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByLogin(string login);

        Task<UserRepository.UserPaging> GetAll(int skip, int limit);

        Task<User> FindByLoginAsync(object filterData, string query);


        Task<User> AddOrUpdate(User user);
        Task Update(User user);
        Task Delete(int id);

    }
}