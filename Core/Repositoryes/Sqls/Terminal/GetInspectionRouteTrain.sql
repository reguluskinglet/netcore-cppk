SELECT pir.Id,pir.CheckListType,pir.Start,pir.[End], prt.Date,prt.RouteId,prt.TrainId, chir.Start as ChangeStart,chir.[End] as ChangeEnd
FROM PlanedInspectionRoutes pir
INNER JOIN PlanedRouteTrains prt ON prt.Id=pir.PlanedRouteTrainId
LEFT JOIN ChangedPlanedInspectionRoutes chir ON chir.PlanedInspectionRouteId=pir.Id