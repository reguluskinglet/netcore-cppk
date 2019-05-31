using System.Collections.Generic;

namespace WpfMultiScreens.Data.Reports
{
  public  class JournalsTableDto
    {
        public int GraphViolationCount { get; set; }
        public int TrainDepoCount { get; set; }
        public int TrainInTripCount { get; set; }
        public IEnumerable<JournalsTableItemDto> Items { get; set; }
    }
}
