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
    public class DeviceRepository : IDeviceRepository
    {
        private readonly string _connectionString;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public DeviceRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["LocalDbConnString"].ConnectionString;
        }

        public List<Device> GetAll()
        {
            using (var db = new SqlConnection(_connectionString))
            {
                _logger.Debug($"GetAll SQL: {SqlSelectAll}");
                return db.Query<Device>(SqlSelectAll).ToList();
            }
        }

        public List<Device> GetAllCharged()
        {
            using (var db = new SqlConnection(_connectionString))
            {
                _logger.Debug($"GetAllCharged SQL: {SqlSelectAllCharged}");
                return db.Query<Device>(SqlSelectAllCharged).ToList();
            }
        }

        public List<Device> GetDevicesByBarcode(string barcode)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                _logger.Debug($"GetDeviceByBarcode SQL: {SqlFindByBarcode}, barcode={barcode}");
                return db.Query<Device>(SqlFindByBarcode, new { Barcode = barcode }).ToList();
            }
        }

        public Device GetDeviceByCellNumber(int cell)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                _logger.Debug($"GetDeviceByCellNumber SQL: {SqlFindByCellNumber}, CellNumber={cell}");
                return db.QuerySingleOrDefault<Device>(SqlFindByCellNumber, new { CellNumber = cell });
            }
        }

        private const string SqlSelectAll = "SELECT * FROM [Devices] WHERE CellNumber>0";
        private const string SqlSelectAllCharged = "SELECT * FROM [Devices] WHERE [LastCharge]=100 AND CellNumber>0";
        private const string SqlFindByBarcode = "SELECT * FROM [Devices] WHERE [Serial]=@Barcode AND CellNumber>0";
        private const string SqlFindByCellNumber = "SELECT * FROM [Devices] WHERE [CellNumber]=@CellNumber";
    }
}
