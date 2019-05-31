using System.Threading.Tasks;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface IEquipmentRepository
    {
        Task<EquipmentRepository.EquipmentUIPaging> GetEquipmentWithCheckLists(int model_id, int parent_id, int skip,
            int limit);

        Task<EquipmentRepository.CheckListEquipmentUI> GetCheckListByEquipmentModelId(int id);

        //Task<EquipmentRepository.CheckListEquipmentUI> AddCheckListsToEquipment(EquipmentRepository.CheckListEquipmentUI ces);

        Task DeleteCheckListFromEquipment(CheckListEquipment ce);

        Task AddFaultToEquipment(FaultEquipment faultEquipment);

        Task RemoveFaultFromEquipment(FaultEquipment faultEquipment);

        //void Update(Equipment equipment);

        Task<EquipmentRepository.EquipmentPaging> GetAll(int skip, int limit);

        Task<EquipmentRepository.EquipmentPaging> GetByCategory(EquipmentCategory cat, int skip, int limit, string filter = null);

        //Equipment Add(Equipment equipment);

        Task Delete(int id);
    }
}