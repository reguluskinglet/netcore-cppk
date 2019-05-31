select * from Faults order by [Name]
OFFSET @skip ROWS
FETCH NEXT @limit ROWS ONLY;