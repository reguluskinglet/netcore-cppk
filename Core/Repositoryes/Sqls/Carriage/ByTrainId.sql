SELECT c.*,
CAST(
   CASE WHEN EXISTS(SELECT * FROM Labels l WHERE l.CarriageId=c.Id) THEN 0
   ELSE 1
   END 
AS BIT) as CanDelete,
m.*
FROM Carriages as c
LEFT JOIN Models as m ON c.ModelId = m.Id
WHERE c.TrainId=@train_id
ORDER BY c.Number