SELECT ts.*,u.*,b.*
FROM TrainTaskStatuses as ts
LEFT JOIN TrainTasks t on t.Id=ts.TrainTaskId
LEFT JOIN auth_users u on u.Id=ts.UserId
LEFT JOIN Brigades b on b.Id=u.BrigadeId
WHERE t.Id=@task_id
ORDER BY ts.Date ASC