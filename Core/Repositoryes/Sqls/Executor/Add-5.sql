INSERT into
TrainTaskExecutors
(BrigadeType, Date, TrainTaskId, UserId, UpdateDate)
VALUES
(@brigade_type, GetDate(), @task_id, @user_id,CURRENT_TIMESTAMP - 0.00005)
SELECT SCOPE_IDENTITY()