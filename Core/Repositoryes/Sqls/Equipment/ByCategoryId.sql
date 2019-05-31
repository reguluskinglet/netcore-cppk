SELECT *
FROM Equipments as e
LEFT JOIN EquipmentCategoryes as ec ON e.CategoryId = ec.Id
WHERE e.CategoryId=@category_id order by e.Name
OFFSET @skip ROWS
FETCH NEXT @limit ROWS ONLY;