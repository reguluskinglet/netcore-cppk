UPDATE
Trains
SET 
Name = @name, 
Description = @description,
StantionId = @stantion_id,
UpdateDate = CURRENT_TIMESTAMP
WHERE
id = @id