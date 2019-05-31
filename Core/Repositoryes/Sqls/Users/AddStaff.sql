INSERT into
auth_users
(Name, PersonNumber, PersonPosition,RoleId,IsBlocked, BrigadeId)
VALUES
(@name, @personNumber, @personPosition,@roleId, @isBlocked, @brigadeId)
SELECT SCOPE_IDENTITY()