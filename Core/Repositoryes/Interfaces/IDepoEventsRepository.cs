using System.Collections.Generic;
using System.Threading.Tasks;
using Rzdppk.Core.Other;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface IDepoEventsRepository
    {
        Task<List<LiveSearchItemDto>> GetTesterUsers(string search);

        Task<List<LiveSearchRouteTrainItemDto>> GetRoutes(string search);

        Task<List<LiveSearchItemDto>> GetTrains(string search);

        Task<List<LiveSearchItemDto>> GetDepos(string search);

        Task<List<LiveSearchItemDto>> GetDepoParkings(int depoId, string search);

        Task<List<LiveSearchItemDto>> GetInspections(int trainId, string search);

        Task<DepoEventDto> Create(DepoEventDto input);

        Task<DepoEventDto> Update(DepoEventDto input);

        Task<DepoEventDto> ById(int id);

        Task<DevExtremeTableData.ReportResponse> GetTable(DepoEventsRequest input);
    }
}