INSERT into
Documents
(Name, Description, TrainTaskCommentId, DocumentType)
VALUES
(@name, @description, @comment_id, @documentType)
SELECT SCOPE_IDENTITY()