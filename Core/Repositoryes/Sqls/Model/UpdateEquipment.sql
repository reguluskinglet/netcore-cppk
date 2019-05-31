UPDATE
EquipmentModels
SET EquipmentId=@equipment_id, IsMark = @IsMark,
UpdateDate = CURRENT_TIMESTAMP
WHERE Id=@id