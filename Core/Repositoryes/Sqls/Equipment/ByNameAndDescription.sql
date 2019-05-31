SELECT e.name as name, e.Description as description, e.id as id, e.CategoryId as CategoryId
FROM Equipments as e
LEFT JOIN EquipmentCategoryes as ec ON e.CategoryId = ec.Id
WHERE 
e.name=@name AND
e.description = @description