select 
traintask.*,
users.Name as InitiatorName, 
carriages.Number as carriageNumber, Carriages.Serial as carriageSerial,
trains.Name as trainName, 
faults.FaultType as faultTypeId, faults.Name as faultName,
equipments.name as equipmentName,
models.ModelType as carriageModelTypeId

from TrainTasks as traintask
LEFT JOIN auth_users AS users ON traintask.UserId = users.Id
LEFT JOIN Carriages AS carriages ON traintask.CarriageId = carriages.Id
LEFT JOIN Trains AS trains ON carriages.TrainId = trains.Id
LEFT JOIN Faults AS faults ON traintask.FaultId = faults.id
LEFT JOIN EquipmentModels AS equipmentmodels ON traintask.EquipmentModelId = equipmentmodels.Id
LEFT JOIN Equipments as equipments ON equipmentmodels.EquipmentId = equipments.Id
LEFT JOIN Models as models ON carriages.ModelId = models.Id

WHERE traintask.id = @id;

