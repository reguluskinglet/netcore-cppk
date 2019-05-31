select 
count (*)
from Trips as trips
left join StantionTrips as stantionTrips ON stantionTrips.TripId = trips.Id
left join TripRoutes as tripRoutes ON tripRoutes.TripId = trips.Id
left join Routes as routes ON routes.id = TripRoutes.RouteId
left join Stantions as stations ON stantionTrips.StantionId = stations.Id
where BrigadeFromId is not null