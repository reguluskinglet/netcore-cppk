using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public class CarriageRepository
    {
        private readonly CarriageSql _sql;
        private readonly ILogger _logger;

        public CarriageRepository(ILogger logger)
        {
            _sql = new CarriageSql();
            _logger = logger;
        }

        public async Task<CarriageExt[]> GetByTrain(Train train)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Carriage.ByTrainId"];

                var result = (await conn.QueryAsync<CarriageExt, Model.Model, CarriageExt>(
                    sql,
                    (carriage, model) =>
                    {
                        carriage.Model = model;
                        return carriage;
                    }, new {train_id = train.Id})).ToList();

                //надо добавить сюда выцепленные вагоны с этого поезда

                const string sqlU = @"select c.*,
                            CAST(
                                CASE WHEN EXISTS(SELECT * FROM Labels l WHERE l.CarriageId=c.Id) THEN 0
                                ELSE 1
                                END 
                            AS BIT) as CanDelete,m.*
                            from [CarriageMigrations] m1
                            left join [Carriages] c on c.Id=m1.CarriageId
                            left join [Models] m on c.ModelId = m.Id
                            where m1.TrainId=@TrainId and c.TrainId is null";

                var resultU = await conn.QueryAsync<CarriageExt, Model.Model, CarriageExt>(
                    sqlU,
                    (carriage, model) =>
                    {
                        carriage.Model = model;
                        carriage.Serial = carriage.Serial + " (выцеплен)";
                        return carriage;
                    }, new { TrainId = train.Id });

                result.AddRange(resultU);

                return result.OrderBy(c=>c.Number).ToArray();
            }
        }

        public async Task<CarriageExt> GetById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Carriage.ById"];

                var result = await conn.QueryAsync<CarriageExt, Model.Model, CarriageExt>(
                    sql,
                    (carriage, model) =>
                    {
                        carriage.Model = model;
                        return carriage;
                    }, new {id = id});

                return result.FirstOrDefault();
            }
        }

        public async Task<Carriage> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                return await conn.QueryFirstOrDefaultAsync<Carriage>(_sql.ById(id));
            }
        }


        private static string TranslateChecklistType(CheckListType ct)
        {
            var ret = "?";
            switch (ct)
            {
                case CheckListType.Inspection:
                    ret = "Приёмка Лб";
                    break;
                case CheckListType.TO1:
                    ret = "ТО-1";
                    break;
                case CheckListType.TO2:
                    ret = "ТО-2";
                    break;
                case CheckListType.Surrender:
                    ret = "Приёмка Пр";
                    break;
            }
            return ret;
        }

        public async Task<CarriageWithEquipment> GetByIdWithEquipment(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var carriage = await GetById(id);

                var mr = new ModelRepository(_logger);
                var equipment = await mr.GetEquipmentByModel(carriage.Model);
                var sqlr = new EquipmentRepository(_logger);
                var sqlLabelsR = new LabelRepository();

                var listeq = new List<LocationEquipmentAlgo>();
                foreach (var loc in equipment)
                {
                    string nameFromParent = null;
                    if (loc.ParentId != 0)
                        nameFromParent = sqlr.GetByParentId(loc.ParentId).Name;

                    //названия типов чеклистов, прицепленных к данному оборудованию
                    var checkListEquipment = await sqlr.GetCheckListByEquipmentModelId(loc.Id);
                    var algosNameList = (from algo in checkListEquipment.Algorithms
                        where algo.NameTask != null
                        select TranslateChecklistType(algo.CheckListType)).ToList();

                    listeq.Add(new LocationEquipmentAlgo
                    {
                        Location = nameFromParent,
                        Equipment = loc.Equipment.Name,
                        Algorithm = String.Join(", ", algosNameList.Distinct()),
                        Labels = (await sqlLabelsR.GetByCarriageId(carriage.Id, loc.Id)).ToArray().Select(e => e.Rfid)
                            .ToArray()
                    });
                }

                var ce = new CarriageWithEquipment
                {
                    Carriage = carriage,
                    Equipment = listeq.ToArray()
                };

                return ce;
            }
        }

        public async Task Update(Carriage carriage)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Carriage.Update"];
                await conn.ExecuteAsync(sql,
                    new
                    {
                        number = carriage.Number,
                        serial = carriage.Serial,
                        id = carriage.Id,
                        model_id = carriage.ModelId,
                        train_id = carriage.TrainId
                    });
            }
        }

        public async Task<Carriage> Add(Carriage carriage)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var id = await conn.QueryAsync<int>(Sql.SqlQueryCach["Carriage.Add"],
                    new
                    {
                        number = carriage.Number,
                        serial = carriage.Serial,
                        model_id = carriage.ModelId,
                        train_id = carriage.TrainId
                    });

                carriage.Id = id.First();

                return carriage;
            }
        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Carriage.Delete"], new {id = id});
            }
        }

        public async Task Unlink(int carriageId, int stantionId)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var conn = new SqlConnection(AppSettings.ConnectionString))
                {
                    var carriage = await ById(carriageId);

                    if (carriage == null)
                        throw new Exception("неверный Id вагона");

                    if (carriage.TrainId == null)
                        throw new Exception("вагон уже выцеплен");
                    
                    //запись в миграции
                    const string sqlM = "insert into [CarriageMigrations] ([CarriageId],[TrainId],[StantionId]) values(@CarriageId,@TrainId,@StantionId)";
                    await conn.ExecuteAsync(sqlM, new { CarriageId = carriageId, TrainId = carriage.TrainId, StantionId = stantionId});

                    //обнуляется trainId у вагона
                    const string sqlC = "update [Carriages] set [TrainId]=null where Id=@Id";
                    await conn.ExecuteAsync(sqlC, new { Id = carriageId });

                    scope.Complete();
                }
            }
        }

        public async Task RestoreLink(int carriageId)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var conn = new SqlConnection(AppSettings.ConnectionString))
                {
                    var migr = await GetLastMigration(carriageId);

                    //todo: схема не позволяет обнулить поезд, при изменении схемы лучше добавить две даты - дату отцепления и возврата
                    //if (migr.TrainId == null)
                    //    throw new Exception("вагон не выцеплен");

                    ////запись в миграции
                    //const string sqlM = "insert into [CarriageMigrations] ([CarriageId],[TrainId],[StantionId]) values(@CarriageId,null,@StantionId)";
                    //await conn.ExecuteAsync(sqlM, new { CarriageId = carriageId, StantionId = migr.StantionId });

                    //возвращается trainId у вагона
                    const string sqlC = "update [Carriages] set [TrainId]=@TrainId where Id=@Id";
                    await conn.ExecuteAsync(sqlC, new { TrainId = migr.TrainId, Id = carriageId });

                    scope.Complete();
                }
            }
        }

        public async Task<CarriageMigration> GetLastMigration(int carriageId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                const string sqlM = "select top 1 * from [CarriageMigrations] where [CarriageId]=@Id order by [UpdateDate] desc";

                var ret = await conn.QueryFirstAsync<CarriageMigration>(sqlM, new {Id = carriageId});

                return ret;
            }
        }

        public async Task<DevExtremeTableData.ReportResponse> GetMigrationHistoryTable(CarriageMigrationHistoryRequest input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = new DevExtremeTableData.ReportResponse();

                const string sqlM = @"select * from [CarriageMigrations] m
                                left join [Trains] t on t.Id=m.TrainId
                                left join [Carriages] c on c.Id=m.CarriageId
                                left join [Stantions] s on s.Id=m.StantionId
                                where m.[CarriageId]=@Id order by m.[UpdateDate] desc";

                var items = (await conn.QueryAsync<CarriageMigration, Train, Carriage, Stantion, CarriageMigration>(
                    sqlM,
                    (migr, train, carriage, stantion) =>
                    {
                        migr.Carriage = carriage;
                        migr.Train = train;
                        migr.Stantion = stantion;
                        return migr;
                    }, new { Id = input.CarriageId })).ToList();

                result.Columns = new List<DevExtremeTableData.Column>
                {
                    new DevExtremeTableData.Column("col0", "Дата выцепления", "date"),
                    new DevExtremeTableData.Column("col1", "Станция привязки", "string")
                    //new DevExtremeTableData.Column("col2", "Дата возврата", "string"),
                };

                result.Rows = new List<DevExtremeTableData.Row>();

                if (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        result.Rows.Add(new DevExtremeTableData.Row
                        {
                            HasItems = false.ToString(),
                            Col0 = item.UpdateDate.ToStringDateTime(),
                            Col1 = item.Stantion.Name
                        });
                    }
                }

                result.Rows = DevExtremeTableUtils.DevExtremeTableFiltering(result.Rows, input.Filters);
                result.Rows = DevExtremeTableUtils.DevExtremeTableSorting(result.Rows, input.Sortings);
                result.Total = result.Rows.Count.ToString();
                result.Paging(input.Paging);

                return result;
            }
        }

        public class CarriageMigrationHistoryRequest
        {
            public int CarriageId { get; set; }
            public DevExtremeTableData.Paging Paging { get; set; }
            public List<DevExtremeTableData.Filter> Filters { get; set; }
            public List<DevExtremeTableData.Sorting> Sortings { get; set; }
        }

        public class CarriageExt: Carriage
        {
            public bool CanDelete { get; set; }
        }

        public class CarriageWithEquipment
        {
            public CarriageExt Carriage { get; set; }
            public LocationEquipmentAlgo[] Equipment { get; set; }
        }

        public class LocationEquipmentAlgo
        {
            public string Location { get; set; }
            public string Equipment { get; set; }
            public string Algorithm { get; set; }
            public string[] Labels { get; set; }
        }
    }
}
