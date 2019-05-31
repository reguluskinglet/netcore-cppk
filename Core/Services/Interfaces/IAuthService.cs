using System.Security.Claims;
using System.Threading.Tasks;
using Rzdppk.Core.ViewModels;
using Rzdppk.Model.Auth;

namespace Rzdppk.Core.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User> FindByLoginAsync(string login, string password, bool verifyPassword);
        Task<User> GetCurrentUser();
        ClaimsIdentity GetIdentity(User user);

        UserInfoDto GetUserInfo(User user);
    }
}