INSERT into
Trains
(Name, Description, StantionId)
VALUES
(@name, @description, @stantion_id)
SELECT SCOPE_IDENTITY()