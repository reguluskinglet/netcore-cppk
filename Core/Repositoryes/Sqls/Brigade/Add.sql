INSERT into
Brigades
(name, description, brigadeType)
VALUES
(@name, @description, @brigadeType)
SELECT SCOPE_IDENTITY()