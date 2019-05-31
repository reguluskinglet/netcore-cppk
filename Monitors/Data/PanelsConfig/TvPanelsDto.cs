using RzdMonitors.Data.Enums;

namespace RzdMonitors.Data.PanelsConfig
{
    public class TvPanelsDto
    {
        public TvPanelsDto()
        {
            Types = new []{ ScreenType.None };
        }

        public int Num { get; set; }

        public ScreenType[] Types { get; set; }
    }
}
