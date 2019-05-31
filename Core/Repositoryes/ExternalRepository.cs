using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rzdppk.Core.Helpers;
using Rzdppk.Core.Old;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;
using Rzdppk.Model.Terminal;
using Stantion = Rzdppk.Model.Stantion;

namespace Rzdppk.Core.Repositoryes
{
    public class ExternalRepository: IExternalRepository
    {
        private readonly IDb _db;
        private readonly IFileService _fileService;

        public ExternalRepository
            (
                IFileService fileService,
                IDb db
            )
        {
            _db = db;
            _fileService = fileService;
        }

        public async Task<bool> SaveDeviceValue(DeviceValueDto model)
        {
            var deviceId = await _db.Connection.QueryFirstOrDefaultAsync<int?>(Sql.SqlQueryCach["Device.GetDeviceIdBySerial"],
                new {serial = model.Serial});

            if (!deviceId.HasValue)
                return false;

            model.DeviceId = deviceId.Value;

            await _db.Connection.ExecuteAsync(Sql.SqlQueryCach["Device.InsertDeviceValue"], model);

            return true;
        }

        public bool SaveDocuments(List<UploadDocumentDto> documents)
        {
            var lockupTaskComment = new Dictionary<Guid, int>();

            _db.Start((transaction, connection) =>
            {
                foreach (var item in documents)
                {
                    var document = new Document
                    {
                        Description = "UploadByMobile.jpg",
                        Name = item.Name,
                        RefId = item.Id,
                        UpdateDate = item.UpdateDate,
                        DocumentType = DocumentType.Image
                    };

                    if(item.TrainTaskCommentTerminalId.HasValue)
                        document.TrainTaskCommentId = GetTaskCommentByRefId(transaction, connection, lockupTaskComment, item.TrainTaskCommentTerminalId.Value);

                    connection.Execute(Sql.SqlQueryCach["Terminal.InsertDocument"], document, transaction);

                    _fileService.Save(item.File, item.Name);
                }
            });

            return true;
        }

