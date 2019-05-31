INSERT into
Stantions
(Name, Description, StantionType)
VALUES
(@name, @description, @type)
SELECT SCOPE_IDENTITY()