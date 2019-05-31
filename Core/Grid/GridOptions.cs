using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Grid.Filter;
using Core.QueryBuilders;
using Rzdppk.Core.Grid;

namespace Core.Grid
{
    public class GridOptions : IGridOptions
    {
        public IList<GridColumnInfo> VisibleColumns { get; set; }
        public List<string> VisibleFilters { get; set; }

        public GridSortOptions SortOptions { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public bool IsSelectedAll { get; set; }
        public List<GridSelectedRow> SelectedRows { get; set; }
        public GridSelectedRow SelectedRow { get; set; }
        public List<List<string>> SplitSelectedRows { get; set; }

        public GridOptions()
        {
            Page = 1;
            PageSize = 50;
            VisibleColumns = new List<GridColumnInfo>();
            SetSortOptionsDefault();
            IsSelectedAll = false;
            SetSelectedRowsDefault();
        }

        public virtual void SetSortOptionsDefault()
        {
            SortOptions = new GridSortOptions { Column = "Id", Direction = SortDirection.Desc };
        }

        public virtual void SetSelectedRowsDefault()
        {
            IsSelectedAll = false;
            SelectedRows = new List<GridSelectedRow>();
        }

        //public virtual bool IsSelectedRow(string rowId)
        //{
        //    if (SelectedRow != null && SelectedRow.RowId.Equals(rowId))
        //        return SelectedRow.IsSelected;

        //    return IsSelectedAll;
        //}
    }
}
