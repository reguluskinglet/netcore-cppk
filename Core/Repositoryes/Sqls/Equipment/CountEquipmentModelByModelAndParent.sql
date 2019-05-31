SELECT COUNT(*)
FROM EquipmentModels as em
WHERE em.ModelId=@model_id AND em.ParentId=@parent_id