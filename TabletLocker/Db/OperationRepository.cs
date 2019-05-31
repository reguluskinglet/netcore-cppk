using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using NLog;
using TabletLocker.Db.Interfaces;
using TabletLocker.Model;

namespace TabletLocker.Db
{
    public class OperationRepository : IOperationRepository
    {
        private readonly string _connectionString;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public OperationRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["LocalDbConnString"].ConnectionString;
        }

        public void Add(OperationOut oper)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                _logger.Debug($"Add SQL: {SqlAdd}, Operation={oper.Operation}, DeviceId={oper.DeviceId}, UserId={oper.UserId}, CreateDate={oper.CreateDate}");
                db.Execute(SqlAdd, new
                {
                    oper.Operation,
                    oper.DeviceId,
                    oper.UserId,
                    oper.CreateDate
                });
            }
        }

        public List<OperationOut> GetAllUnsentForDevice(int deviceId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                _logger.Debug($"GetAllUnsentForDevice SQL: {SqlGetUnsentForDevice}, DeviceId={deviceId}");
                return db.Query<OperationOut>(SqlGetUnsentForDevice, new {DeviceId = deviceId}).ToList();
            }
        }

        public OperationOut GetLastUnsentForDevice(int deviceId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                _logger.Debug($"GetLastUnsentForDevice SQL: {SqlGetUnsentForDevice}, DeviceId={deviceId}");
                return db.Query<OperationOut>(SqlGetUnsentForDevice, new { DeviceId = deviceId }).OrderByDescending(o => o.CreateDate).FirstOrDefault();
            }
        }

        public List<OperationOut> GetAllUnsent()
        {
            using (var db = new SqlConnection(_connectionString))
            {
                _logger.Debug($"GetAllUnsent SQL: {SqlGetAllUnsent}");
                return db.Query<OperationOut>(SqlGetAllUnsent).ToList();
            }
        }

        private const string SqlAdd = "INSERT INTO [OperationOut] ([Operation],[DeviceId],[UserId],[CreateDate],[IsSent]) VALUES(@operation,@DeviceId,@UserId,@CreateDate,0)";

        private const string SqlGetUnsentForDevice = "SELECT * FROM [OperationOut] WHERE [IsSent]=0 AND [DeviceId]=@DeviceId";

        //private const string SqlGetLastUnsent = "SELECT TOP 1 * FROM [OperationOut] WHERE [IsSent]=0 ORDER BY [CreateDate]";

        private const string SqlGetAllUnsent = "SELECT * FROM [OperationOut] WHERE [IsSent]=0";
    }
}
