SELECT te.*,u.*,b.*
FROM TrainTaskExecutors as te
LEFT JOIN TrainTasks t on t.Id=te.TrainTaskId
LEFT JOIN auth_users u on u.Id=te.UserId
LEFT JOIN Brigades b on b.Id=u.BrigadeId
WHERE t.Id=@task_id
ORDER BY te.Date ASC