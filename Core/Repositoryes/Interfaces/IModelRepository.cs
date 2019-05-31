using System.Data.SqlClient;
using System.Threading.Tasks;
using Rzdppk.Model;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface IModelRepository
    {

        Task<EquipmentModel> AddEquipmentToModel(EquipmentModel equipmentModel);
        
        //void DeleteEquipmentFromModel(int id, SqlTransaction tran = null);
        Task DeleteEquipmentFromModel(int id);

        //  Task<ModelRepository.LocationModelExt[]> GetLocationsByModel(Model.Model model);

        Task<ModelRepository.EquipmentModelExt[]> GetEquipmentByModel(Model.Model model);

        Task Update(Model.Model model);

        Task<ModelRepository.ModelPaging> GetAll(int skip, int limit);

        Task<Model.Model> GetById(int id);

        Task<Model.Model> Add(Model.Model equipmentCategory);

        Task Delete(int id);
    }
}