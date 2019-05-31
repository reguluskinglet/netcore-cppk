using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;

namespace Rzdppk.Model.Terminal
{
    public class TrainTaskCommentTerminal : BaseEntityTerminal
    {
        public Guid TrainTaskId { get; set; }

        public virtual TrainTaskTerminal TrainTask { get; set; }

        public DateTime Date { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public string Text { get; set; }

        public virtual ICollection<DocumentTerminal> Documents { get; set; }
    }
}
