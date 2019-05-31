INSERT into
auth_roles
(Name, Permissions)
VALUES
(@name, @permissions)
SELECT SCOPE_IDENTITY()