        public bool SaveTerminalResult(TerminalResultDto model)
        {
            _db.Start((transaction, connection) =>
            {
                var lockupTask = new Dictionary<Guid, int>();
                var lockupInspection = new Dictionary<Guid, int>();
                var lockupTaskStatus = new Dictionary<Guid, int>();

                if (model.InspectionTerminalDtos != null)
                {
                    var inds = model.InspectionTerminalDtos.Select(x => x.Id);

                    lockupInspection = connection.Query<InspectionModel>(Sql.SqlQueryCach["Terminal.GetInspectionIds"],new {refId = inds },transaction).ToDictionary(x=>x.RefId,x=>x.Id);

                    foreach (var inspection in model.InspectionTerminalDtos)
                    {
                        if (lockupInspection.TryGetValue(inspection.Id, out int inspectionId))
                        {
                            inspection.RefId = inspectionId;
                            connection.Execute(Sql.SqlQueryCach["Terminal.UpdateInspection"], inspection, transaction);
                        }
                        else
                        {
                            inspection.RefId = connection.QueryFirst<int>(Sql.SqlQueryCach["Terminal.InsertInspection"], inspection, transaction);
                            lockupInspection.Add(inspection.Id, inspection.RefId.Value);
                        }
                    }
                }

                if (model.TrainTaskTerminalDtos != null)
                {
                    var inds = model.TrainTaskTerminalDtos.Select(x => x.Id);
                    lockupTask= connection.Query<InspectionModel>(Sql.SqlQueryCach["Terminal.GetTrainTaskIds"], new { refId = inds }, transaction).ToDictionary(x => x.RefId, x => x.Id);

                    foreach (var task in model.TrainTaskTerminalDtos)
                    {
                        if (lockupTask.TryGetValue(task.Id, out int taskId))
                        {
                            task.RefId = taskId;
                            connection.Execute(Sql.SqlQueryCach["Terminal.UpdateTrainTask"], task, transaction);
                        }
                        else
                        {
                            var id = connection.QueryFirst<int>(Sql.SqlQueryCach["Terminal.InsertTrainTask"], task, transaction);

                            lockupTask.Add(task.Id, id);
                        }
                    }
                }

                if (model.TrainTaskAttributeTerminalDtos != null)
                {
                    foreach (var attribute in model.TrainTaskAttributeTerminalDtos)
                    {
                        if (attribute.InspectionId.HasValue)
                            attribute.InspectionRefId = GetInspectionByRefId(transaction, connection, lockupInspection, attribute.InspectionId.Value);

                        attribute.TrainTaskRefId= GetTaskByRefId(transaction, connection, lockupTask, attribute.TrainTaskId);

                        var id = connection.QueryFirst<int>(Sql.SqlQueryCach["Terminal.InsertTrainTaskAttributes"], attribute, transaction);

                        lockupTask.Add(attribute.Id, id);
                    }
                }

                if (model.TrainTaskStatusDtos != null)
                {
                    foreach (var status in model.TrainTaskStatusDtos)
                    {
                        status.RefId = GetTaskByRefId(transaction, connection, lockupTask, status.TrainTaskId);
                        var id= connection.QueryFirst<int>(Sql.SqlQueryCach["Terminal.InsertTrainTaskStatus"], status, transaction);

                        lockupTaskStatus.Add(status.Id, id);
                    }
                }

                if (model.TrainTaskCommentDtos != null)
                {
                    foreach (var comment in model.TrainTaskCommentDtos)
                    {
                        comment.RefId = GetTaskByRefId(transaction, connection,lockupTask, comment.TrainTaskId);
                        connection.Execute(Sql.SqlQueryCach["Terminal.InsertTrainTaskComment"], comment, transaction);
                    }
                }

                if (model.TrainTaskExecutorDtos != null)
                {
                    foreach (var executor in model.TrainTaskExecutorDtos)
                    {
                        executor.RefId = GetTaskByRefId(transaction, connection, lockupTask, executor.TrainTaskId);
                        connection.Execute(Sql.SqlQueryCach["Terminal.InsertTrainTaskExecutor"], executor, transaction);
                    }
                }

                if (model.MeterageDtos != null)
                {
                    foreach (var meterage in model.MeterageDtos)
                    {
                        if (meterage.InspectionId.HasValue)
                            meterage.InspectionRefId = GetInspectionByRefId(transaction, connection, lockupInspection, meterage.InspectionId.Value);

                        if(meterage.TaskStatusId.HasValue)
                            meterage.TaskStatusRefId = GetTaskStatusByRefId(transaction, connection, lockupTaskStatus, meterage.TaskStatusId.Value);

                        connection.Execute(Sql.SqlQueryCach["Terminal.InsertMeterage"], meterage, transaction);
                    }
                }

                if (model.InspectionDataTerminals != null)
                {
                    foreach (var inspectionData in model.InspectionDataTerminals)
                    {
                        inspectionData.InspectionRefId = GetInspectionByRefId(transaction, connection, lockupInspection,
                            inspectionData.InspectionTerminalId);

                        connection.Execute(Sql.SqlQueryCach["Terminal.InsertInspectionData"], inspectionData, transaction);
                    }
                }

                if (model.SignatureDtos != null)
                {
                    foreach (var signature in model.SignatureDtos)
                    {
                        signature.InspectionRefId = GetInspectionByRefId(transaction, connection, lockupInspection,
                            signature.InspectionId);

                        connection.Execute(Sql.SqlQueryCach["Terminal.InsertSignature"], signature, transaction);
                    }
                }

                if (model.DeviceTasks != null)
                {
                    foreach (var deviceTask in model.DeviceTasks)
                    {
                        connection.Execute(Sql.SqlQueryCach["Terminal.InsertDeviceTask"], deviceTask, transaction);
                    }
                }
            });

            return true;
        }

        private int GetTaskByRefId(SqlTransaction transaction, SqlConnection connection, Dictionary<Guid, int> lockupTask, Guid refId)
        {
            if (!lockupTask.TryGetValue(refId, out int taskId))
            {
                taskId = connection.QueryFirst<int>(Sql.SqlQueryCach["Terminal.GetTaskByRefId"], new { id = refId }, transaction);
                lockupTask.Add(refId, taskId);
            }

            return taskId;
        }

        private int GetTaskStatusByRefId(SqlTransaction transaction, SqlConnection connection, Dictionary<Guid, int> lockupTaskStatus, Guid refId)
        {
            if (!lockupTaskStatus.TryGetValue(refId, out int taskId))
            {
                taskId = connection.QueryFirst<int>(Sql.SqlQueryCach["Terminal.GetTaskStatusByRefId"], new { id = refId }, transaction);
                lockupTaskStatus.Add(refId, taskId);
            }

            return taskId;
        }

        private int GetInspectionByRefId(SqlTransaction transaction, SqlConnection connection, Dictionary<Guid, int> lockup, Guid refId)
        {
            if (!lockup.TryGetValue(refId, out int taskId))
            {
                taskId = connection.QueryFirst<int>(Sql.SqlQueryCach["Terminal.GetInspectionByRefId"], new { id = refId }, transaction);
                lockup.Add(refId, taskId);
            }

            return taskId;
        }

