SELECT *,
CAST(
   CASE WHEN EXISTS(SELECT * FROM Labels l WHERE l.EquipmentModelId = em.Id) THEN 0
   WHEN EXISTS(SELECT * FROM CheckListEquipments c WHERE c.EquipmentModelId = em.Id) THEN 0
   WHEN EXISTS(SELECT * FROM TrainTasks t WHERE t.EquipmentModelId = em.Id) THEN 0
   WHEN EXISTS(SELECT * FROM EquipmentModels em1 WHERE em1.ParentId = em.Id) THEN 0
   ELSE 1
   END 
AS BIT) as CanDelete
FROM EquipmentModels em
LEFT JOIN Equipments e ON e.id=em.EquipmentId
WHERE em.ModelId=@model_id
