insert into TaskPrintItems
(LabelId, TaskPrintId, TimePrinted, UserId)
values
(@labelId, @taskPrintId, @timePrinted, @userId)
SELECT SCOPE_IDENTITY()