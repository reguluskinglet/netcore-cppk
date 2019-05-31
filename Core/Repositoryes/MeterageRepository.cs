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
using Rzdppk.Model;

namespace Rzdppk.Core.Repositoryes
{
    public class MeterageRepository: IMeterageRepository, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IDb _db;

        public MeterageRepository(ILogger logger)
        {
            _db = new Db();
            _logger = logger;
        }

        public MeterageRepository(IDb db)
        {
            _db = db;
        }

        public async Task<LabelUI[]> GetLabels(int inspectionId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Meterage.LabelsByInspectionId"];
                var result = await conn
                    .QueryAsync<Meterage, Label, Carriage, Train, EquipmentModel, Equipment, Meterage>(
                        sql,
                        (meterage, label, carriage, train, em, equipment) =>
                        {
                            carriage.Train = train;
                            meterage.Label = label;
                            meterage.Label.Carriage = carriage;
                            meterage.Label.EquipmentModel = em;
                            meterage.Label.EquipmentModel.Equipment = equipment;
                            return meterage;
                        }, new {inspection_id = inspectionId});

                //if (result.ToArray().Length == 0)
                //    return  new LabelUI[0];

                ////Оставляем только уникальные по labelId
                //var output = new List<LabelUI>();
                //foreach (var item in result)
                //{
                //    if (output.FirstOrDefault(e => e.Label.Id == item.LabelId)?.Label.Id == item.LabelId)
                //        continue;
                //    output.Add(new LabelUI{Date = item.Date, Label = item.Label});
                //}

                //result = result.GroupBy(x => x.LabelId).Select(x => x.First()).ToArray();

                var ret = result.Select(meterage => new LabelUI
                    {
                        Date = meterage.Date,
                        Label = meterage.Label
                    })
                    .ToArray();

                return ret;
            }
        }

        public async Task<MeterageUI[]> GetMeterages(int inspectionId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Meterage.MeteragesByInspectionId"];
                var result = await conn.QueryAsync<Meterage>(
                    sql, new {inspection_id = inspectionId});

                var ret = result.Select(meterage => new MeterageUI
                    {
                        Date = meterage.Date,
                        Value = meterage.Value ?? 0
                    })
                    .ToArray();

                return ret;
            }
        }

        public class MeterageUI
        {
            public DateTime Date { get; set; }
            public int  Value { get; set; }
        }

        public class LabelUI
        {
            public DateTime Date { get; set; }
            public Label Label { get; set; }
        }

        public void Dispose()
        {
            _db.Connection.Close();
        }
    }
}
