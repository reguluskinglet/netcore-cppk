using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model
{
    public class RemovedEntity : BaseEntity
    {
        public TableNames Table { get; set; }

        /// <summary>
        /// Пишим сюда текущий Id сущности которая удаляется
        /// </summary>
        public int EntityIdentifler { get; set; }

        /// <summary>
        /// Если у удаленной сущности есть RefId пишим сюда
        /// </summary>
        public Guid? RefIdentifler { get; set; }
    }
}
