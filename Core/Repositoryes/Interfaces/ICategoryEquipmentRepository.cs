using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface ICategoryEquipmentRepository
    {
        Task Update(EquipmentCategory equipmentCategory);

        //Task<CategoryEquipmentRepository.EquipmentCategoryPaging> GetAll(int skip, int limit, string filter);

        Task<EquipmentCategory> GetById(int id);

        Task Add(EquipmentCategory equipmentCategory);
        Task Delete(int id);


        //Task<User> FindByLoginAsync(object filterData, string query);
    }
}