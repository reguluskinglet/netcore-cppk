using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using NPOI.OpenXmlFormats.Dml.Chart;
using Rzdppk.Core.Helpers;
using Rzdppk.Core.Options;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;
using Rzdppk.Model.Raspisanie;

namespace Rzdppk.Core.Repositoryes
{
    public class DepoEventsRepository : IDepoEventsRepository
    {
        private readonly IDb _db;
        private readonly String _table;
        private readonly ILogger _logger;

        public DepoEventsRepository(IDb db)
        {
            _db = db;
            _table = "DepoEvents";
        }

        public DepoEventsRepository(ILogger logger)
        {
            _logger = logger;
            _table = "DepoEvents";
        }


        /// <summary>
        /// Получить все по id поезда
        /// </summary>
        public async Task<List<DepoEvent>> ByTrainId(int trainId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var data = await conn.QueryAsync<DepoEvent>(CommonSql.ByPropertyId(_table, "TrainId", trainId));
                return data.ToList();
            }
        }


        public async Task<List<LiveSearchItemDto>> GetTesterUsers(string search)
        {
            var sql = "select * from [auth_users] where [IsBlocked]=0";

            var dbArgs = new DynamicParameters();

            if (!string.IsNullOrEmpty(search) && search.Length > 0)
            {
                sql += " and [Name] like @search";
                dbArgs.Add("search", $"%{search}%");
            }

            var list = (await _db.Connection.QueryAsync<User>(sql, dbArgs)).OrderBy(o => o.Name).Select(o =>
                new LiveSearchItemDto
                {
                    Id = o.Id,
                    Name = o.Name
                }).ToList();

            return list;
        }

