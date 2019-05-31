using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Executor;
using Rzdppk.Core.Services;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public class ExecutorRepository 
    {
        private readonly ILogger _logger;
        private readonly IDb _db;

        public ExecutorRepository(ILogger logger)
        {
            _db = new Db();
            _logger = logger;
        }

        public ExecutorRepository(IDb db)
        {
            _db = db;
        }
        public async Task<List<TrainTaskExecutor>> GetByTaskId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Executor.ByTaskId"];
                var result = await conn.QueryAsync<TrainTaskExecutor, User, Brigade, TrainTaskExecutor>(
                    sql,
                    (exec, user, brigade) =>
                    {
                        user.Brigade = brigade;
                        exec.User = user;
                        return exec;
                    }, new {task_id = id});

                return result.ToList();
            }
        }

        public async Task<TrainTaskExecutor> GetExecutorById(int executorId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new ExecutorSqls();
                var executor = await conn.QueryFirstOrDefaultAsync<TrainTaskExecutor>(sql.GetExecutorById,new { executorId });
                return executor;
            }

        }

        
        [Obsolete]
        public async Task<TrainTaskExecutor> Add(TrainTaskExecutor ex, bool timeShift = false)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Executor.Add"];
                if (timeShift)
                    sql = Sql.SqlQueryCach["Executor.Add-5"];

                var id = await conn.QueryAsync<int>(sql,
                    new {task_id = ex.TrainTaskId, brigade_type = ex.BrigadeType, user_id = ex.UserId},
                    _db.Transaction.Transaction);

                ex.Id = id.First();

                return ex;
            }
        }

        //TODO переделать на юзинг
        public async Task<TrainTaskExecutor> AddTransaction(TrainTaskExecutor ex, bool timeShift = false)
        {
            var sql = Sql.SqlQueryCach["Executor.Add"];
            if (timeShift)
                sql = Sql.SqlQueryCach["Executor.Add-5"];

            var id = await _db.Connection.QueryAsync<int>(sql,
                new { task_id=ex.TrainTaskId, brigade_type = ex.BrigadeType, user_id = ex.UserId }, _db.Transaction.Transaction);

            ex.Id = id.First();

            return ex;
        }

        public async Task<TrainTaskExecutor> AddNewExecutorToTask(TaskService.UpdateTaskData data, User user, bool timeShift)
        {
            if (data.TrainTaskExecutorsId != null)
            {
                var executor = new TrainTaskExecutor
                {
                    BrigadeType = (BrigadeType) data.TrainTaskExecutorsId,
                    TrainTaskId = data.TraintaskId,
                    UserId = user.Id
                };
                var result =  await AddNewExecutorToTask(executor, user, timeShift);
                return result;
            }
            return new TrainTaskExecutor();

        }


        public async Task<TrainTaskExecutor> AddNewExecutorToTask(TrainTaskExecutor data, User user, bool timeShift)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new ExecutorSqls();
                var query = sql.AddExecutorToTask;
                if (timeShift)
                    query = sql.AddExecutorToTaskTimeShift;

                var id = await conn.QueryFirstOrDefaultAsync<int>(query,
                    new { task_id = data.TrainTaskId, brigade_type = data.BrigadeType, user_id = data.UserId }
                );
                var result =  await GetExecutorById(id);
                return result;
            }

        }


        public BrigadeType[] GetAvaibleExecutors(int permissions = -1)
        {
            var list = new List<BrigadeType>();

            //тут начинается ебоклюйство с правами.
            //Показывать задачи бригады локомативщиков 6
            var needPermissionBits = 64;
            var res = permissions & needPermissionBits;
            if (res == needPermissionBits)
            {
                list.Add(BrigadeType.Depo);
            }

            ////Показывать задачи бригады приемщиков 8 
            needPermissionBits = 256;
            res = permissions & needPermissionBits;
            if (res == needPermissionBits)
            {
                list.Add(BrigadeType.Depo);
            }

            ////Админиблядьстрация 18 
            needPermissionBits = 262144;
            res = permissions & needPermissionBits;
            if (res == needPermissionBits)
            {
                list.Add(BrigadeType.Receiver);
                list.Add(BrigadeType.Depo);
                list.Add(BrigadeType.Locomotiv);
            }

            //ставить исполнителя вручную может только админ! (и админиблядьстрация)
            if (permissions == -1)
            {
                list.Add(BrigadeType.Receiver);
                list.Add(BrigadeType.Depo);
                list.Add(BrigadeType.Locomotiv);
            }
            return list.Distinct().ToArray();
        }


    }
}
