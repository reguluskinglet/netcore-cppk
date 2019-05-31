UPDATE  auth_users
SET Login=NULL, PasswordHash=NULL, IsBlocked=1 
WHERE Id=@id