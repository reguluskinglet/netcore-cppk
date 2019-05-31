using System;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Terminal
{
    public class TerminalInspectionRouteTrain: BaseEntity
    {
        public CheckListType CheckListType { get; set; }

        /// <summary>
        /// Дата начала ТО-2
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Дата окончания ТО-2
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// День прохождения по маршруту
        /// </summary>
        public DateTime Date { get; set; }

        public int RouteId { get; set; }
        public virtual Route Route { get; set; }

        public int TrainId { get; set; }
        public Train Train { get; set; }
    }
}