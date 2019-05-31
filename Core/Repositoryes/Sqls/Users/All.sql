SELECT *
FROM auth_users u
INNER JOIN auth_roles r ON r.Id=u.RoleId
LEFT JOIN Brigades b ON b.Id=u.BrigadeId
WHERE u.Login=@login