using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    public class TrainTaskStatus :BaseEntityRef
    {
        public int TrainTaskId { get; set; }

        public virtual TrainTask TrainTask { get; set; }

        public TaskStatus Status { get; set; }

        public DateTime Date { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }
    }
}
