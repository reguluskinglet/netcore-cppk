SELECT Id,OrderDate,Status, Count, Type, TrainName, Author, BrigadeType, EditDate, CreateDate, CarriageName, EquipmentName,HasInspection,TaskType
FROM View_TaskInspections t
WHERE ParentId IS NULL 
--and
