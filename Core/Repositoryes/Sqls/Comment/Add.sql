insert into TrainTaskComments
(Date, Text, TrainTaskId, UserId)
VALUES
(@date, @text, @trainTaskId, @userId)
SELECT SCOPE_IDENTITY()