select stantionTrips.* from Routes as routes
left join TripRoutes as tripRoutes ON routes.id = tripRoutes.RouteId
left join Trips as trips ON tripRoutes.TripId = trips.Id
left join StantionTrips as stantionTrips ON stantionTrips.TripId = trips.Id
where 
stantionTrips.BrigadeToId is null AND
routes.id = @id
order by RouteId, StantionTrips.TripId, StantionTrips.Number
