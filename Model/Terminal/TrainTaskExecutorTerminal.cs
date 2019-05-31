using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Terminal
{
    public class TrainTaskExecutorTerminal : BaseEntityTerminal
    {
        public Guid TrainTaskId { get; set; }

        public TrainTaskTerminal TrainTask { get; set; }

        public DateTime Date { get; set; }

        public BrigadeType BrigadeType { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
