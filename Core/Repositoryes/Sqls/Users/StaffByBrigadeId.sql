SELECT u.Id,u.Name,u.PersonNumber, u.PersonPosition, u.BrigadeId,b.*
FROM auth_users u
LEFT JOIN Brigades b ON b.Id=u.BrigadeId
WHERE u.BrigadeId=@brigade_id