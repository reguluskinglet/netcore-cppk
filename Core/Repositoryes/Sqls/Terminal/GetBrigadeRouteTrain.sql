SELECT x.Id, x.PlanedRouteTrainId, x.StantionEndId, x.StantionStartId, x.UpdateDate, x.UserId,chb.UserId as ChangeUserId, b.BrigadeType,pr.TrainId,pr.RouteId, pr.Date,x.UpdateDate
FROM   PlaneBrigadeTrains x
INNER JOIN auth_users u ON u.Id=x.UserId
INNER JOIN Brigades b ON b.Id=u.BrigadeId
INNER JOIN PlanedRouteTrains pr ON pr.Id=x.PlanedRouteTrainId
LEFT JOIN ChangePlaneBrigadeTrains chb ON chb.PlaneBrigadeTrainId=x.Id