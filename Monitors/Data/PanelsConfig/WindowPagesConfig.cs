using System.Windows;
using RzdMonitors.Data.Base;
using WpfMultiScreens.Windows;

namespace RzdMonitors.Data.PanelsConfig
{
    public class WindowPagesConfig
    {
        public WindowPagesConfig()
        {
            Pages = new IReportPage[]{};
        }

        public int ScreenNum { get; set; }

        public ScreenWindow Window { get; set; }

        public int CurrentIdx { get; set; } = 0;

        public IReportPage[] Pages { get; set; }
    }
}
