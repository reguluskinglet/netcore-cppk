UPDATE
EquipmentCategoryes
SET 
name = @name, 
description = @description,
UpdateDate = CURRENT_TIMESTAMP
WHERE
id = @id
