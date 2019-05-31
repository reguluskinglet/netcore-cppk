SELECT d.*
FROM Documents d
LEFT JOIN TrainTaskComments c ON c.Id=d.TrainTaskCommentId
LEFT JOIN TrainTasks t ON t.Id=c.TrainTaskId
WHERE t.id=@task_id
ORDER BY d.TrainTaskCommentId ASC, d.Description ASC