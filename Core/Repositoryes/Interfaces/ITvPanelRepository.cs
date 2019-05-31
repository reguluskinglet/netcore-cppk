using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Dto;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface ITvPanelRepository
    {
        Task<int> RegisterBox(TvBoxRegisterDto box);

        Task<List<TvPanelsDto>> GetBoxPanels(int boxId);

        Task AddBoxPanels(TvBoxPanelsDto dto);

        Task DeleteBoxPanels(TvBoxPanelsDto dto);

        Task<string> GetDepoName(int depoStantionId);

        //report 1
        Task<ScheduleDeviationTableDto> GetScheduleDeviationTable();

        //report2
        Task<ScheduleDeviationGraphDto> GetScheduleDeviationGraphData(DateTime start, DateTime end);

        //report 3
        Task<BrigadeScheduleDeviationTableDto> GetBrigadeScheduleDeviationTable();

        //report 4
        Task<ToDeviationTableDto> GetToDeviationTable();

        //report 5
        Task<CriticalMalfunctionsTableDto> GetCriticalMalfunctionsTable();

        //report 7
        Task<TrainsInDepoMalfunctionTableDto> GetTrainsInDepoDepoMalfunctionsTable(int depoStantionId);

        //report 8
        Task<TrainsInDepoStatusTableDto> GetTrainsInDepoStatusTable(int depoStantionId);
        Task<JournalsTableDto> GetJournalsTable(int depoStantionId);
    }
}