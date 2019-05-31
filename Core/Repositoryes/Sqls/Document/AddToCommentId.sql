update Documents
set TrainTaskCommentId = @trainTaskCommentId,
UpdateDate = CURRENT_TIMESTAMP
where id = @id