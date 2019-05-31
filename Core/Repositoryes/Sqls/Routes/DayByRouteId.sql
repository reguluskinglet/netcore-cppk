select top(1) trips.Day from Routes as routes
left join TripRoutes as tripRoutes ON routes.id = tripRoutes.RouteId
left join Trips as trips ON tripRoutes.TripId = trips.Id
where routes.id = @id
