using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Terminal
{
    public class DocumentTerminal : BaseEntityTerminal
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public DocumentType DocumentType { get; set; }

        public Guid? TrainTaskCommentTerminalId { get; set; }

        public byte[] Data { get; set; }

        public virtual TrainTaskCommentTerminal TrainTaskCommentTerminal { get; set; }
    }
}
