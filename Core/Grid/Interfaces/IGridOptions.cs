using System.Collections.Generic;
using Core.Grid.Filter;
using Rzdppk.Core.Grid;

namespace Core.Grid
{

    public interface IGridOptions
    {
        IList<GridColumnInfo> VisibleColumns { get; set; }
        GridSortOptions SortOptions { get; set; }
        List<string> VisibleFilters { get; set; }
        void SetSortOptionsDefault();
        int Page { get; set; }
        int PageSize { get; set; }

        bool IsSelectedAll { get; set; }
        List<GridSelectedRow> SelectedRows { get; set; }
        GridSelectedRow SelectedRow { get; set; }
        void SetSelectedRowsDefault();
    }
}