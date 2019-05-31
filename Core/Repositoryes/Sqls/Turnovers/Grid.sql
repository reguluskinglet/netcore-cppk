SELECT t.Id,t.Name,d.Name as DirectionName,(SELECT COUNT(*) From Routes r WHERE r.TurnoverId=t.Id ) as RouteCount
FROM Turnovers t
INNER JOIN Directions d ON t.DirectionId=d.Id
OUTER APPLY(
            SELECT (
            SELECT dor.Day+''  FROM DayOfRoutes dor
            WHERE dor.TurnoverId = t.Id
            ORDER BY dor.Day
            FOR XML PATH('')) [days]
			) rs