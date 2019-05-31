update StantionTrips
set 
CheckListType = @checkListType,
UpdateDate = CURRENT_TIMESTAMP
where id = @id