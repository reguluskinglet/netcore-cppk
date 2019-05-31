SELECT t.*,
CAST(
   CASE WHEN EXISTS(SELECT * FROM Carriages c WHERE c.TrainId=t.Id) THEN 0
   ELSE 1
   END 
AS BIT) as CanDelete, s.*
FROM Trains as t
LEFT JOIN Stantions as s ON t.StantionId = s.Id
ORDER BY t.Name
OFFSET @skip ROWS
FETCH NEXT @limit ROWS ONLY