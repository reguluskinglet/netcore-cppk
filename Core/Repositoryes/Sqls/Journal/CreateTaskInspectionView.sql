create view View_TaskInspections as
SELECT *
FROM (
SELECT i.Id,i.Status,i.CheckListType as Type,tr.Name as TrainName,tr.Id as TrainId,
STUFF((SELECT ',' + convert(varchar(10),c.Number) From TrainTaskAttributes a 
INNER JOIN TrainTasks task ON task.Id=a.TrainTaskId
INNER JOIN Carriages c ON c.Id=task.CarriageId
WHERE a.InspectionId=i.Id 
GROUP BY c.Number
ORDER BY c.Number
FOR XML PATH('')), 1, 1, '')
as CarriageName, NULL as CarriageId,NULL as EquipmentId, NULL as EquipmentName,u.Id as UserId, u.Name as Author,i.DateStart as CreateDate,i.DateEnd as EditDate,
b.BrigadeType,
(SELECT MAX(d) FROM (VALUES (i.DateStart), (i.DateEnd)) AS v (d)) AS OrderDate,
(SELECT COUNT(*) From TrainTaskAttributes a WHERE a.InspectionId=i.Id ) as Count,
NULL as ParentId,
CAST(1 AS bit) as HasInspection,
0 as TaskLevel,
NULL as TaskType
FROM Inspections i
INNER JOIN Trains tr ON tr.Id=i.TrainId
INNER JOIN auth_users u ON u.Id=i.UserId
INNER JOIN Brigades b ON b.Id=u.BrigadeId
UNION ALL
SELECT t.Id,
(SELECT TOP(1) s.Status FROM TrainTaskStatuses s WHERE s.TrainTaskId=t.Id ORDER BY s.Date DESC) as Status,
99 as Type,tr.Name as TrainName, tr.Id as TrainId,c.Serial as CarriageName,t.CarriageId,e.Id as EquipmentId,e.Name as EquipmentName,u.Id as UserId,u.Name as Author,t.CreateDate,t.EditDate,
(SELECT TOP(1) BrigadeType FROM TrainTaskExecutors ex WHERE ex.TrainTaskId=t.Id ORDER BY ex.Date DESC) as BrigadeType,
(SELECT MAX(d) FROM (VALUES (t.CreateDate), (t.EditDate)) AS v (d)) AS OrderDate,
NULL as Count,
i.Id as ParentId,
CAST(0 AS bit) as HasInspection,
(SELECT TOP(1) atr.TaskLevel FROM TrainTaskAttributes atr WHERE atr.TrainTaskId=t.Id and atr.TaskLevel IS NOT NULL ORDER BY atr.UpdateDate DESC) as TaskLevel,
t.TaskType
FROM Inspections i
INNER JOIN TrainTaskAttributes a ON a.InspectionId=i.Id
INNER JOIN TrainTasks t ON t.Id=a.TrainTaskId
INNER JOIN auth_users u ON u.Id=t.UserId
INNER JOIN Carriages c ON c.Id=t.CarriageId
INNER JOIN Trains tr ON tr.Id=c.TrainId
INNER JOIN EquipmentModels em ON em.Id=t.EquipmentModelId
INNER JOIN Equipments e ON e.Id=em.EquipmentId
UNION ALL
SELECT t.Id,
(SELECT TOP(1) s.Status FROM TrainTaskStatuses s WHERE s.TrainTaskId=t.Id ORDER BY s.Date DESC) as Status,
99 as Type,tr.Name as TrainName, tr.Id as TrainId,c.Serial as CarriageName,t.CarriageId, e.Id as EquipmentId, e.Name as EquipmentName,u.Id as UserId,u.Name as Author,t.CreateDate,t.EditDate,
(SELECT TOP(1) BrigadeType FROM TrainTaskExecutors ex WHERE ex.TrainTaskId=t.Id ORDER BY ex.Date DESC) as BrigadeType,
(SELECT MAX(d) FROM (VALUES (t.CreateDate), (t.EditDate)) AS v (d)) AS OrderDate,
NULL as Count,
NULL as ParentId,
CAST(0 AS bit) as HasInspection,
(SELECT TOP(1) a.TaskLevel FROM TrainTaskAttributes a WHERE a.TrainTaskId=t.Id and a.TaskLevel IS NOT NULL ORDER BY a.UpdateDate DESC) as TaskLevel,
t.TaskType
FROM TrainTasks t
INNER JOIN auth_users u ON u.Id=t.UserId
INNER JOIN Carriages c ON c.Id=t.CarriageId
INNER JOIN Trains tr ON tr.Id=c.TrainId
INNER JOIN EquipmentModels em ON em.Id=t.EquipmentModelId
INNER JOIN Equipments e ON e.Id=em.EquipmentId
) as x