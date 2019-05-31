using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    public class TrainTaskExecutor : BaseEntityRef
    {
        public int TrainTaskId { get; set; }

        public TrainTask TrainTask{ get; set; }

        public DateTime Date { get; set; }

        public BrigadeType BrigadeType { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
