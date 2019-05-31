SELECT x.RefId as Id, CheckListEquipmentId, FaultId,i.RefId as InspectionId, TaskLevel,t.RefId as TrainTaskId,x.UpdateDate, x.UserId, Value
FROM  TrainTaskAttributes x
INNER JOIN TrainTasks t ON t.Id=TrainTaskId
INNER JOIN Inspections i ON i.Id=InspectionId
--WHERE x.UpdateDate>@date