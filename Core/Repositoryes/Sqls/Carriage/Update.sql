UPDATE
Carriages
SET 
ModelId = @model_id, 
Number = @number,
Serial = @serial,
TrainId = @train_id,
UpdateDate = CURRENT_TIMESTAMP
WHERE
id = @id