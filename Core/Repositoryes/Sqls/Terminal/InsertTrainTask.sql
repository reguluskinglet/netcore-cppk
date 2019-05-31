INSERT INTO [dbo].[TrainTasks]([CarriageId],[CreateDate],[Description],[EquipmentModelId],[RefId],[TaskType],[UpdateDate],[UserId])
VALUES (@CarriageId,@CreateDate,@Description,@EquipmentModelId,@Id,@TaskType,@UpdateDate,@UserId)
select SCOPE_IDENTITY()