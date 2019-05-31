SELECT *
FROM Documents as d
LEFT JOIN TrainTaskComments as tts ON tts.Id = d.TrainTaskCommentId
WHERE d.id=@id