        private int GetTaskCommentByRefId(SqlTransaction transaction, SqlConnection connection, Dictionary<Guid, int> lockup, Guid refId)
        {
            if (!lockup.TryGetValue(refId, out int taskId))
            {
                taskId = connection.QueryFirst<int>(Sql.SqlQueryCach["Terminal.GetTaskCommentByRefId"], new { id = refId }, transaction);
                lockup.Add(refId, taskId);
            }

            return taskId;
        }

        public async Task<object> GetAllData(DateTime? date)
        {
            var brigades = await _db.GetAllWithFilter<Brigade>(date);
            var roles = await _db.GetAllWithFilter<UserRole>(date, "auth_roles");
            var users = await _db.GetAllWithFilter<User>(date, "auth_users");
            var equipmentCategoryes = await _db.GetAllWithFilter<EquipmentCategory>(date, "EquipmentCategoryes");
            var equipments = await _db.GetAllWithFilter<Equipment>(date);
            var faults = await _db.GetAllWithFilter<Fault>(date);
            var faultEquipments = await _db.GetAllWithFilter<FaultEquipment>(date);
            var models = await _db.GetAllWithFilter<Model.Model>(date);
            var equipmentModels = await _db.GetAllWithFilter<EquipmentModel>(date);
            var checkListEquipments = await _db.GetAllWithFilter<CheckListEquipment>(date);
            var stantions = await _db.GetAllWithFilter<Stantion>(date);
            var routes = await _db.GetAllWithFilter<Route>(date);
            var trips = await _db.GetAllWithFilter<Trip>(date);
            //var tripRoutes = await _db.GetAllWithFilter<TripRoute>(date);
           // var stantionTrips = await _db.GetAllWithFilter<StantionTrip>(date);
            var trains = await _db.GetAllWithFilter<Train>(date);
            var carriages = await _db.GetAllWithFilter<Carriage>(date);
            var labels = await _db.GetAllWithFilter<Label>(date);


            var inspections = await GetEntities<InspectionTerminalDto>(date, "GetInspections");
            var tasks = await GetTrainTasks(date);
            var comments = await GetTrainTaskComments(date);
            var statuses= await GetEntities<TrainTaskStatusDto>(date, "GetTrainTaskStatuses");
            var executors = await GetEntities<TrainTaskExecutorDto>(date, "GetTrainTaskExecutors");

            var documents = await GetEntities<DocumentDto>(date, "GetDocuments", true);
            var taskAttributes = await GetEntities<TrainTaskAttributeTerminalDto>(date, "GetTrainTaskAttributes");

            var devices= await _db.GetAllWithFilter<Device>(date);
            var deviceFaults= await _db.GetAllWithFilter<DeviceFault>(date);

            var brigadeRouteTrains = (await GetBrigadeTrains(date)).ToList();
            var inspectionRouteTrains= (await GetInspectionRouteTrains(date)).ToList();

            if (IsNull(brigades) &&
                IsNull(roles) &&
                IsNull(users) &&
                IsNull(equipmentCategoryes) &&
                IsNull(equipments) &&
                IsNull(faults) &&
                IsNull(faultEquipments) &&
                IsNull(models) &&
                IsNull(equipmentModels) &&
                IsNull(checkListEquipments) &&
                IsNull(stantions) &&
                IsNull(routes) &&
                IsNull(trips) &&

                IsNull(devices) &&
                IsNull(deviceFaults) &&

                //IsNull(tripRoutes) &&
                //  IsNull(stantionTrips) &&
                IsNull(trains) &&
                IsNull(carriages) &&
                IsNull(labels) &&
                IsNull(taskAttributes) &&
                IsNull(tasks) &&
                IsNull(comments) &&
                IsNull(statuses) &&
                IsNull(executors) &&
                IsNull(documents) &&
                IsNull(inspectionRouteTrains) &&
                IsNull(brigadeRouteTrains)
            )
                return null;


            return new
            {
                Devices=devices,
                DeviceFaults=deviceFaults,
                Brigades = brigades,
                Roles = roles,
                Users = users,

                EquipmentCategoryes = equipmentCategoryes,
                Equipments = equipments,

                Faults = faults,
                FaultEquipments= faultEquipments,

                Models = models,
                EquipmentModels= equipmentModels,

                CheckListEquipments= checkListEquipments,

                Stantions = stantions,
                Routes = routes,
                Trips = trips,
                //TripRoutes= tripRoutes,
               // StantionTrips= stantionTrips,

                Trains= trains,
                Carriages = carriages,

                Labels= labels,

                Inspections= inspections,
                //Meterage
                //Signatures
                TrainTasks =tasks,
                TrainTaskAttributes = taskAttributes,
                TrainTaskStatuses =statuses,
                TrainTaskComments=comments,
                TrainTaskExecutors=executors,
                Documents = documents,

                BrigadeRouteTrains = brigadeRouteTrains,
                InspectionRouteTrains=inspectionRouteTrains
            };
        }

