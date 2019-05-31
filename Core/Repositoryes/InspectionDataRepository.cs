using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public class InspectionDataRepository : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IDb _db;
        
        public InspectionDataRepository(ILogger logger)
        {
            _db = new Db();
            _logger = logger;
        }

        public InspectionDataRepository(IDb db)
        {
            _db = db;
        }

        public async Task<List<InspectionData>> GetByInspectionId(int inspectionId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["InspectionData.ById"];

                var result = await conn.QueryAsync<InspectionData, Carriage, InspectionData>(
                    sql,
                    (data, carriage) =>
                    {
                        data.Carriage = carriage;
                        return data;
                    }, new {inspectionId = inspectionId});

                return result.ToList();
            }
        }

        public class InspectionCounters
        {
            public int Measurements { get; set; }
            public int Labels { get; set; }
            public int LabelsAll { get; set; }
            public int Tasks { get; set; }
        }

        public void Dispose()
        {
            _db.Connection.Close();
        }
    }
}
