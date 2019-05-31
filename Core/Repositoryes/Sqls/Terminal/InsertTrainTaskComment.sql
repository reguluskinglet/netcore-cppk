INSERT INTO [dbo].[TrainTaskComments]
           ([Date]
           ,[Text]
           ,[TrainTaskId]
           ,[UserId]
           ,[UpdateDate]
           ,[RefId])
     VALUES
           (@Date
           ,@Text
           ,@RefId
           ,@UserId
           ,@UpdateDate
           ,@Id)