SELECT st.Id,st.InTime,st.OutTime,s.Name
FROM StantionOnTrips st
INNER JOIN Stantions s ON s.Id=st.StantionId
WHERE st.TripId=@id