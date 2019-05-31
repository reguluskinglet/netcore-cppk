SELECT u.Id,u.Name,u.BrigadeId,b.*
FROM auth_users u
LEFT JOIN Brigades b ON b.Id=u.BrigadeId
WHERE u.Id=@user_id