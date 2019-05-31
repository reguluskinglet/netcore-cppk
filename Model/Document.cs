using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    public class Document : BaseEntityRef
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public DocumentType DocumentType { get; set; }

        public int? TrainTaskCommentId { get; set; }

        public virtual TrainTaskComment TrainTaskComment { get; set; }
    }
}
