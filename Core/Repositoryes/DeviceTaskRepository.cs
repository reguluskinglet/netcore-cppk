using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Dto;
using Rzdppk.Model.Enums;
using TaskStatus = System.Threading.Tasks.TaskStatus;

namespace Rzdppk.Core.Repositoryes
{
    public class DeviceTaskRepository : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IDb _db;

        public DeviceTaskRepository(ILogger logger)
        {
            _db = new Db();
            _logger = logger;
        }

        public DeviceTaskRepository(IDb db)
        {
            _db = db;
        }

        public DeviceTaskRepository()
        {
            _db = new Db();
        }

        public async Task<DevExtremeTableData.ReportResponse> GetTable(DeviceTaskRequest input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var result = new DevExtremeTableData.ReportResponse();

                const string sql = @"SELECT t.*,d.*,f.*,u.*,LastComment.* FROM [DeviceTasks] t
                                    LEFT JOIN [Devices] d on d.Id=t.DeviceId
                                    LEFT JOIN [DeviceFaults] f on f.Id=t.DeviceFaultId
                                    LEFT JOIN [auth_users] u on u.Id=t.UserId
                                    OUTER APPLY (SELECT TOP 1 * FROM [DeviceTaskComments] c WHERE t.Id = c.DeviceTaskId ORDER BY c.Date DESC) as LastComment
                                    ORDER BY t.CreateDate DESC";

                var items = (await conn.QueryAsync<DeviceTask, Device, DeviceFault, User, DeviceTaskComment, DeviceTaskItemDto>(
                    sql, (task, device, fault, user, comment) =>
                    {
                        return new DeviceTaskItemDto
                        {
                            Id = task.Id,
                            Date = task.CreateDate,
                            Device = $"{device.Name} ({device.CellNumber})",
                            DeviceFault = fault.Name,
                            User = user.Name,
                            Status = TaskStatusEnumtoString(comment.Status)
                        };
                    })).ToList();

                result.Columns = new List<DevExtremeTableData.Column>
                {
                    new DevExtremeTableData.Column("col0", "ИД", "number"),
                    new DevExtremeTableData.Column("col1", "Статус", "string"),
                    new DevExtremeTableData.Column("col2", "Устройство", "string"),
                    new DevExtremeTableData.Column("col3", "Инициатор", "string"),
                    new DevExtremeTableData.Column("col4", "Дата", "date"),
                    new DevExtremeTableData.Column("col5", "Типовая неисправность", "string"),
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
                            Col0 = item.Id.ToString(),
                            Col1 = item.Status,
                            Col2 = item.Device,
                            Col3 = item.User,
                            Col4 = item.Date.ToStringDateTime(),
                            Col5 = item.DeviceFault
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

        public async Task<DeviceTaskDto> GetById(int id, bool loadComments = true)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                const string sql = @"SELECT t.*,d.*,f.*,u.*,LastComment.* FROM [DeviceTasks] t
                                    LEFT JOIN [Devices] d on d.Id=t.DeviceId
                                    LEFT JOIN [DeviceFaults] f on f.Id=t.DeviceFaultId
                                    LEFT JOIN [auth_users] u on u.Id=t.UserId
                                    OUTER APPLY (SELECT TOP 1 * FROM [DeviceTaskComments] c WHERE t.Id = c.DeviceTaskId ORDER BY c.Date DESC) as LastComment
                                    WHERE t.Id=@Id";

                var ret = (await conn.QueryAsync<DeviceTask, Device, DeviceFault, User, DeviceTaskComment, DeviceTaskDto>(
                    sql, (task, device, fault, user, comment) =>
                    {
                        return new DeviceTaskDto
                        {
                            Id = task.Id,
                            CreateDate = task.CreateDate,
                            Device = new ClassifierDto {Id = device.Id, Name = $"{device.Name} ({device.CellNumber})"},
                            DeviceFault = new ClassifierDto {Id = fault.Id, Name = fault.Name},
                            Description = task.Description,
                            User = new ClassifierDto {Id = user.Id, Name = user.Name},
                            Status = new DeviceTaskStatusDto
                            {
                                Id = comment.Status,
                                Name = TaskStatusEnumtoString(comment.Status)
                            }
                        };
                    }, new {Id = id})).First();

                if (loadComments)
                {
                    const string sqlc =
                        "SELECT * FROM [DeviceTaskComments] c LEFT JOIN [auth_users] u on u.Id=c.UserId WHERE c.DeviceTaskId = @Id ORDER BY [Date] DESC";

                    var comments = (await conn.QueryAsync<DeviceTaskComment, User, DeviceCommentDto>(
                        sqlc, (comment, user) =>
                        {
                            return new DeviceCommentDto
                            {
                                Date = comment.Date,
                                Text = comment.Text,
                                User = user.Name,
                                Status = TaskStatusEnumtoString(comment.Status)
                            };
                        }, new {Id = id})).ToList();

                    ret.Comments = comments;
                }

                return ret;
            }
        }

