UPDATE [dbo].[DepoEvents]
   SET [ParkingId] = @ParkingId
      ,[InspectionId] = @InspectionId
      ,[InspectionTxt] = @InspectionTxt
      ,[TrainId] = @TrainId
      ,[RouteId] = @RouteId
      ,[InTime] = @InTime
      ,[ParkingTime] = @ParkingTime
      ,[RepairStopTime] = @RepairStopTime
      ,[TestStartTime] = @TestStartTime
      ,[TestStopTime] = @TestStopTime
      ,[UserId] = @UserId
	  ,[OutTime] = @OutTime
WHERE Id=@Id