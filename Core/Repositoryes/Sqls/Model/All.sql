select * from Models order by Id
OFFSET @skip ROWS
FETCH NEXT @limit ROWS ONLY;