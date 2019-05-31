INSERT INTO [dbo].[Inspections]([CheckListType],[DateEnd],[DateStart],[Status],[UpdateDate],[TrainId],[RefId],[UserId])
VALUES (@CheckListType,@DateEnd,@DateStart,@Status,@UpdateDate,@TrainId,@Id,@UserId)
select SCOPE_IDENTITY()