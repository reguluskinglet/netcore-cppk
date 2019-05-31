SELECT d.Id, p.Name as ParkingName, i.CheckListType, biu.BrigadeType, t.Name as TrainName, r.Name as RouteName,u.Name as UserName, InTime, OutTime ,d.InspectionTxt, ParkingTime, RepairStopTime, TestStartTime, TestStopTime
FROM   DepoEvents d
INNER JOIN Trains t ON t.Id=TrainId
LEFT OUTER JOIN Routes r ON r.Id = d.RouteId
INNER JOIN Parkings p ON p.Id=d.ParkingId
LEFT JOIN auth_users u ON u.Id=d.UserId
LEFT JOIN Inspections i ON i.Id=d.InspectionId
LEFT JOIN auth_users iu ON iu.id=i.UserId
LEFT JOIN Brigades biu ON biu.Id=i.UserId
WHERE 1=1
--and