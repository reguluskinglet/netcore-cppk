SELECT u.*, b.*
FROM auth_users u
LEFT JOIN Brigades b ON b.Id=u.BrigadeId
ORDER BY u.id
OFFSET @skip ROWS
FETCH NEXT @limit ROWS ONLY;