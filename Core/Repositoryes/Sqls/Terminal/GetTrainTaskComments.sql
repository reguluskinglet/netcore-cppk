SELECT c.RefId as Id, Date, Text,t.RefId as TrainTaskId,c.UserId, c.UpdateDate
FROM  TrainTaskComments c
INNER JOIN TrainTasks t ON t.Id=c.TrainTaskId
--WHERE c.UpdateDate>@date