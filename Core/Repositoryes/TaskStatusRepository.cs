using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using Rzdppk.Core.Repositoryes.Sqls.TaskStatus;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;
using TaskStatus = Rzdppk.Model.Enums.TaskStatus;

namespace Rzdppk.Core.Repositoryes
{
    public class TaskStatusRepository // ITaskStatusRepository, IDisposable
    {
        private readonly IDb _db;
        private readonly ILogger _logger;

        public TaskStatusRepository(ILogger logger)
        {
            _db = new Db();
            _logger = logger;
        }

        public TaskStatusRepository(IDb db, ILogger logger)
        {
            _db = db;
            _logger = logger;
        }


        public async Task<TrainTaskStatus[]> ByTaskId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["TaskStatus.ByTaskId"];
                var result = await conn.QueryAsync<TrainTaskStatus, User, Brigade, TrainTaskStatus>(
                    sql,
                    (status, user, brigade) =>
                    {
                        user.Brigade = brigade;
                        status.User = user;
                        return status;
                    }, new {task_id = id});

                return result.ToArray();
            }
        }

        public TaskStatus[] GetNextStatuses(TaskStatus? current, BrigadeType? executorBrigadeType = 0, int permissions = -1)
        {
            var list = new List<TaskStatus>();

            //тут начинается ебоклюйство с правами.
            // бригады локомативщиков 6
            var needPermissionBits = 64;
            var res = permissions & needPermissionBits;
            if (res == needPermissionBits)
            {
                if (current == TaskStatus.New)
                {
                    list.Add(TaskStatus.Log);
                }
                else if (current == TaskStatus.Confirmation)
                {
                    list.Add(TaskStatus.Closed);
                    list.Add(TaskStatus.Remake);
                }
            }
            //бригады депо 7
            //needPermissionBits = 64;
            //res = permissions & needPermissionBits;
            //if (res == needPermissionBits)
            //{

            //    list.Add(TaskStatus.AcceptedForExecution);
            //    list.Add(TaskStatus.Done);
            //    list.Add(TaskStatus.NotConfirmed);
                
            //}
            // приемщиков 8 
            needPermissionBits = 256;
            res = permissions & needPermissionBits;
            if (res == needPermissionBits)
            {
                if (current == TaskStatus.New)
                {
                    list.Add(TaskStatus.Log);
                }
                else if (current == TaskStatus.InWork)
                {
                    list.Add(TaskStatus.Confirmation);
                }
            }

            //Админиблядьстрация 18 
            needPermissionBits = 262144;
            res = permissions & needPermissionBits;
            if (res == needPermissionBits)
            {
                list.Add(TaskStatus.New);
                list.Add(TaskStatus.Log);
                list.Add(TaskStatus.InWork);
                list.Add(TaskStatus.Closed);
                list.Add(TaskStatus.Remake);
                list.Add(TaskStatus.Confirmation);
            }

            //admin
            if (permissions == -1)
            {
                list.Add(TaskStatus.New);
                list.Add(TaskStatus.Log);
                list.Add(TaskStatus.InWork);
                list.Add(TaskStatus.Confirmation);
                list.Add(TaskStatus.Remake);
                list.Add(TaskStatus.Closed);
            }

            return list.Distinct().ToArray();
        }

        public List<TaskStatus> GetAvailableStatusesNewTask(TaskStatus? current, BrigadeType? executorBrigadeType = 0, int permissions = -1)
        {
            var list = new List<TaskStatus>();

            //тут начинается ебоклюйство с правами.
            // бригады локомативщиков 6
            var needPermissionBits = 64;
            var res = permissions & needPermissionBits;
            if (res == needPermissionBits)
            {
                list.Add(TaskStatus.Log);
            }
            //бригады депо 7
            //needPermissionBits = 128;
            //res = permissions & needPermissionBits;
            //if (res == needPermissionBits)
            //{
            //    list.Add(TaskStatus.InWork);
            //}
            // приемщиков 8 
            needPermissionBits = 256;
            res = permissions & needPermissionBits;
            if (res == needPermissionBits)
            {
                list.Add(TaskStatus.Log);
            }


            //Админиблядьстрация 18 
            needPermissionBits = 262144;
            res = permissions & needPermissionBits;
            if (res == needPermissionBits)
            {
                list.Add(TaskStatus.New);
                list.Add(TaskStatus.Log);
                list.Add(TaskStatus.InWork);
                list.Add(TaskStatus.Confirmation);
                list.Add(TaskStatus.Remake);
                list.Add(TaskStatus.Closed);
            }

                //{
                //    if (executorBrigadeType == BrigadeType.Depo)
                //        list.Add(TaskStatus.InWork);
                //    if (executorBrigadeType == BrigadeType.Receiver)
                //    {
                //        list.Add(TaskStatus.InWork);
                //        list.Add(TaskStatus.ToCheck);
                //    }
                //    if (executorBrigadeType == BrigadeType.Locomotiv)
                //    {
                //        list.Add(TaskStatus.InWork);
                //        list.Add(TaskStatus.Confirmation);
                //    }
                //}

            //для админа доступны все статусы из текущей рабочей схемы
            if (permissions == -1)
            {
                list.Add(TaskStatus.New);
                list.Add(TaskStatus.Log);
                list.Add(TaskStatus.InWork);
                list.Add(TaskStatus.Confirmation);
                list.Add(TaskStatus.Remake);
                list.Add(TaskStatus.Closed);
            }

            return list.Distinct().ToList();
        }


        public TrainTaskStatus GetByTrainTaskId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["TaskStatus.LastByTrainTaskId"];
                var result = conn.Query<TrainTaskStatus>(sql, new {id = id});
                return result.FirstOrDefault();
            }
        }


        public async Task<int> ChangeStatusByTrainTaskId(int taskId, int statusId, User user, bool newtask = false, bool timeShift = false)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var permissions = user.Role.Permissions;

                var trainTaskStatus = new TrainTaskStatus
                {
                    TrainTaskId = taskId,
                    Status = (TaskStatus) statusId,
                    UserId = user.Id,
                    Date = DateTime.Now
                };

                SqlMapper.AddTypeMap(typeof(DateTime), System.Data.DbType.DateTime2);

                var sql = new TaskStatusSqls();
                var query = sql.AddStatusToTask;
                if (timeShift)
                    query = sql.AddStatusToTaskTimeShift;
                

                var id = await conn.QueryAsync<int>(query,
                    new
                    {
                        date = trainTaskStatus.Date,
                        status = trainTaskStatus.Status,
                        trainTaskId = trainTaskStatus.TrainTaskId,
                        userId = trainTaskStatus.UserId
                    });

                //Админу никаких автосмен
                var needPermissionBits = -1;
                var res = permissions & needPermissionBits;
                if (res != needPermissionBits)
                {
                    if (!newtask)
                    {

                        // бригады локомативщиков 6
                        needPermissionBits = 64;
                        res = permissions & needPermissionBits;
                        if (res == needPermissionBits)
                        {
                            if (statusId == (int) TaskStatus.Remake)
                                await ChangeStatusByTrainTaskId(taskId, (int) TaskStatus.InWork, user);
                        }
                        // приемщиков 8 
                        needPermissionBits = 256;
                        res = permissions & needPermissionBits;
                        if (res == needPermissionBits)
                        {
                            if (statusId == (int) TaskStatus.Remake)
                                await ChangeStatusByTrainTaskId(taskId, (int) TaskStatus.InWork, user);

                        }
                       
                    }
                }

                return id.FirstOrDefault();


            }
        }

    }
}
