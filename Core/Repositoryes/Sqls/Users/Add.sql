INSERT into
auth_users
(BrigadeId, IsBlocked, Login, Name, PasswordHash, PersonNumber, PersonPosition, RoleId)
VALUES
(@brigadeId, @isBlocked, @login, @name, @passwordHash, @personNumber, @personPosition, @roleId)
SELECT SCOPE_IDENTITY()