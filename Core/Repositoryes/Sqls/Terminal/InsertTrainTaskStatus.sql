INSERT INTO [dbo].[TrainTaskStatuses]([Date],[Status],[TrainTaskId],[UserId],[UpdateDate],[RefId])
VALUES(@Date,@Status,@RefId,@UserId,@UpdateDate,@Id)
select SCOPE_IDENTITY()