        public async Task<List<LiveSearchRouteTrainItemDto>> GetRoutes(string search)
        {
            var now = DateTime.Now;
            var dateFrom = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0, now.Kind);
            var dateTo = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999, now.Kind);

            var sql =
                "select * from [PlanedRouteTrains] pt left join [Routes] r on r.Id=pt.RouteId where (pt.Date between @DateFrom and @DateTo)";

            var dbArgs = new DynamicParameters();
            dbArgs.Add("DateFrom", dateFrom);
            dbArgs.Add("DateTo", dateTo);

            if (!string.IsNullOrEmpty(search) && search.Length > 0)
            {
                sql += " and r.[Name] like @search";
                dbArgs.Add("search", $"%{search}%");
            }

            var list = (await _db.Connection.QueryAsync<PlanedRouteTrain, Route, LiveSearchRouteTrainItemDto>(
                sql,
                (plannedTrain, route) => new LiveSearchRouteTrainItemDto
                {
                    Id = route.Id,
                    Name = route.Name,
                    TrainId = plannedTrain.TrainId
                }, dbArgs)).ToList();

            return list;
        }

        public async Task<List<LiveSearchItemDto>> GetTrains(string search)
        {
            var sql = "select * from [Trains]";

            var dbArgs = new DynamicParameters();

            if (!string.IsNullOrEmpty(search) && search.Length > 0)
            {
                sql += " where [Name] like @search";
                dbArgs.Add("search", $"%{search}%");
            }

            var list = (await _db.Connection.QueryAsync<Train>(sql, dbArgs)).OrderBy(o => o.Name).Select(o =>
                new LiveSearchItemDto
                {
                    Id = o.Id,
                    Name = o.Name
                }).ToList();

            return list;
        }

        public async Task<List<LiveSearchItemDto>> GetDepos(string search)
        {
            var sql = "select * from [Stantions] where [StantionType]=0";

            var dbArgs = new DynamicParameters();

            if (!string.IsNullOrEmpty(search) && search.Length > 0)
            {
                sql += " and [Name] like @search";
                dbArgs.Add("search", $"%{search}%");
            }

            var list = (await _db.Connection.QueryAsync<Stantion>(sql, dbArgs)).OrderBy(o => o.Name).Select(o =>
                new LiveSearchItemDto
                {
                    Id = o.Id,
                    Name = o.Name
                }).ToList();

            return list;
        }


        public async Task<List<LiveSearchItemDto>> GetDepoParkings(int depoId, string search)
        {
            var sql = "select * from [Parkings] where [StantionId]=@DepoId";

            var dbArgs = new DynamicParameters();
            dbArgs.Add("DepoId", depoId);

            if (!string.IsNullOrEmpty(search) && search.Length > 0)
            {
                sql += " and [Name] like @search";
                dbArgs.Add("search", $"%{search}%");
            }

            var list = (await _db.Connection.QueryAsync<Stantion>(sql, dbArgs)).OrderBy(o => o.Name).Select(o =>
                new LiveSearchItemDto
                {
                    Id = o.Id,
                    Name = o.Name
                }).ToList();

            return list;
        }

        public async Task<List<LiveSearchItemDto>> GetInspections(int trainId, string search)
        {
            var sql = "select * from [Inspections] i left join [auth_users] u on u.Id=i.UserId left join [Brigades] b on b.Id=u.BrigadeId where i.[TrainId]=@TrainId";

            var dbArgs = new DynamicParameters();
            dbArgs.Add("TrainId", trainId);

            var list = (await _db.Connection.QueryAsync<Inspection, User, Brigade, LiveSearchItemDto>(
                sql,
                (inspection, user, brigade) => new LiveSearchItemDto
                {
                    Id = inspection.Id,
                    Name = GetInspectionString(inspection, brigade?.BrigadeType)
                }, dbArgs)).OrderBy(o => o.Name).ToList();

            if (!string.IsNullOrEmpty(search) && search.Length > 0)
            {
                list = list.Where(o => o.Name.Contains(search)).ToList();
            }

            return list;
        }

        public async Task<DepoEventDto> Create(DepoEventDto input)
        {
            if (input.Inspection != null && input.InspectionTxt != null)
            {
                throw new Exception("Инспекцию можно либо выбрать, либо задать текстом");
            }

            var sql =
                @"insert into [DepoEvents] ([ParkingId],[InspectionId],[InspectionTxt],[TrainId],[RouteId],[InTime],[ParkingTime],[RepairStopTime],[TestStartTime],[TestStopTime],[UserId]) values (
                                            @ParkingId,@InspectionId,@InspectionTxt,@TrainId,@RouteId,@InTime,@ParkingTime,@RepairStopTime,@TestStartTime,@TestStopTime,@UserId) SELECT SCOPE_IDENTITY()";
            var id = await _db.Connection.QueryFirstOrDefaultAsync<int>(sql,
                new
                {
                    ParkingId = input.Parking?.Id,
                    InspectionId = input.Inspection?.Id,
                    InspectionTxt = input.InspectionTxt,
                    TrainId = input.Train?.Id,
                    RouteId = input.Route?.Id,
                    InTime = input.InTime,
                    ParkingTime = input.ParkingTime,
                    RepairStopTime = input.RepairStopTime,
                    TestStartTime = input.TestStartTime,
                    TestStopTime = input.TestStopTime,
                    UserId = input.TesterUser?.Id
                });

            return await ById(id);
        }

        public async Task<DepoEventDto> Update(DepoEventDto input)
        {
            if (input.Inspection != null && input.InspectionTxt != null)
            {
                throw new Exception("Инспекцию можно либо выбрать, либо задать текстом");
            }

            var sql =
                @"update [DepoEvents] set [ParkingId]=@ParkingId,[InspectionId]=@InspectionId,[InspectionTxt]=@InspectionTxt,[TrainId]=@TrainId,[RouteId]=@RouteId,[InTime]=@InTime,
                                            [ParkingTime]=@ParkingTime,[RepairStopTime]=@RepairStopTime,[TestStartTime]=@TestStartTime,[TestStopTime]=@TestStopTime,[UserId]=@UserId where id=@Id";
            await _db.Connection.ExecuteAsync(sql,
                new
                {
                    ParkingId = input.Parking?.Id,
                    InspectionId = input.Inspection?.Id,
                    InspectionTxt = input.InspectionTxt,
                    TrainId = input.Train?.Id,
                    RouteId = input.Route?.Id,
                    InTime = input.InTime,
                    ParkingTime = input.ParkingTime,
                    RepairStopTime = input.RepairStopTime,
                    TestStartTime = input.TestStartTime,
                    TestStopTime = input.TestStopTime,
                    UserId = input.TesterUser?.Id,
                    Id = input.Id
                });

            return await ById(input.Id);
        }

        public async Task<DepoEventDto> ById(int id)
        {
            var sql = @"select * from [DepoEvents] e
                        left join [Parkings] p on p.Id=e.ParkingId
                        left join [Inspections] i on i.Id=e.InspectionId
                        left join [auth_users] ui on ui.Id=i.UserId
                        left join [Brigades] b on b.Id=ui.BrigadeId
                        left join [Trains] t on t.Id=e.TrainId
                        left join [Routes] r on r.Id=e.RouteId
                        left join [auth_users] u on u.Id=e.UserId where e.Id=@Id";

            var item = (await _db.Connection.QueryAsync(sql,
                new[]
                {
                    typeof(DepoEvent), typeof(Parking), typeof(Inspection), typeof(User), typeof(Brigade), typeof(Train), typeof(Route), typeof(User)
                },
                BindToModel, new { Id = id })).First();

            return item;
        }

        //table
        public async Task<DevExtremeTableData.ReportResponse> GetTable(DepoEventsRequest input)
        {
            var result = new DevExtremeTableData.ReportResponse();

            var sql = @"select * from [DepoEvents] e
                        left join [Parkings] p on p.Id=e.ParkingId
                        left join [Inspections] i on i.Id=e.InspectionId
                        left join [auth_users] ui on ui.Id=i.UserId
                        left join [Brigades] b on b.Id=ui.BrigadeId
                        left join [Trains] t on t.Id=e.TrainId
                        left join [Routes] r on r.Id=e.RouteId
                        left join [auth_users] u on u.Id=e.UserId";

            var items = (await _db.Connection.QueryAsync(sql,
                new[]
                {
                    typeof(DepoEvent), typeof(Parking), typeof(Inspection), typeof(User), typeof(Brigade), typeof(Train), typeof(Route), typeof(User)
                },
                BindToModel)).ToList();

            result.Columns = new List<DevExtremeTableData.Column>
            {
                new DevExtremeTableData.Column("col0", "Место постановки", "string"),
                new DevExtremeTableData.Column("col1", "Состав", "string"),
                new DevExtremeTableData.Column("col2", "Маршрут", "string"),
                new DevExtremeTableData.Column("col3", "Время захода в депо", "date"),
                new DevExtremeTableData.Column("col4", "Время постановки на позицию", "string"),
                new DevExtremeTableData.Column("col5", "Инспекция", "string"),
                new DevExtremeTableData.Column("col6", "Время окончания ремонта", "date"),
                new DevExtremeTableData.Column("col7", "Начало проверки под напряжением", "date"),
                new DevExtremeTableData.Column("col8", "Окончание проверки под напряжением", "date"),
                new DevExtremeTableData.Column("col9", "Проверяющий", "string"),
            };

            result.Rows = new List<DevExtremeTableData.Row>();

            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    result.Rows.Add(new DevExtremeTableData.Row
                    {
                        Id = new DevExtremeTableData.RowId { Id = item.Id, Type = 0 },
                        HasItems = false.ToString(),
                        Col0 = item.Parking.Name,
                        Col1 = item.Train.Name,
                        Col2 = item.Route.Name,
                        Col3 = item.InTime.ToStringDateTime(),
                        Col4 = item.ParkingTime?.ToStringDateTime(),
                        Col5 = item.Inspection == null ? item.InspectionTxt : item.Inspection.Name,
                        Col6 = item.RepairStopTime?.ToStringDateTime(),
                        Col7 = item.TestStartTime?.ToStringDateTime(),
                        Col8 = item.TestStopTime?.ToStringDateTime(),
                        Col9 = item.TesterUser?.Name
                    });
                }
            }

            result.Rows = DevExtremeTableUtils.DevExtremeTableFiltering(result.Rows, input.Filters);
            result.Rows = DevExtremeTableUtils.DevExtremeTableSorting(result.Rows, input.Sortings);
            result.Total = result.Rows.Count.ToString();
            result.Paging(input.Paging);

            return result;
        }

        private static DepoEventDto BindToModel(IReadOnlyList<object> objects)
        {
            var ret = new DepoEventDto();
            if (objects[0] is DepoEvent depoevent)
            {
                ret.Id = depoevent.Id;
                ret.InspectionTxt = depoevent.InspectionTxt;
                ret.InTime = depoevent.InTime;
                ret.ParkingTime = depoevent.ParkingTime;
                ret.RepairStopTime = depoevent.RepairStopTime;
                ret.TestStartTime = depoevent.TestStartTime;
                ret.TestStopTime = depoevent.TestStopTime;
            }

            if (objects[1] is Parking parking)
            {
                ret.Parking = new LiveSearchItemDto
                {
                    Id = parking.Id,
                    Name = parking.Name
                };
            }

            if (objects[2] is Inspection inspection)
            {
                var brigade = objects[4] as Brigade;
                ret.Inspection = new LiveSearchItemDto
                {
                    Id = inspection.Id,
                    Name = GetInspectionString(inspection, brigade?.BrigadeType)
                };
            }

            if (objects[5] is Train train)
            {
                ret.Train = new LiveSearchItemDto
                {
                    Id = train.Id,
                    Name = train.Name
                };
            }

            if (objects[6] is Route route)
            {
                ret.Route = new LiveSearchItemDto
                {
                    Id = route.Id,
                    Name = route.Name
                };
            }

            if (objects[7] is User user)
            {
                ret.TesterUser = new LiveSearchItemDto
                {
                    Id = user.Id,
                    Name = user.Name
                };
            }

            return ret;
        }

        private static string GetInspectionString(Inspection inspection, BrigadeType? brigade)
        {
            var date = inspection.DateEnd.HasValue
                ? $"{inspection.DateStart:dd.MM.yyyy HH:mm} - {inspection.DateEnd:dd.MM.yyyy HH:mm}"
                : inspection.DateStart.ToString("dd.MM.yyyy HH:mm");

            return $"{inspection.CheckListType.GetDescription()}{(brigade.HasValue ? $"({brigade.Value.ShortDescription()})" : "")} {date}";
        }

        private static string GetStringBrigadeType(BrigadeType brigadeType)
        {
            switch (brigadeType)
            {
                case BrigadeType.Locomotiv:
                    return "ЛБ";
                case BrigadeType.Depo:
                    return "БД";
                case BrigadeType.Receiver:
                    return "БП";
                default:
                    return "?";
            }
        }


        private static string GetStringCheckListType(CheckListType checkListType)
        {
            switch (checkListType)
            {
                case CheckListType.TO1:
                    return "ТО1";
                case CheckListType.TO2:
                    return "ТО1";
                case CheckListType.Inspection:
                    return "Инспекция";
                case CheckListType.Surrender:
                    return "Приемка";
                default:
                    return "?";
            }
        }
    }

    public class LiveSearchItemDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime? DateStart { get; set; }

        public DateTime DateEnd { get; set; }
    }

    public class LiveSearchRouteTrainItemDto : LiveSearchItemDto
    {
        public int TrainId { get; set; }
    }

    public class DepoEventDto
    {
        public int Id { get; set; }

        public LiveSearchItemDto Parking { get; set; }

        public LiveSearchItemDto Inspection { get; set; }

        public string InspectionTxt { get; set; }

        public LiveSearchItemDto Train { get; set; }

        public LiveSearchItemDto Route { get; set; }

        public DateTime InTime { get; set; }

        public DateTime? ParkingTime { get; set; }

        public DateTime? RepairStopTime { get; set; }

        public DateTime? TestStartTime { get; set; }

        public DateTime? TestStopTime { get; set; }

        public LiveSearchItemDto TesterUser { get; set; }
    }

    public class DepoEventsRequest
    {
        //public DepoEventsFilter Filter { get; set; }
        //public EventTableData.RowId ParentId { get; set; }
        public DevExtremeTableData.Paging Paging { get; set; }
        public List<DevExtremeTableData.Filter> Filters { get; set; }
        public List<DevExtremeTableData.Sorting> Sortings { get; set; }
    }

    //public class DepoEventsFilter
    //{
    //    public DateTime? StartDate { get; set; }
    //    public DateTime? EndDate { get; set; }
    //}
}