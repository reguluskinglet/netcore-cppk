using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Base
{
    public class BaseEntityTerminal
    {
        public Guid Id { get; set; }

        public DateTime UpdateDate { get; set; }

        public SendEntityStatus SendStatus { get; set; }
    }
}
