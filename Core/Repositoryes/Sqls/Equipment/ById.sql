SELECT *
FROM Equipments as e
LEFT JOIN EquipmentCategoryes as ec ON e.CategoryId = ec.Id
WHERE e.id=@equipment_id