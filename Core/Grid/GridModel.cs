using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Core.QueryBuilders;

namespace Core.Grid
{
    public class GridModel<TEntity> : IGridModel<TEntity> where TEntity : class
    {
        private readonly ColumnBuilder<TEntity> _columnBuilder;


        public GridModel(string sql, string rowCountSql = null)
        {
            GridSql = sql;
            RowCountSql = rowCountSql;

            _columnBuilder = new ColumnBuilder<TEntity>();
        }

        public string GridSql { get;}

        public string RowCountSql { get; }

        public ColumnBuilder<TEntity> Column
        {
            get { return _columnBuilder; }
        }
    }
}
