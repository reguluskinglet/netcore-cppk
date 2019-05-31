SELECT x.RefId as Id, BrigadeType, Date, t.RefId as TrainTaskId,x.UserId, x.UpdateDate
FROM  TrainTaskExecutors x
LEFT JOIN TrainTasks t ON t.Id=x.TrainTaskId
--WHERE x.UpdateDate>@date