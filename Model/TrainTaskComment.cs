using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    public class TrainTaskComment : BaseEntityRef
    {
        public int TrainTaskId { get; set; }

        public virtual TrainTask TrainTask { get; set; }

        public DateTime Date { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public string Text { get; set; }

        public virtual ICollection<Document> Documents { get; set; }
    }
}
