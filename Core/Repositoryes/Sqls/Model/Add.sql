INSERT into
Models
(name, description, modeltype)
VALUES
(@name, @description, @model_type)
SELECT SCOPE_IDENTITY()