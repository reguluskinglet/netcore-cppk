using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Rzdppk.Core.Helpers;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Core.ViewModels;
using Rzdppk.Model.Auth;
using Microsoft.AspNetCore.Http;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDb _db;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthService
        (
            IUserRepository userRepository,
            IHttpContextAccessor contextAccessor,
            IDb db
        )
        {
            _userRepository = userRepository;
            _contextAccessor = contextAccessor;
            _db = db;
        }


        public async Task<User> FindByLoginAsync(string login, string password, bool verifyPassword)
        {
            var user = await _userRepository.GetUserByLogin(login);

            if (user == null || verifyPassword && !CryptoHelper.VerifyHashedPassword(user.PasswordHash, password))
                return null;

            return user;
        }

        public async Task<User> GetCurrentUser()
        {
            var userName = _contextAccessor.HttpContext.User.Identity.Name;

            return await _userRepository.GetUserByLogin(userName);
        }

        public ClaimsIdentity GetIdentity(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.Permissions.ToString()),
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, "Role");

            return claimsIdentity;
        }

        public UserInfoDto GetUserInfo(User user)
        {
            var userInfo = new UserInfoDto
            {
                Id = user.Id,
                Permissions = user.Role.Permissions,
                Name = user.Name,
                BrigadeId= user.BrigadeId,
                BrigadeType=user.Brigade?.BrigadeType
            };

            return userInfo;
        }
    }
}
