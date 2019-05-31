INSERT into
TrainTaskExecutors
(BrigadeType, Date, TrainTaskId, UserId)
VALUES
(@brigade_type, GetDate(), @task_id, @user_id)
SELECT SCOPE_IDENTITY()