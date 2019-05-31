using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Rzdppk.Core.Repositoryes.Base.Interface
{
    public interface IDb : IDisposable
    {
        SqlConnection Connection { get; }
        TransactionRepository Transaction { get; }
        Task<IEnumerable<TEntity>> GetAll<TEntity>(string tableName = null);
        Task<List<TEntity>> GetAllWithFilter<TEntity>(DateTime? date, string tableName = null);
        void Start(Action<SqlTransaction, SqlConnection> action);
    }
}