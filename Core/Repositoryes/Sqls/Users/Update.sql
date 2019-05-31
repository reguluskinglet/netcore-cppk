UPDATE
auth_users
SET
BrigadeId = @brigadeId,
IsBlocked = @isBlocked,
Login = @login,
Name = @name,
PersonNumber = @personNumber,
PersonPosition = @personPosition,
RoleId = @roleId,
UpdateDate = CURRENT_TIMESTAMP
WHERE
id = @id


