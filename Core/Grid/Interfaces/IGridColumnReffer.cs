using System;
using System.Linq.Expressions;

namespace Core.Grid
{
    public interface IGridColumnReffer<TEntity>
    {
        IGridColumn<TEntity> As(Expression<Func<TEntity, object>> propertySpecifier);
    }
}