        public async Task<DeviceTaskDto> Add(DeviceTaskDto input)
        {
            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var conn = new SqlConnection(AppSettings.ConnectionString))
                {
                    const string sql = "INSERT INTO [DeviceTasks] ([CreateDate],[Description],[DeviceId],[UserId],[DeviceFaultId]) VALUES(@Date,@Description,@DeviceId,@UserId,@FaultId) SELECT SCOPE_IDENTITY()";

                    var id = await conn.QueryFirstOrDefaultAsync<int>(sql,
                        new
                        {
                            Date = input.CreateDate,
                            Description = input.Description,
                            DeviceId = input.Device.Id,
                            UserId = input.User.Id,
                            FaultId = input.DeviceFault.Id
                        });

                    const string sqlc = "INSERT INTO [DeviceTaskComments] ([DeviceTaskId],[UserId],[Status],[Date]) VALUES (@DeviceTaskId,@UserId,@Status,@Date)";

                    await conn.ExecuteAsync(sqlc,
                        new
                        {
                            DeviceTaskId = id,
                            UserId = input.User.Id,
                            Status = TaskStatus.Created,
                            Date = input.CreateDate
                        });

                    var ret = await GetById(id);

                    transaction.Complete();

                    return ret;
                }
            }
        }

        public async Task SaveCommentAndStatus(DeviceTaskCommentDto input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var task = await GetById(input.DeviceTaskId);

                if (task.Status.Id != input.Status.Id || !string.IsNullOrEmpty(input.Text))
                {
                    var SqlCreateRemoteTaskComment =
                        "INSERT INTO [DeviceTaskComments] ([DeviceTaskId],[UserId],[Status],[Date],[Text]) VALUES (@DeviceTaskId,@UserId,@Status,@Date,@Text)";

                    await conn.ExecuteAsync(SqlCreateRemoteTaskComment,
                        new
                        {
                            DeviceTaskId = input.DeviceTaskId,
                            UserId = input.User.Id,
                            Status = input.Status.Id,
                            Date = input.Date,
                            Text = input.Text
                        });
                }
            }
        }

        //с ящика
        public async Task CreateTask(TaskOutDto task)
        {
            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var conn = new SqlConnection(AppSettings.ConnectionString))
                {
                    var SqlCreateRemoteTask =
                        "INSERT INTO [DeviceTasks] ([DeviceId],[UserId],[DeviceFaultId],[CreateDate],[RefId]) VALUES (@DeviceId,@UserId,@DeviceFaultId,@CreateDate,@RefId) " +
                        " SELECT SCOPE_IDENTITY()";

                    var taskId = await conn.ExecuteScalarAsync<int>(SqlCreateRemoteTask,
                        new
                        {
                            task.DeviceId,
                            task.UserId,
                            task.DeviceFaultId,
                            task.CreateDate,
                            task.RefId
                        });

                    var SqlCreateRemoteTaskComment =
                        "INSERT INTO [DeviceTaskComments] ([DeviceTaskId],[UserId],[Status],[Date]) VALUES (@DeviceTaskId,@UserId,@Status,@Date)";

                    await conn.ExecuteAsync(SqlCreateRemoteTaskComment,
                        new
                        {
                            DeviceTaskId = taskId,
                            UserId = task.UserId,
                            Status = TaskStatus.Created,
                            Date = task.CreateDate
                        });

                    transaction.Complete();
                }
            }
        }

        public async Task<int> GetDeviceOpenTaskCount(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                const string sql =
                    "SELECT Count(*) FROM DeviceTasks t OUTER APPLY(SELECT TOP 1 * FROM DeviceTaskComments c WHERE t.Id = c.DeviceTaskId ORDER BY c.Date DESC) as LastComment WHERE LastComment.Status <> 2 And t.DeviceId = @Id";

                var cnt = await conn.QuerySingleAsync<int>(sql, new {Id = id});

                return cnt;
            }
        }

        public List<DeviceTaskStatusDto> GetAllStatuses()
        {
            var ret = new List<DeviceTaskStatusDto>();

            foreach (DeviceTaskStatus item in Enum.GetValues(typeof(DeviceTaskStatus)))
            {
                ret.Add(new DeviceTaskStatusDto
                {
                    Id = item,
                    Name = TaskStatusEnumtoString(item)
                });
            }

            return ret;
        }

        public static string TaskStatusEnumtoString(DeviceTaskStatus status)
        {
            var ret = "?";

            switch (status)
            {
                case DeviceTaskStatus.Closed:
                    ret = "закрыт";
                    break;
                case DeviceTaskStatus.Create:
                    ret = "новый";
                    break;
                case DeviceTaskStatus.InWork:
                    ret = "в работе";
                    break;
            }

            return ret;
        }

        public void Dispose()
        {
            _db.Connection?.Close();
        }
    }

    public class DeviceTaskItemDto
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public string Device { get; set; }

        public string User { get; set; }

        public DateTime Date { get; set; }

        public string DeviceFault { get; set; }

        public string Description { get; set; }
    }

    public class DeviceTaskDto
    {
        public int? Id { get; set; }

        public ClassifierDto User { get; set; }

        public ClassifierDto Device { get; set; }

        public DateTime CreateDate { get; set; }

        public string Description { get; set; }

        public DeviceTaskStatusDto Status { get; set; }

        public ClassifierDto DeviceFault { get; set; }

        public List<DeviceCommentDto> Comments { get; set; }
    }

    public class DeviceTaskStatusDto
    {
        public DeviceTaskStatus Id { get; set; }

        public string Name { get; set; }
    }

    public class ClassifierDto
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class DeviceCommentDto
    {
        public DateTime Date { get; set; }

        public string Status { get; set; }

        public string Text { get; set; }

        public string User { get; set; }
    }

    public class DeviceTaskCommentDto
    {
        public DateTime Date { get; set; }

        public string Text { get; set; }

        public User User { get; set; }

        public int DeviceTaskId { get; set; }

        public DeviceTaskStatusDto Status { get; set; }
    }

    public class DeviceTaskRequest
    {
        public DevExtremeTableData.Paging Paging { get; set; }
        public List<DevExtremeTableData.Filter> Filters { get; set; }
        public List<DevExtremeTableData.Sorting> Sortings { get; set; }
    }
}
