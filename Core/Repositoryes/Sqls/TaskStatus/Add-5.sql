insert into TrainTaskStatuses
(Date, Status,TrainTaskId, UserId, UpdateDate)
VALUES
(@date, @status,@trainTaskId,@userId, CURRENT_TIMESTAMP - 0.00005)
SELECT SCOPE_IDENTITY()