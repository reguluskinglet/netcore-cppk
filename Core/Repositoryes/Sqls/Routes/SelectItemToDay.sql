SELECT r.Id as Value, Name, (SELECT COUNT(*) FROM PlanedRouteTrains p WHERE p.RouteId=r.Id and p.Date between @DateFrom and @DateTo) as CountTodayRoute
FROM Routes r
ORDER BY CountTodayRoute DESC, Name