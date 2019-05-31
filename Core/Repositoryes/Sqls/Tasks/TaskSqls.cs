namespace Rzdppk.Core.Repositoryes.Sqls.Tasks
{
    public class TaskSqls
    {
        public string AddTaskWithoutAttributes = @"
            insert into TrainTasks
            (CarriageId, CreateDate, Description, EquipmentModelId, TaskType, UserId)
            Values
            (@carriageId, CURRENT_TIMESTAMP, @description, @equipmentModelId, @taskType , @userId)
            SELECT SCOPE_IDENTITY()
        ";

        public string GetTaskWithoutAttributes = @"
            select * from TrainTasks where id = @id
        ";

        public string AddAtributeToTask = @"
            insert into TrainTaskAttributes
            (TrainTaskId, FaultId, UserId, TaskLevel)
            Values
            (@TrainTaskId, @FaultId, @userId, @taskLevel)
            SELECT SCOPE_IDENTITY()
        ";

    


        //Атрибуты по ид таска
        public string GetTrainTaskAttributes = @"
            select * from TrainTaskAttributes where TrainTaskId = @trainTaskId
        ";

        //Аирибут по ид атрибута
        public string GetTrainTaskAttributeById = @"
            select * from TrainTaskAttributes where Id = @attributeId
        ";

        //Аирибут по ид атрибута
        public string GetAllTrainTask = @"
            select * from TrainTasks
        ";

        public string GetTrainTaskByIdAndAttributeId(int taskId)
        {
            return  $@"
        select
            traintask.*,
        users.Name as InitiatorName,
        carriages.Number as carriageNumber, Carriages.Serial as carriageSerial,
        trains.Name as trainName,
        faults.FaultType as faultTypeId, 
		faults.Name as faultName,
        equipments.name as equipmentName,
        models.ModelType as carriageModelTypeId,
        attrib.TaskLevel as tasklevel, 
		attrib.id as attributeId,
		attrib.UpdateDate as attributeUpdateDate,
		attrib.UserId as attributeUserId,
		attrib.InspectionId as attributeInspectionId,
		attrib.faultId as attributeFaultId,
		attrib.CheckListEquipmentId as attributeCheckListEquipmentId,
		attrib.Value as attributeValue,
        cle.NameTask as CheckListEquipmentTaskName,
		cle.ValueType as CheckListEquipmentValueType,
		cle.Value as CheckListEquipmentValue

            from TrainTasks as traintask
            LEFT JOIN auth_users AS users ON traintask.UserId = users.Id
			LEFT JOIN Carriages AS carriages ON traintask.CarriageId = carriages.Id
            LEFT JOIN Trains AS trains ON carriages.TrainId = trains.Id
            LEFT JOIN TrainTaskAttributes as attrib ON attrib.TrainTaskId = traintask.id
			LEFT JOIN Faults AS faults ON attrib.FaultId = faults.id
            LEFT JOIN EquipmentModels AS equipmentmodels ON traintask.EquipmentModelId = equipmentmodels.Id
            LEFT JOIN Equipments as equipments ON equipmentmodels.EquipmentId = equipments.Id
            LEFT JOIN Models as models ON carriages.ModelId = models.Id
            LEFT JOIN CheckListEquipments as cle ON cle.Id = attrib.CheckListEquipmentId
            WHERE traintask.id = {taskId}
        ";
        }



    }
}