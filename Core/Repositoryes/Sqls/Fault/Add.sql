INSERT into
Faults
(name, description, faultType)
VALUES
(@name, @description, @faultType)
SELECT SCOPE_IDENTITY()