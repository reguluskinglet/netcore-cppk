using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Stantions;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using Rzdppk.Core.Repositoryes.Sqls.Train;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public class ReportRepository : IDisposable
    {
        private readonly ILogger _logger;
        

        public ReportRepository(ILogger logger)
        {
            _logger = logger;
        }

        public ReportItemUI[] GetList()
        {
            var list = new List<ReportItemUI>();
            list.Add(new ReportItemUI
            {
                Id = 1,
                Name = "Отчет по задачам"
            });
            list.Add(new ReportItemUI
            {
                Id = 2,
                Name = "История неисправности оборудования"
            });
            return list.ToArray();
        }

        public async Task<ReportPagingUI> Get(int id, int skip, int limit, string filter = null, string orderby = null)
        {
            var output = new ReportPagingUI();
            switch (id)
            {
                case 1:
                    output = await GetReportTasks(skip, limit);
                    break;

                case 2:
                    output = await GetReportEquipmentTaskHistory(skip, limit, filter, orderby);
                    break;

                default:
                    throw new Exception("report not found");
            }

            return output;
        }

        public async Task<ReportPagingUI> GetReportTasks(int skip, int limit)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = (await conn.QueryAsync<TrainTaskReport1>(
                    $"{ReportCommon.SqlSelect} {ReportCommon.Sql1Common} {ReportCommon.Sql1PagingEnd}",
                    new[]
                    {
                        typeof(TrainTaskReport1), typeof(TrainTaskStatus), typeof(Carriage), typeof(Model.Model), typeof(Train), typeof(EquipmentModel), typeof(Equipment), typeof(User)
                    },
                    objects =>
                    {
                        var task = (TrainTaskReport1) objects[0];
                        task.Status = (TrainTaskStatus) objects[1];
                        task.Carriage = (Carriage) objects[2];
                        task.Carriage.Model = (Model.Model) objects[3];
                        task.Carriage.Train = (Train) objects[4];
                        task.EquipmentModel = (EquipmentModel) objects[5];
                        task.EquipmentModel.Equipment = (Equipment) objects[6];
                        var user = (User) objects[7];
                        user.PasswordHash = null;
                        task.User = user;

                        switch (task.Carriage.Model.ModelType)
                        {
                            case ModelType.HeadVagon:
                                task.CarriageTypeString = Other.Other.CarriageTypeString.HeadVagon;
                                break;
                            case ModelType.TractionVagon:
                                task.CarriageTypeString = Other.Other.CarriageTypeString.TractionVagon;
                                break;
                            case ModelType.TrailerVagon:
                                task.CarriageTypeString = Other.Other.CarriageTypeString.TrailerVagon;
                                break;
                        }

                        return task;
                    }, new {skip = skip, limit = limit, repeat_task_status = Model.Enums.TaskStatus.Remake}
                )).ToArray();

                var sqlc = $"{ReportCommon.SqlCount} {ReportCommon.Sql1Common}";
                var count = conn.ExecuteScalar<int>(sqlc);

                var columns = new List<ReportColumnsUI>
                {
                    new ReportColumnsUI
                    {
                        Name = "Номер Задачи",
                        Type = "int"
                    },
                    new ReportColumnsUI
                    {
                        Name = "Статус",
                        Type = "enum"
                    },
                    new ReportColumnsUI
                    {
                        Name = "Тип",
                        Type = "enum"
                    },
                    new ReportColumnsUI
                    {
                        Name = "Поезд",
                        Type = "string"
                    },
                    new ReportColumnsUI
                    {
                        Name = "Вагон",
                        Type = "string"
                    },
                    new ReportColumnsUI
                    {
                        Name = "Оборудование",
                        Type = "string"
                    },
                    new ReportColumnsUI
                    {
                        Name = "Инициатор",
                        Type = "string"
                    },
                    new ReportColumnsUI
                    {
                        Name = "Дата",
                        Type = "date"
                    },
                    new ReportColumnsUI
                    {
                        Name = "Повторы",
                        Type = "int"
                    }
                };

                var ret = new ReportPagingUI
                {
                    Columns = columns.ToArray(),
                    Rows = result.Select(row => new List<string>
                        {
                            row.Id.ToString(),
                            ((int) (row.Status?.Status ?? Model.Enums.TaskStatus.New)).ToString(),
                            ((int) row.TaskType).ToString(),
                            row.Carriage.Train.Name,
                            //row.Carriage.Train.Name + " " + row.Carriage.Number,
                            row.Carriage.Serial + " (" + row.Carriage.Number + ", " + row.CarriageTypeString + ")", 
                            row.EquipmentModel.Equipment.Name,
                            row.User.Name,
                            row.CreateDate.ToString(),
                            row.Repeats.ToString()
                        })
                        .Select(vals => new ReportValuesUI
                        {
                            Values = vals.ToArray()
                        }).ToArray(),
                    Total = count
                };
                return ret;
            }
        }

        public async Task<ReportPagingUI> GetReportEquipmentTaskHistory(int skip, int limit, string filter, string orderby)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                CreateFilter(filter, out string sqlfilter);
                CreateOrderBy(orderby, out string sqlorderby);
                var result = (await conn.QueryAsync<TrainTaskReport1>(
                    $"{ReportCommon.Sql2Select} {ReportCommon.Sql2Common} {sqlfilter} {sqlorderby} {ReportCommon.Sql2End}",
                    new[]
                    {
                        typeof(TrainTaskReport1), typeof(Carriage), typeof(Train), typeof(EquipmentModel),
                        typeof(Equipment)
                    },
                    objects =>
                    {
                        var task = (TrainTaskReport1) objects[0];
                        task.Carriage = (Carriage) objects[1];
                        task.Carriage.Train = (Train) objects[2];
                        task.EquipmentModel = (EquipmentModel) objects[3];
                        task.EquipmentModel.Equipment = (Equipment) objects[4];
                        return task;
                    }, new {skip = skip, limit = limit}
                )).ToArray();

                var sqlc = $"{TaskCommon.SqlCount} {TaskCommon.SqlCommon}";
                var count = conn.ExecuteScalar<int>(sqlc);

                var columns = new List<ReportColumnsUI>
                {
                    new ReportColumnsUI
                    {
                        Name = "Номер",
                        Type = "int",
                        Alias = "TaskId"
                    },
                    new ReportColumnsUI
                    {
                        Name = "Состав",
                        Type = "string",
                        Alias = "TrainName"
                    },
                    new ReportColumnsUI
                    {
                        Name = "Вагон",
                        Type = "string",
                        Alias = "CarriageName"
                    },
                    new ReportColumnsUI
                    {
                        Name = "Наименование оборудования",
                        Type = "string",
                        Alias = "EquipmentName"
                    },
                    new ReportColumnsUI
                    {
                        Name = "Тип инцидента",
                        Type = "string",
                        Alias = "TaskTypeId"
                    },
                    new ReportColumnsUI
                    {
                        Name = "Время создания инцидента",
                        Type = "date",
                        Alias = "CreateDate"
                    },
                    new ReportColumnsUI
                    {
                        Name = "Время закрытия инцидента",
                        Type = "date",
                        Alias = "CloseDate"
                    },


                };


                var sqlRmodel = new ModelRepository(_logger);
                foreach (var item in result)
                {
                    var carriageTypeId = (int)(await sqlRmodel.GetById(item.Carriage.ModelId)).ModelType;
                    if (carriageTypeId == 0)
                        item.CarriageTypeString = Other.Other.CarriageTypeString.HeadVagon;

                    if (carriageTypeId == 1)
                        item.CarriageTypeString = Other.Other.CarriageTypeString.TractionVagon;

                    if (carriageTypeId == 2)
                        item.CarriageTypeString = Other.Other.CarriageTypeString.TrailerVagon;
                }
                

                var ret = new ReportPagingUI
                {
                    Columns = columns.ToArray(),
                    Rows = result.Select(row => new List<string>
                        {
                            row.Id.ToString(),
                            row.Carriage.Train.Name,
                            row.Carriage.Serial + " (" + row.Carriage.Number + ", " + row.CarriageTypeString + ")",
                            row.EquipmentModel.Equipment.Name,
                            _TaskTypeEnumToString(row.TaskType),
                            row.CreateDate.ToString(),
                            row.UpdateDate.ToString()
                        })
                        .Select(vals => new ReportValuesUI
                        {
                            Values = vals.ToArray()
                        }).ToArray(),
                    Total = count
                };

                return ret;
            }
        }

        private string _TaskTypeEnumToString(TaskType type)
        {
            var ret = "";
            switch (type)
            {
                case TaskType.Cto:
                    ret = "Санитарный";
                    break;
                case TaskType.Service:
                    ret = "Сервисный";
                    break;
                case TaskType.Technical:
                    ret = "Технический";
                    break;
                default:
                    throw new Exception("unknown task type: " + type);
            }

            return ret;
        }

        private static void CreateFilter(string filter, out string sqlfilter)
        {
            sqlfilter = "";
            if (filter != null)
            {
                var filters = JsonConvert.DeserializeObject<FilterBody[]>(filter);
                var listFilters = new List<string>();
                foreach (var item in filters)
                {
                    switch (item.Filter)
                    {
                        case "TrainId":
                            listFilters.Add($"t.Id = {item.Value}");
                            break;

                        case "CarriageId":
                            listFilters.Add($"c.Id = {item.Value}");
                            break;

                        case "TaskId":
                            listFilters.Add($"ts.Id = {item.Value}");
                            break;

                        case "EquipmentName":
                            listFilters.Add($"e.Name = {item.Value}");
                            break;
                    }
                }

                if (listFilters.Any())
                {
                    sqlfilter = "WHERE " + String.Join(" AND ", listFilters.ToArray());
                }
            }
        }

        public static void CreateOrderBy(string orderby, out string sqlorderby)
        {
            sqlorderby = null;
            if (orderby != null)
            {
                var orderbys = JsonConvert.DeserializeObject<OrderBody>(orderby);
                switch (orderbys.Column)
                {
                    case "TrainName":
                        sqlorderby = "t.Name";
                        break;

                    case "CarriageName":;
                        sqlorderby = "c.Name";
                        break;

                    case "TaskId":
                        sqlorderby = "ts.Id";
                        break;

                    case "EquipmentName":
                        sqlorderby = "e.Name";
                        break;

                    case "TaskTypeId":
                        sqlorderby = "ts.TaskType";
                        break;

                    case "CreateDate":
                        sqlorderby = "ts.CreateDate";
                        break;

                    case "CloseDate":
                        sqlorderby = "ts.CloseDate";
                        break;
                }

                var dir = "ASC";
                if (orderbys.Direction.ToLower() == "desc")
                {
                    dir = "DESC";
                }

                if (sqlorderby != null)
                {
                    sqlorderby = $"ORDER BY {sqlorderby} {dir}";
                }
            }

            if (sqlorderby == null)
            {
                sqlorderby = "ORDER BY ts.CreateDate DESC";
            }
        }

        public class FilterBody
        {
            public string Filter { get; set; }
            public string Value { get; set; }
        }

        public class OrderBody
        {
            public string Column { get; set; }
            public string Direction { get; set; }
        }

        public class TrainTaskReport1 : TrainTask
        {
            public TrainTaskStatus Status { get; set; }
            public string CarriageTypeString { get; set; }
            //public Fault Fault { get; set;}
            public int Repeats { get; set; }
            public int Attributes { get; set; }
        }

        public class TrainTaskReport2
        {

        }

        public class ReportItemUI
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class ReportPagingUI
        {
            public ReportColumnsUI[] Columns { get; set; }
            public ReportValuesUI[] Rows { get; set; }
            public int Total { get; set; }
        }

        public class ReportColumnsUI
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Alias { get; set; }
        }

        public class ReportValuesUI
        {
            public string[] Values { get; set; }
        }

        public void Dispose()
        {
            //_db.Connection.Close();
        }
    }
}
