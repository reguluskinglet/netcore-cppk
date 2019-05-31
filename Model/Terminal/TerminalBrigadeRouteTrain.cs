using System;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Terminal
{
    /// <summary>
    /// Заплонированные мероприятия  c изменениями
    /// </summary>
    public class TerminalBrigadeRouteTrain : BaseEntity
    {
        public int PlanedRouteTrainId { get; set; }

        public int StantionEndId { get; set; }

        public int StantionStartId { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public DateTime Date { get; set; }

        public BrigadeType BrigadeType { get; set; }

        /// <summary>
        /// Поезд
        /// </summary>
        public int TrainId { get; set; }
        public virtual Train Train { get; set; }

        public int RouteId { get; set; }
        public Route Route { get; set; }
    }
}