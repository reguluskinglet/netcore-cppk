UPDATE
Faults
SET 
name = @name, 
description = @description,
faultType = @faultType,
UpdateDate = CURRENT_TIMESTAMP
WHERE
id = @id
