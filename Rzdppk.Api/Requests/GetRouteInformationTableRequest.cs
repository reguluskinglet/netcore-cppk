namespace Rzdppk.Api.Requests
{
    public class GetRouteInformationTableRequest
    {
        public int PlanedRouteTrainId { get; set;}
        public int EntityId { get; set; }
        public int TimelineTypeEnum { get; set;}
    }
}