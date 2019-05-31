using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Grid
{
    public class GridColumn<TEntity> : IGridColumn<TEntity> where TEntity : class
    {
        private Type _dataType;
        public string SystemName { get; set; }
        public string SqlName { get; set; }
        public ColumnJoin? ColumnJoin { get; set; }
        public string OrderByName { get; private set; }

        private readonly Func<TEntity, object> _columnValueFunc;

        public bool IsDefault { get; set; }

        public GridColumn()
        {
        }

        public GridColumn(Func<TEntity, object> columnValueFunc, string systemName, Type type)
        {
            SystemName = systemName;
            _columnValueFunc = columnValueFunc;
            _dataType = type;
        }

        public IGridColumn<TEntity> Named(string name)
        {
            DisplayName = name;
            return this;
        }

        public string DisplayName { get; private set; }
        public int? Width { get; set; }

        public IGridColumn<TEntity> SetWidth(int persent)
        {
            Width = persent;
            return this;
        }

        public IGridColumn<TEntity> Default()
        {
            IsDefault = true;
            return this;
        }

        public object GetValue(TEntity instance)
        {
            var value= _columnValueFunc(instance);

            if (value != null)
                return value;

            return null;
        }

        public IGridColumn<TEntity> LeftJoin(string tableName, string forigenKey, string property ="Name")
        {
            ColumnJoin = new ColumnJoin(tableName, forigenKey, property);

            return this;
        }

        public IGridColumn<TEntity> OrderBy(string orderName)
        {
            OrderByName = orderName;

            return this;
        }
    }
}
