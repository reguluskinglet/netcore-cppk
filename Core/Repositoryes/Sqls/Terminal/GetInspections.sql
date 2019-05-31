SELECT RefId as Id, CheckListType, DateEnd, DateStart, Status, UpdateDate,TrainId,UserId
FROM  Inspections x
--WHERE x.UpdateDate>@date