UPDATE
Brigades
SET 
name = @name, 
description = @description, 
brigadeType = @brigadeType,
UpdateDate = CURRENT_TIMESTAMP
WHERE
id = @id
