using System.Threading.Tasks;
using Rzdppk.Model.Enums;
using System.Collections.Generic;

namespace Rzdppk.Core.Repositoryes
{
    public interface ICheckListEquipmentRepository
    {
        Task<List<CheckListEquipmentRepository.CheckListEquipmentTableItem>> GetList(int equipmentModleId);

        Task<CheckListEquipmentRepository.CheckListEquipmentTableItem> AddChecklist(CheckListEquipmentRepository.CheckListEquipmentDto input);

        Task DeleteChecklist(int id);
    }
}