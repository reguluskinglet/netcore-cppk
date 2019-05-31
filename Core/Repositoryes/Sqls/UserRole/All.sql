select * from auth_roles 
order by id
--LEFT JOIN TripRoutes as tripRoutes ON trips.id = tripRoutes.TripId
--LEFT JOIN StantionTrips as stantionTrips ON stantionTrips.id = tripRoutes.TripId

OFFSET @skip ROWS
FETCH NEXT @limit ROWS ONLY;