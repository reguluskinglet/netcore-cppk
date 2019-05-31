using System.Collections.Generic;
using System.Threading.Tasks;
using Rzdppk.Core.ViewModels;

namespace Rzdppk.Core.Services.Interfaces
{
    public interface ICommonService
    {
        Task<AutocompliteData> GetAllReference();
        Task<TripSource> GetTripSource();
        Task<IEnumerable<SelectItem>> GetDirectionsSelectItem();
        Task<DepoEventDataSource> GetDepoEventDataSource();
    }
}