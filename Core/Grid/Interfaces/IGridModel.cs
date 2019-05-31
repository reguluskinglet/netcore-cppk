using System.Collections.Generic;
using Core.QueryBuilders;

namespace Core.Grid
{
    public interface IGridModel<TEntity> where TEntity : class 
    {
        ColumnBuilder<TEntity> Column { get; }
        string GridSql { get; }

        string RowCountSql { get; }
    }
}