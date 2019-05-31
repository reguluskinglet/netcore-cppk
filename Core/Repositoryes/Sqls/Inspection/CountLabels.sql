select count(*) from Meterage m
left join Labels l on l.Id=m.LabelId
WHERE m.InspectionId=@inspection_id and l.Id is not null