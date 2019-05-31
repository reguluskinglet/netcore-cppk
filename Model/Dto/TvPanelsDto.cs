using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Dto
{
    public class TvPanelsDto
    {
        public int Num { get; set; }

        public ScreenType[] Types { get; set; }
    }
}
