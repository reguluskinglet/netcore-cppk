using System.Collections.Generic;
using System.Threading.Tasks;
using Rzdppk.Core.Other;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public interface ITvPanelSetupRepository
    {
        Task<DevExtremeTableData.ReportResponse> GetTable(TvPanelRequest input);

        Task<TvPanelSetupPaging> GetAllBoxes(int skip, int limit);

        Task<TvPanelSetupPaging> GetAllBoxes(int skip, int limit, string filter);

        Task DeleteBox(int boxId);

        List<ScreenTypeDto> GetAllScreenTypes();

        Task<int> AddPanel(PanelAddDto input);

        Task ChangePanelType(int panelId, ScreenType newType);

        Task DeletePanel(int panelId);
    }
}