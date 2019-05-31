select 
traintask.TaskType as type, traintask.createDate as createDate, traintask.Id as taskId,
users.Name as userName, 
carriages.Number as carriageNumber, 
trains.Name as trainName,
equipments.name as erquipmentName,
faults.Name as faultName

from TrainTasks as traintask
LEFT JOIN auth_users AS users ON traintask.UserId = users.Id
LEFT JOIN Carriages AS carriages ON traintask.CarriageId = carriages.Id
LEFT JOIN Trains AS trains ON carriages.TrainId = trains.Id
LEFT JOIN EquipmentModels AS equipmentmodels ON traintask.EquipmentModelId = equipmentmodels.Id
LEFT JOIN Equipments as equipments ON equipmentmodels.EquipmentId = equipments.Id
LEFT JOIN Faults AS faults ON traintask.FaultId = faults.id
where traintask.InspectionId is null
order by taskId
--OFFSET @skip ROWS
--FETCH NEXT @limit ROWS ONLY