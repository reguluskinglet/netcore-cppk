insert into TrainTasks
(CarriageId, CreateDate, Description, EquipmentModelId, TaskType, UserId)
Values
(@carrigeId, CURRENT_TIMESTAMP, @description, @equipmentModelId, @taskType , @userId)
SELECT SCOPE_IDENTITY()