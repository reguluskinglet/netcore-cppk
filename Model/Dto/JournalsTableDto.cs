using System;
using System.Collections.Generic;
using System.Text;

namespace Rzdppk.Model.Dto
{
  public  class JournalsTableDto
    {
        public int GraphViolationCount { get; set; }
        public int TrainDepoCount { get; set; }
        public int TrainInTripCount { get; set; }
        public IEnumerable<JournalsTableItemDto> Items { get; set; }
    }
}
