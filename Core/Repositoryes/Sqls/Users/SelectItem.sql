SELECT Id as Value, Name as Text
FROM auth_users u
WHERE Name IS NOT NULL
ORDER BY Name