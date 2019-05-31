using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface ITechPassRepository
    {
        Task<TechPass> Update(TechPass input);

        Task<TechPassRepository.TechPassPaging> GetAll(int skip, int limit, string filter);

        Task<TechPass> ById(int id);

        Task<TechPass> Add(TechPass input);
        Task<TechPass> Delete(int id);
    }
}