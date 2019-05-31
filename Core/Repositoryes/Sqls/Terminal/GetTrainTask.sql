SELECT t.RefId as Id, CarriageId, CreateDate, EquipmentModelId, TaskType, t.UserId,t.UpdateDate,t.Description,t.Id as TaskNumber
FROM  TrainTasks t
--WHERE t.UpdateDate>@date