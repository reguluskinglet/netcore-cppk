using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace Rzdppk.Core.Repositoryes.Base
{
    public class TransactionRepository
    {
        public SqlTransaction Transaction;

        readonly SqlConnection _connection;

        public TransactionRepository(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
        }

        public TransactionRepository(SqlConnection connection)
        {
            _connection = connection;
        }

        public void BeginTransaction()
        {
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            Transaction = _connection.BeginTransaction();
            
        }

        public void BeginTransactionIso(IsolationLevel lvl)
        {
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            Transaction = _connection.BeginTransaction(lvl);
        }


        public SqlConnection Connection
        {
            get { return _connection; }
        }

        /// <summary>
        /// Принять изменения
        /// </summary>
        public void CommitTransaction()
        {
            if (Transaction != null)
            {
                try
                {
                    Transaction.Commit();
                }
                finally
                {
                    if (_connection != null && _connection.State == ConnectionState.Open)
                    {
                        _connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Откатить изменения
        /// </summary>
        public void RollBackTransaction()
        {
            if (Transaction != null)
            {
                if (_connection.State == ConnectionState.Open)
                {
                    try
                    {
                        Transaction.Rollback();
                    }
                    finally
                    {
                        if (_connection != null && _connection.State == ConnectionState.Open)
                        {
                            _connection.Close();
                        }
                    }
                }
            }
        }

        public TResult QueryFirst<TResult>(string sql, object data = null)
        {
            return _connection.QueryFirstOrDefault<TResult>(sql, data, Transaction);
        }

        public IEnumerable<TResult> Query<TResult>(string sql, object data = null)
        {
            return _connection.Query<TResult>(sql, data, Transaction);
        }

        public void Insert<TEntity>(List<TEntity> entities, string sql)
        {
            _connection.Execute(sql, entities, Transaction);
        }

        public int Insert(string sql, object data)
        {
            return _connection.QueryFirstOrDefault<int>(sql, data, Transaction);
        }

        public void Execute<TEntity>(string sql, TEntity entity)
        {
            _connection.Execute(sql, entity, Transaction);
        }
    }
}
