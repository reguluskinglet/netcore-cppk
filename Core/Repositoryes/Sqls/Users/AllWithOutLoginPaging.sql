SELECT *
FROM auth_users
where Login is null
ORDER BY id
OFFSET @skip ROWS
FETCH NEXT @limit ROWS ONLY;