using System.Data.SqlClient;
using System.Threading.Tasks;
using Rzdppk.Model;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface IMeterageRepository
    {
        Task<MeterageRepository.LabelUI[]> GetLabels(int inspectionId);

        Task<MeterageRepository.MeterageUI[]> GetMeterages(int inspectionId);
    }
}