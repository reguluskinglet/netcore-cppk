SELECT x.RefId as Id, Date, Status, t.RefId as TrainTaskId, x.UserId, x.UpdateDate
FROM  TrainTaskStatuses x
INNER JOIN TrainTasks t ON t.Id=x.TrainTaskId
--WHERE x.UpdateDate>@date