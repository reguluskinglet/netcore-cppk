
MERGE [dbo].[Documents] AS target  
USING (SELECT @Description,@Name,@TrainTaskCommentId,@UpdateDate,@RefId,@DocumentType
      ) AS source (Description,Name,TrainTaskCommentId,UpdateDate,RefId,DocumentType)  
ON (target.RefId = source.RefId) 
WHEN MATCHED 
    THEN UPDATE SET target.TrainTaskCommentId = source.TrainTaskCommentId 
WHEN NOT MATCHED  
    THEN INSERT (Description,Name,TrainTaskCommentId,UpdateDate,RefId,DocumentType) VALUES(source.Description,source.Name,source.TrainTaskCommentId,source.UpdateDate,source.RefId,source.DocumentType) ;
