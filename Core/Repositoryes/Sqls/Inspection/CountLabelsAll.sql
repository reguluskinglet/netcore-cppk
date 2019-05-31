select count(*) from Meterage m
left join Labels l on l.Id=m.LabelId
WHERE l.Id is not null