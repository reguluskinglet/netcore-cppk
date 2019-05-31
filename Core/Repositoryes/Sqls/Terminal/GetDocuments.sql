SELECT x.RefId as Id, Description, Name, c.RefId as TrainTaskCommentTerminalId, x.UpdateDate,DocumentType
FROM  Documents x
LEFT JOIN TrainTaskComments c ON c.Id=x.TrainTaskCommentId
WHERE (x.DocumentType=1 or x.DocumentType=2) --and
--WHERE x.UpdateDate>@date