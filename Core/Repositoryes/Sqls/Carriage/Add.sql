INSERT into
Carriages
(ModelId, Number, Serial, TrainId)
VALUES
(@model_id, @number, @serial, @train_id)
SELECT SCOPE_IDENTITY()