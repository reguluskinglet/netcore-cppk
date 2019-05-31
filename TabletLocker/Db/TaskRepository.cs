using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using NLog;
using TabletLocker.Model;

namespace TabletLocker.Db
{
    public class TaskRepository : ITaskRepository
    {
        private readonly string _connectionString;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public TaskRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["LocalDbConnString"].ConnectionString;
        }

        public void Add(TaskOut task)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                _logger.Debug($"Add SQL: {SqlAdd}, DeviceFaultId={task.DeviceFaultId}, DeviceId={task.DeviceId}, UserId={task.UserId}, CreateDate={task.CreateDate}");
                db.Execute(SqlAdd, new
                {
                    task.DeviceId,
                    task.UserId,
                    task.DeviceFaultId,
                    task.CreateDate
                });
            }
        }

        public List<TaskOut> GetUnsentForDevice(int deviceId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                _logger.Debug($"GetUnsentForDevice SQL: {SqlGetUnsentForDevice}, DeviceId={deviceId}");
                return db.Query<TaskOut>(SqlGetUnsentForDevice, new {DeviceId = deviceId}).ToList();
            }
        }

        public List<TaskOut> GetAllUnsent()
        {
            using (var db = new SqlConnection(_connectionString))
            {
                _logger.Debug($"GetAllUnsent SQL: {SqlGetAllUnsent}");
                return db.Query<TaskOut>(SqlGetAllUnsent).ToList();
            }
        }

        private const string SqlAdd = "INSERT INTO [TaskOut] ([DeviceId],[UserId],[DeviceFaultId],[CreateDate],[IsSent]) VALUES(@DeviceId,@UserId,@DeviceFaultId,@CreateDate,0)";

        private const string SqlGetUnsentForDevice = "SELECT * FROM [TaskOut] WHERE [IsSent]=0 AND [DeviceId]=@DeviceId";

        private const string SqlGetAllUnsent = "SELECT * FROM [TaskOut] WHERE [IsSent]=0";
    }
}
