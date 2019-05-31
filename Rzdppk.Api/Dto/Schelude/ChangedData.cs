using System.Collections.Generic;
using static Rzdppk.Api.ScheludeChangedDtos;

namespace Rzdppk.Api
{
    public class ChangedData
    {
        public ChangedTo2OrCtoDto To2 { get; set;}
        public ChangedTo2OrCtoDto Cto { get; set; }
        public ChangeTimeRangeBrigadeDto TimeBrigade { get; set; }
        public RouteInformationTableTrip TimeRangeTrip { get; set; }
    }
}