INSERT into
EquipmentModels
(EquipmentId, ModelId, ParentId, IsMark)
VALUES
(@equipmentId, @modelId, @parentId, @IsMark)
SELECT SCOPE_IDENTITY()