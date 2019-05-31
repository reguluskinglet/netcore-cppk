namespace Rzdppk.Core.Repositoryes.Sqls.TaskStatus
{
    public class TaskStatusSqls
    {
        public string AddStatusToTask = @"
            insert into TrainTaskStatuses
            (Date, Status,TrainTaskId, UserId)
            VALUES
            (@date, @status,@trainTaskId,@userId)
            SELECT SCOPE_IDENTITY()
        ";
        public string AddStatusToTaskTimeShift = @"
            insert into TrainTaskStatuses
            (Date, Status,TrainTaskId, UserId, UpdateDate)
            VALUES
            (@date, @status,@trainTaskId,@userId, CURRENT_TIMESTAMP - 0.00005)
            SELECT SCOPE_IDENTITY()
        ";
    }
}