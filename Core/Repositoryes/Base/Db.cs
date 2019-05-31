using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Sqls;

namespace Rzdppk.Core.Repositoryes.Base
{
    public class Db : IDb
    {
        private SqlConnection _connection;

        public SqlConnection Connection => _connection ?? (_connection = GetConnection());

        private static SqlConnection GetConnection()
        {
            var conn = new SqlConnection(AppSettings.ConnectionString);

            if (conn.State == ConnectionState.Closed)
                conn.Open();
            
            return conn;
        }

        private TransactionRepository _transaction;

        public TransactionRepository Transaction=> _transaction ?? (_transaction=new TransactionRepository(Connection));


        public void Start(Action<SqlTransaction, SqlConnection> action)
        {
            try
            {
                Transaction.BeginTransaction();

                 action.Invoke(Transaction.Transaction, Connection);

                Transaction.CommitTransaction();
            }
            catch (Exception)
            {
                Transaction.RollBackTransaction();

                throw;
            }
        }

        public async Task<IEnumerable<TEntity>> GetAll<TEntity>(string tableName = null)
        {
            tableName = tableName ?? typeof(TEntity).Name + "s";

            return await Connection.QueryAsync<TEntity>("SELECT * FROM " + tableName);
        }

        public async Task<List<TEntity>> GetAllWithFilter<TEntity>(DateTime? date,string tableName = null)
        {
            if (!date.HasValue)
                return (await GetAll<TEntity>(tableName)).ToList();

            tableName = tableName ?? typeof(TEntity).Name + "s";

            return (await Connection.QueryAsync<TEntity>($"SELECT * FROM {tableName} WHERE UpdateDate>=@date", new { date = date })).ToList();
        }


        public async Task<TResult> GetById<TResult>(int id)
        {
            var name = typeof(TResult).Name;

            return await Connection.QueryFirstOrDefaultAsync<TResult>(Sql.SqlQueryCach[name], new {id = id});
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
