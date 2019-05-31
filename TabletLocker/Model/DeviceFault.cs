using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TabletLocker.Model
{
    public class DeviceFault
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}