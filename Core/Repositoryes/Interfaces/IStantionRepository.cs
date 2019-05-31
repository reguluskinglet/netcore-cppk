using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface IStantionRepository
    {
        Task<StantionsRepository.StantionPaging> GetAll(int skip, int limit);

        Task<Stantion> GetById(int id);

        Task Add(Stantion stantion);

        Task Delete(int id);
    }
}