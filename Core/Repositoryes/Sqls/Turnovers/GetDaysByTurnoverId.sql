SELECT day.Day
FROM Turnovers t
INNER JOIN DayOfRoutes day ON day.TurnoverId=t.Id
WHERE t.Id=@id