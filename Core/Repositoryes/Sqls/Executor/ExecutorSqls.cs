namespace Rzdppk.Core.Repositoryes.Sqls.Executor
{
    public class ExecutorSqls
    {
        public string AddExecutorToTask = @"
            insert into TrainTasks
            (CarriageId, CreateDate, Description, EquipmentModelId, FaultId, TaskType, UserId)
            Values
            (@carrigeId, CURRENT_TIMESTAMP, @description, @equipmentModelId, @faultId, @taskType , @userId)
            SELECT SCOPE_IDENTITY()
        ";
        public string AddExecutorToTaskTimeShift = @"
            INSERT into
            TrainTaskExecutors
            (BrigadeType, Date, TrainTaskId, UserId, UpdateDate)
            VALUES
            (@brigade_type, GetDate(), @task_id, @user_id,CURRENT_TIMESTAMP - 0.00005)
            SELECT SCOPE_IDENTITY()
        ";

        //Аирибут по ид атрибута
        public string GetExecutorById = @"
            select * from TrainTaskExecutors where Id = @executorId
        ";
    }
}