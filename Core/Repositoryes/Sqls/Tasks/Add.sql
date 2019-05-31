insert into TrainTasks
(CarriageId, CreateDate, Description, EquipmentModelId, FaultId, TaskType, UserId)
Values
(@carrigeId, CURRENT_TIMESTAMP, @description, @equipmentModelId, @faultId, @taskType , @userId)
SELECT SCOPE_IDENTITY()