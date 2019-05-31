using System.Collections.Generic;

namespace Core.Grid
{
    public interface IGridMapper<TEntity> where TEntity : class
    {
        IEnumerable<TEntity> Mapper(IEnumerable<TEntity> rows);
    }
}