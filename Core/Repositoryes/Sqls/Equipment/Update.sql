UPDATE
Equipments
SET 
name = @name, 
description = @description, 
categoryId = @categoryId,
UpdateDate = CURRENT_TIMESTAMP
WHERE
id = @id
