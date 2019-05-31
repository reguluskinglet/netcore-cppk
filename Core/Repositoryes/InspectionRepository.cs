using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rzdppk.Api.Dto.EventTable;
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
    public class InspectionRepository 
    {
        private readonly ILogger _logger;
        private readonly string _tableName;
        private readonly InspectionSql _sql;

        public InspectionRepository(ILogger logger)
        {
            _logger = logger;
            _tableName = "Inspections";
            _sql = new InspectionSql(_tableName);
        }



        public async Task<List<Inspection>> GetAllSortByProperty(string property, string direction)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<Inspection>(CommonSql.GetAllSortByProperty(_tableName, property, direction));
                return result.ToList();
            }
        }

        public async Task<List<Inspection>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<Inspection>(CommonSql.GetAll(_tableName));
                return result.ToList();
            }
        }

        public async Task<Inspection> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Inspection.ById"];
                var result = await conn.QueryFirstOrDefaultAsync<Inspection>(sql, new {id = id});
                return result;
            }
        }

        public async Task<InspectionByIdDto> ByIdForEventTable(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryFirstOrDefaultAsync<InspectionByIdDto>(_sql.ByIdForEventTable(id));
                return result;
            }
        }


        public async Task<List<Inspection>> GetByTrainId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = await conn.QueryAsync<Inspection>(CommonSql.ByPropertyId(_tableName, "TrainId", id));
                return result.ToList();
            }
        }

        public async Task<InspectionCounters> GetCounters(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var measCount = await conn.QueryAsync<int>(
                    Sql.SqlQueryCach["Inspection.CountMeasurements"], new {inspection_id = id});
                var lblCount = await conn.QueryAsync<int>(
                    Sql.SqlQueryCach["Inspection.CountLabelsInspectionIdDistinct"], new {inspection_id = id});
                var lblAllCount = await conn.QueryAsync<int>(
                    Sql.SqlQueryCach["Inspection.CountLabelsAll"]);
                var taskCount = await conn.QueryAsync<int>(
                    Sql.SqlQueryCach["Inspection.CountTasks"], new {inspection_id = id});

                return new InspectionCounters
                {
                    Labels = lblCount.FirstOrDefault(),
                    LabelsAll = lblAllCount.FirstOrDefault(),
                    Measurements = measCount.FirstOrDefault(),
                    Tasks = taskCount.FirstOrDefault()
                };
            }
        }

        public class InspectionCounters
        {
            public int Measurements { get; set; }
            public int Labels { get; set; }
            public int LabelsAll { get; set; }
            public int Tasks { get; set; }
        }


    }
}
