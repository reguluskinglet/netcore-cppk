select routes.* from TripRoutes as tripRoutes
LEFT JOIN Routes as routes ON tripRoutes.RouteId = routes.Id
where tripRoutes.TripId = @id
