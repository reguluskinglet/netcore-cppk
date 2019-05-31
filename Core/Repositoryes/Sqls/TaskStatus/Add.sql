insert into TrainTaskStatuses
(Date, Status,TrainTaskId, UserId)
VALUES
(@date, @status,@trainTaskId,@userId)
SELECT SCOPE_IDENTITY()