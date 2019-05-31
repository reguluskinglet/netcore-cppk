update StantionTrips
set 
BrigadeFromId = @brigadeFromId,
BrigadeToId = @brigadeToId,
UpdateDate = CURRENT_TIMESTAMP
where id = @id