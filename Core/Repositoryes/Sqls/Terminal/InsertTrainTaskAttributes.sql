INSERT INTO [dbo].[TrainTaskAttributes]([CheckListEquipmentId],[FaultId],[InspectionId],[RefId],[TaskLevel],[TrainTaskId],[UpdateDate],[UserId],[Value],[Description])
VALUES (@CheckListEquipmentId,@FaultId,@InspectionRefId,@Id,@TaskLevel,@TrainTaskRefId,@UpdateDate,@UserId,@Value,@Description)
select SCOPE_IDENTITY()