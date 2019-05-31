SELECT Id, ParkingId, InspectionId, InspectionTxt, TrainId, RouteId, InTime, OutTime, ParkingTime, RepairStopTime, TestStartTime, TestStopTime, UserId
FROM DepoEvents
WHERE Id=@id