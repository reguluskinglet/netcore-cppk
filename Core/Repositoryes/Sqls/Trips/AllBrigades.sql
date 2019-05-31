select 
routes.Name as RouteName, routes.Id as RouteId, 
stantionTrips.BrigadeFromId, stantionTrips.BrigadeToId, stantionTrips.HourFrom, stantionTrips.MinuteFrom,
stantionTrips.HourTo, stantionTrips.MinuteTo, stantionTrips.Id as stantionTripId, StantionTrips.TripId as TripId, StantionTrips.Number,
trips.Day,
stations.name as StationName
from Trips as trips
left join StantionTrips as stantionTrips ON stantionTrips.TripId = trips.Id
left join TripRoutes as tripRoutes ON tripRoutes.TripId = trips.Id
left join Routes as routes ON routes.id = tripRoutes.RouteId
left join Stantions as stations ON stantionTrips.StantionId = stations.Id
--where BrigadeFromId is not null
order by routes.Name, StantionTrips.TripId, StantionTrips.Number
--OFFSET @skip ROWS
--FETCH NEXT @limit ROWS ONLY;