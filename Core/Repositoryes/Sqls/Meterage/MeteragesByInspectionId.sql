SELECT *
FROM Meterage m
WHERE m.InspectionId=@inspection_id AND m.Value IS NOT NULL
ORDER BY m.Date ASC