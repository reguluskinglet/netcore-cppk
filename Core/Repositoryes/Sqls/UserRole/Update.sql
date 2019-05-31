UPDATE
auth_roles
SET 
Name = @name, 
Permissions = @permissions,
UpdateDate = CURRENT_TIMESTAMP
WHERE
id = @id
