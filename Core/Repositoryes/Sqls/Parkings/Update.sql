UPDATE
Parkings
SET 
name = @name, 
description = @description, 
stantionId = @stantionId,
UpdateDate = CURRENT_TIMESTAMP
WHERE
id = @id
