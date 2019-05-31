select 
traintask.TaskType as tasktype, traintask.createDate as createDate, traintask.Id as taskId,
users.Name as userName, 
carriages.Number as carriageNumber, 
carriages.Id as carriageId,
carriages.Serial as carriagesSerialNumber,
trains.Name as trainName, trains.Id as trainId,
equipments.name as equipmentName,
faults.Name as faultName,
i.Id as inspectionId,
i.CheckListType as inspectionType,
taskAtrib.Id as TaskAttributeId

from TrainTasks as traintask
LEFT JOIN auth_users AS users ON traintask.UserId = users.Id
LEFT JOIN Carriages AS carriages ON traintask.CarriageId = carriages.Id
LEFT JOIN Trains AS trains ON carriages.TrainId = trains.Id
LEFT JOIN EquipmentModels AS equipmentmodels ON traintask.EquipmentModelId = equipmentmodels.Id
LEFT JOIN Equipments as equipments ON equipmentmodels.EquipmentId = equipments.Id
LEFT JOIN TrainTaskAttributes as taskAtrib ON taskAtrib.TrainTaskId = traintask.Id
LEFT JOIN Faults AS faults ON taskAtrib.FaultId = faults.id
LEFT JOIN Inspections i on i.Id=taskAtrib.InspectionId
--where traintask.InspectionId is null
order by 1
--OFFSET @skip ROWS
--FETCH NEXT @limit ROWS ONLY