        private bool IsNull<TEntity>(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                return true;

            return !entities.Any();
        }

        private async Task<List<TrainTaskTerminalDto>> GetTrainTasks(DateTime? date)
        {
            var sql = Sql.SqlQueryCach["Terminal.GetTrainTask"];

            if (date.HasValue)
                sql = sql.ReplaceWhere("WHERE t.UpdateDate>@date");

            var tasks = await _db.Connection.QueryAsync<TrainTaskTerminalDto>(sql,new{ date = date });

            return tasks.ToList();
        }

        private async Task<List<TrainTaskCommentDto>> GetTrainTaskComments(DateTime? date)
        {
            var sql = Sql.SqlQueryCach["Terminal.GetTrainTaskComments"];

            if (date.HasValue)
                sql = sql.ReplaceWhere("WHERE c.UpdateDate>@date");

            var tasks = await _db.Connection.QueryAsync<TrainTaskCommentDto>(sql, new { date = date });

            return tasks.ToList();
        }


        private async Task<List<TEntity>> GetEntities<TEntity>(DateTime? date, string command, bool hasReplaceAnd=false)
        {
            try
            {
                var sql = Sql.SqlQueryCach[$"Terminal.{command}"];

                if (date.HasValue)
                    sql = hasReplaceAnd
                        ? sql.ReplaceAnd("and x.UpdateDate>@date")
                        : sql.ReplaceWhere("WHERE x.UpdateDate>@date");

                var tasks = await _db.Connection.QueryAsync<TEntity>(sql, new { date = date });

                return tasks.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private Task<IEnumerable<TerminalBrigadeRouteTrainDto>> GetBrigadeTrains(DateTime? date)
        {
            var sql = Sql.SqlQueryCach["Terminal.GetBrigadeRouteTrain"];

            if (date.HasValue)
                sql += Environment.NewLine + "WHERE x.UpdateDate>@date or chb.UpdateDate>@date";

            return _db.Connection.QueryAsync<TerminalBrigadeRouteTrainDto>(sql, new { date = date });
        }


        private Task<IEnumerable<TerminalInspectionRouteTrainDto>> GetInspectionRouteTrains(DateTime? date)
        {
            var sql = Sql.SqlQueryCach["Terminal.GetInspectionRouteTrain"];

            if (date.HasValue)
                sql += Environment.NewLine + "WHERE pir.UpdateDate>@date or chir.UpdateDate>@date";

            return _db.Connection.QueryAsync<TerminalInspectionRouteTrainDto>(sql, new { date = date });
        }

        public class InspectionModel
        {
            public int Id { get; set; }
            public Guid RefId { get; set; }
        }

        public class TerminalBrigadeRouteTrainDto
        {
            public int Id { get; set; }
            public int PlanedRouteTrainId { get; set; }
            public int StantionEndId { get; set; }
            public int StantionStartId { get; set; }
            public int UserId { get; set; }
            public DateTime Date { get; set; }
            public BrigadeType BrigadeType { get; set; }
            public int TrainId { get; set; }
            public int RouteId { get; set; }

            public int? ChangeUserId { get; set; }

            public int CurrentUserId => ChangeUserId ?? UserId;
        }

        public class TerminalInspectionRouteTrainDto
        {
            public int Id { get; set; }
            public CheckListType CheckListType { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public DateTime Date { get; set; }
            public int RouteId { get; set; }
            public int TrainId { get; set; }

            public DateTime? ChangeStart { get; set; }
            public DateTime? ChangeEnd { get; set; }

            public DateTime CurrentStart => ChangeStart ?? Start;
            public DateTime CurrentEnd => ChangeEnd ?? End;
        }

        public class TerminalResultDto
        {
            public List<TrainTaskTerminalDto> TrainTaskTerminalDtos { get; set; }
            public List<TrainTaskAttributeTerminalDto> TrainTaskAttributeTerminalDtos { get; set; }
            public List<TrainTaskCommentDto> TrainTaskCommentDtos { get; set; }
            public List<TrainTaskStatusDto> TrainTaskStatusDtos { get; set; }
            public List<TrainTaskExecutorDto> TrainTaskExecutorDtos { get; set; }
            public List<DocumentDto> DocumentDtos { get; set; }
            public List<InspectionTerminalDto> InspectionTerminalDtos { get; set; }
            public List<InspectionDataTerminalDto> InspectionDataTerminals { get; set; }
            public List<MeterageDto> MeterageDtos { get; set; }

            public List<SignatureDto> SignatureDtos { get; set; }
            public List<DeviceTask> DeviceTasks { get; set; }
        }

        public class SignatureDto : TrainTaskTerminalDtoBase
        {
            public int UserId { get; set; }
            public Guid InspectionId { get; set; }
            public int? InspectionRefId { get; set; }
            public string CaptionImage { get; set; }
        }

        public class TrainTaskTerminalDtoBase
        {
            public Guid Id { get; set; }
            public DateTime UpdateDate { get; set; }
            public int? RefId { get; set; }
        }

        public class MeterageDto
        {
            public DateTime Date { get; set; }

            public int? Value { get; set; }

            public int LabelId { get; set; }

            public Guid? InspectionId { get; set; }

            public DateTime UpdateDate { get; set; }

            public int? InspectionRefId { get; set; }

            public bool IsRfidScaned { get; set; }

            public Guid? TaskStatusId { get; set; }
            public int? TaskStatusRefId { get; set; }
        }

        public class InspectionDataTerminalDto : TrainTaskTerminalDtoBase
        {
            public InspectionDataType Type { get; set; }

            public int Value { get; set; }

            public Guid InspectionTerminalId { get; set; }

            public int InspectionRefId { get; set; }

            public int? CarriageId { get; set; }

            public string Text { get; set; }
        }

        public class InspectionTerminalDto : TrainTaskTerminalDtoBase
        {
            public CheckListType CheckListType { get; set; }
            public DateTime DateStart { get; set; }
            public DateTime? DateEnd { get; set; }
            public InspectionStatus Status { get; set; }
            public int UserId { get; set; }
            public int TrainId { get; set; }
        }

        public class TrainTaskTerminalDto: TrainTaskTerminalDtoBase
        {
            public int TaskNumber { get; set; }
            public DateTime CreateDate { get; set; }
            public string Description { get; set; }
            public TaskType TaskType { get; set; }
            public int CarriageId { get; set; }
            public int EquipmentModelId { get; set; }
            public int UserId { get; set; }
            public User User { get; set; }
        }

        public class TrainTaskAttributeTerminalDto : TrainTaskTerminalDtoBase
        {
            public Guid TrainTaskId { get; set; }
            public int? TrainTaskRefId { get; set; }

            public Guid? InspectionId { get; set; }
            public int? InspectionRefId { get; set; }

            public int? CheckListEquipmentId { get; set; }
            public int? FaultId { get; set; }
            public int UserId { get; set; }
            public int? Value { get; set; }
            public TaskLevel? TaskLevel { get; set; }

            public string Description { get; set; }
        }

        public class TrainTaskCommentDto: TrainTaskTerminalDtoBase
        {
            public Guid TrainTaskId { get; set; }

            public DateTime Date { get; set; }

            public int UserId { get; set; }

            public string Text { get; set; }
        }

        public class TrainTaskStatusDto: TrainTaskTerminalDtoBase
        {
            public Guid TrainTaskId { get; set; }

            public Model.Enums.TaskStatus Status { get; set; }

            public DateTime Date { get; set; }

            public int UserId { get; set; }
        }

        public class TrainTaskExecutorDto: TrainTaskTerminalDtoBase
        {
            public Guid TrainTaskId { get; set; }

            public DateTime Date { get; set; }

            public BrigadeType BrigadeType { get; set; }

            public int UserId { get; set; }
        }

        public class DocumentDto : TrainTaskTerminalDtoBase
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public DocumentType DocumentType { get; set; }

            public Guid? TrainTaskCommentTerminalId { get; set; }
        }

        public class UploadDocumentDto : DocumentDto
        {
            public IFormFile File { get; set; }

            public int? TrainTaskCommentId { get; set; }
        }

        public class DeviceValueDto
        {
            public int DeviceId { get; set; }

            public DeviceValueType Type { get; set; }

            public int Value { get; set; }

            public string Serial { get; set; }

            public double Lat { get; set; }

            public double Lng { get; set; }
        }
        //public class InspectionDto
        //{
        //    public Guid Id { get; set; }
        //    public CheckListType CheckListType { get; set; }
        //    public DateTime DateStart { get; set; }
        //    public DateTime? DateEnd { get; set; }
        //    public InspectionStatus Status { get; set; }
        //    public int UserId { get; set; }
        //    public int TrainId { get; set; }
        //}

    }
}
