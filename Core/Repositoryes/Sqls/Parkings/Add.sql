INSERT into
Parkings
(Name, Description, StantionId)
VALUES
(@name, @description, @stantionId)
SELECT SCOPE_IDENTITY()