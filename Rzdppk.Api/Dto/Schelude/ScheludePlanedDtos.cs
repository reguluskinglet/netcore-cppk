using System;
using System.Collections.Generic;

namespace Rzdppk.Api
{
    public class ScheludePlanedDtos
    {

        public class PlaneBrigadeTrainDto
        {
            public int Id { get; set; }
            public int StantionStartId { get; set; }
            public int StantionEndId { get; set; }
            public List<int> UserIds { get; set; }
            //Для совместимости
            public int UserId { get; set; }
            public int PlanedRouteTrainId { get; set; }

        }

    }
}
