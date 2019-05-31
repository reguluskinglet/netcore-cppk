SELECT p.*, s.*
FROM Parkings AS p
LEFT JOIN Stantions as s ON p.StantionId = s.Id
ORDER by p.Name
OFFSET @skip ROWS
FETCH NEXT @limit ROWS ONLY;