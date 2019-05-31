using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface IUserRoleRepository
    { 
        Task<UserRoleRepository.UserRolePaging> GetAll(int skip, int limit);

        UserRole GetById(int id);

        Task<int> Add(UserRoleRepository.UserRoleUi input);
        Task AddUpdateUserRole (UserRoleRepository.UserRoleUi input);
        
        Task Delete(int id);
    }
}