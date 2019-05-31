UPDATE
auth_users
SET
Name = @name, 
PersonNumber = @personNumber,
PersonPosition = @personPosition,
RoleId = @roleId,
IsBlocked = @isBlocked,
BrigadeId = @brigadeId,
UpdateDate = CURRENT_TIMESTAMP
WHERE
id = @id


