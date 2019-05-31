using System.Text.RegularExpressions;
using Core.QueryBuilders;

namespace Core.Grid
{
	public class LazyPagination
	{
	    public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int RowCount { get; }
        private const int LeftSidePage = 3;

        public LazyPagination(int rowCount, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            RowCount = rowCount;
        }

	    public int CurrentPage
	    {
	        get
	        {
	            return PageNumber;
	        }
	    }

	    public int TotalPagesCount
	    {
	        get
	        {
	            var pagesCount = RowCount / PageSize;

                if (RowCount % PageSize > 0)
                    pagesCount++;

	            return pagesCount;
	        }
	    }

	    public bool HasSides
	    {
	        get
	        {
	            return TotalPagesCount >= 11;
	        }
	    }

	    public int TotalElementsCount
	    {
	        get
	        {
                return RowCount;
	        }
	    }

	    public int FirstElementOnPage
	    {
	        get
	        {
	            return PageSize*(CurrentPage - 1) + 1;
	        }
	    }

	    public int LastElementOnPage
	    {
	        get
	        {
	            return CurrentPage == TotalPagesCount ? TotalElementsCount : PageSize * CurrentPage;
	        }
	    }

	    private int RightSidePage
	    {
	        get
	        {
                return TotalPagesCount - 2;
	        }
	    }

        public int FirstPageInPagesFrame
        {
            get
            {
                if (!HasSides) 
                    return 1;

                if (CurrentPage <= (LeftSidePage + 3))
                    return LeftSidePage + 1;

                if (CurrentPage >= (RightSidePage - 3))
                    return RightSidePage - 5;

                return CurrentPage - 2;
            }
        }

	    public int LastPageInPagesFrame
	    {
	        get
	        {
                if (!HasSides)
                    return TotalPagesCount;

                if (CurrentPage <= (LeftSidePage + 3))
                    return LeftSidePage + 5;

                if (CurrentPage >= (RightSidePage - 3))
                    return RightSidePage - 1;

                return CurrentPage + 2;
	            
	        }
	    }

	    public string GetOffestRow()
        {
            int numberToSkip = (PageNumber - 1) * PageSize;

            return new OffsetRowBuilder(numberToSkip, PageSize).QueryResult;
	    }

	    public string SetOffset(string sql)
        {
            var pattern = new Regex(@"--OFFSET.*");

            return pattern.Replace(sql, GetOffestRow());
        }
    }

    public class LazyPagination<TData> :LazyPagination
    {
        public LazyPagination(TData data, int rowCount, int pageNumber, int pageSize) 
            : base(rowCount, pageNumber, pageSize)
        {
            Data = data;
        }

        public TData Data { get; set; }
    }
}
