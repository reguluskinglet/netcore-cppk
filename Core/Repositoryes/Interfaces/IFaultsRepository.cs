using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface IFaultsRepository
    {
        //Task Update(Fault fault);

        Task<FaultsRepository.FaultPaging> GetAll(int skip, int limit);

        //Fault ByIdWithStations(int id);

        //Task Add(Fault equipmentCategory);
        Task Delete(int id);


        Task<Fault[]> GetByEquipmentId(int id);


        //Task<User> FindByLoginAsync(object filterData, string query);
    }
}