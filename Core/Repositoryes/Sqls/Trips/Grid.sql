SELECT t.Id,t.Name,t.TripType
FROM Trips t
OUTER APPLY(
            SELECT (
            SELECT dot.Day+''  FROM DayOfTrips dot
            WHERE dot.TripId = t.Id
            ORDER BY dot.Day
            FOR XML PATH('')) [Days]
			) r
OUTER APPLY(SELECT (
			SELECT 
					CASE   
					   WHEN sub.minv = sub.OutTime THEN '(' + FORMAT(sub.OutTime,'HH:mm') + ') ' + sub.Name + ' -' 
					   WHEN sub.maxv = sub.OutTime THEN  ' ' + sub.Name +' (' + FORMAT(sub.InTime,'HH:mm') + ')'
					END   
			 FROM 
					(
					 SELECT st.Id,st.InTime,st.OutTime,s.Name,
					 MIN(st.OutTime) OVER(PARTITION BY	st.TripId) minv,
					 MAX(st.OutTime) OVER(PARTITION BY	st.TripId) maxv
					 FROM StantionOnTrips st
					 INNER JOIN Stantions s ON s.Id=st.StantionId
					 WHERE st.TripId=t.Id
					) sub
			WHERE sub.minv = sub.OutTime OR sub.maxv = sub.OutTime
			ORDER BY sub.OutTime
			FOR XML PATH('')
			) [route]
) rou