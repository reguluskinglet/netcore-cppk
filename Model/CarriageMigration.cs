using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;

namespace Rzdppk.Model
{
    public class CarriageMigration : BaseEntity
    {
        public int CarriageId { get; set; }
        public virtual Carriage Carriage { get; set; }

        public int TrainId { get; set; }
        public Train Train { get; set; }

        public int StantionId { get; set; }
        public Stantion Stantion { get; set; }
    }
}
