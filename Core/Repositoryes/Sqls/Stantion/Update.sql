UPDATE
Stantions
SET 
Name = @name, 
Description = @description,
StantionType = @type,
UpdateDate = CURRENT_TIMESTAMP
WHERE
id = @id