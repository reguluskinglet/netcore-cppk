SELECT *
FROM Equipments as e
LEFT JOIN EquipmentCategoryes as ec ON e.CategoryId = ec.Id
ORDER BY e.Name
OFFSET @skip ROWS
FETCH NEXT @limit ROWS ONLY;