SELECT *
FROM EquipmentModels as em
WHERE em.ModelId=@model_id AND em.ParentId IS NULL
ORDER BY em.Id
OFFSET @skip ROWS
FETCH NEXT @limit ROWS ONLY;