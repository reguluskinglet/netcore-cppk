UPDATE [dbo].[Inspections]
SET [DateEnd] = @DateEnd,[Status] = @Status,[UpdateDate] = @UpdateDate
WHERE Id=@RefId