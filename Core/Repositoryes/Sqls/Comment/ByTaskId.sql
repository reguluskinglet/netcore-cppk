SELECT tc.*,u.*,b.*
FROM TrainTaskComments as tc
LEFT JOIN TrainTasks t on t.Id=tc.TrainTaskId
LEFT JOIN auth_users u on u.Id=tc.UserId
LEFT JOIN Brigades b on b.Id=u.BrigadeId
WHERE t.Id=@task_id
ORDER BY tc.Date ASC