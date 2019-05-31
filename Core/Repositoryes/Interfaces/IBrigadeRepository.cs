using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface IBrigadeRepository
    {
        Task Update(Brigade brigade);

        Task<BrigadeRepository.BrigadePaging> GetAll(int skip, int limit);

        Task<Brigade> ById(int id);

        Task Add(Brigade equipmentCategory);
        Task<Brigade> Delete(int id);


        //Task<User> FindByLoginAsync(object filterData, string query);
    }
}