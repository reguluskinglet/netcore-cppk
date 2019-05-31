using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Enums;

namespace RzdMonitors.Db
{
    public class TvPanelGroupDto
    {
        public int Id { get; set; }

        public ScreenType[] Types { get; set; }
    }
}
