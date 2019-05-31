select stantionTrips.* from Routes as routes
left join TripRoutes as tripRoutes ON routes.id = tripRoutes.RouteId
left join Trips as trips ON tripRoutes.TripId = trips.Id
left join StantionTrips as stantionTrips ON stantionTrips.TripId = trips.Id
where 
routes.id = @id
order by tripId, number
