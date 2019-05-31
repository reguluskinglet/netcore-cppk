UPDATE
auth_users
SET
BrigadeId = @brigadeId,
IsBlocked = @isBlocked,
Login = @login,
Name = @name,
PasswordHash = @passwordHash,
PersonNumber = @personNumber,
PersonPosition = @personPosition,
RoleId = @roleId,
UpdateDate = CURRENT_TIMESTAMP
WHERE
id = @id


