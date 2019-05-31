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
    public class DeviceFaultRepository : IDeviceFaultRepository
    {
        private readonly string _connectionString;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public DeviceFaultRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["LocalDbConnString"].ConnectionString;
        }

        public List<DeviceFault> GetAll()
        {
            using (var db = new SqlConnection(_connectionString))
            {
                _logger.Debug($"GetAll SQL: {SqlSelectAll}");
                return db.Query<DeviceFault>(SqlSelectAll).ToList();
            }
        }

        private const string SqlSelectAll = "SELECT * FROM [DeviceFaults]";
    }
}
