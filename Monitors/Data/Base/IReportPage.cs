using RzdMonitors.Data.Enums;

namespace RzdMonitors.Data.Base
{
    public interface IReportPage
    {
        ScreenType ScreenType { get; set; }

        void UpdateData();
    }
}
