SELECT ce.*,em.*,e.*
FROM EquipmentModels em
LEFT JOIN Equipments e on e.Id=em.EquipmentId
LEFT JOIN CheckListEquipments ce on ce.EquipmentModelId=em.Id
WHERE em.Id=@equipment_model_id
