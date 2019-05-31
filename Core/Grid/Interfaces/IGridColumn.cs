using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Grid
{
    public interface IGridColumn<TEntity>
    {
        IGridColumn<TEntity> Named(string name);

        IGridColumn<TEntity> SetWidth(int persent);

        IGridColumn<TEntity> Default();

        IGridColumn<TEntity> LeftJoin(string tableName, string forigenKey, string property = "Name");

        IGridColumn<TEntity> OrderBy(string orderName);
    }
}
