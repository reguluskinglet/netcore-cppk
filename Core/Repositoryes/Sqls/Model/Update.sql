UPDATE
Models
SET 
name = @name, 
description = @description, 
modeltype = @modeltype,
UpdateDate = CURRENT_TIMESTAMP
WHERE
id = @id
