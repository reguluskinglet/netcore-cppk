using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Newtonsoft.Json;
using NLog;
using Rzdppk.Model.Dto;
using TabletLocker.Model;
using TabletLocker.Model.Dto;

namespace TabletLocker.Db
{
    public class DbSyncronizer
    {
        private static DbSyncronizer _instance;

        private SqlConnection _localConnection;

        private readonly HttpClient _remoteClient;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        //for clear & sync race
        private readonly object _locker = new object();

        private DbSyncronizer()
        {
            _remoteClient = GetRemoteConnection();
        }

        public static DbSyncronizer GetInstance()
        {
            return _instance ?? (_instance = new DbSyncronizer());
        }

        public HttpClient GetRemoteConnection()
        {
            var apiKey = ConfigurationManager.AppSettings.Get("ApiKey");
            var syncServiceUrl = ConfigurationManager.AppSettings.Get("DbSyncServiceUrl");
            var client = new HttpClient
            {
                BaseAddress = new Uri(syncServiceUrl)
            };
            client.DefaultRequestHeaders.Add("ApiKey", apiKey);
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        public void SyncDatabases()
        {
            lock (_locker)
            {
                try
                {
                    _logger.Debug("SyncDatabases start");
                    var localDbConnString = ConfigurationManager.ConnectionStrings["LocalDbConnString"];

                    using (_localConnection = new SqlConnection(localDbConnString.ConnectionString))
                    {
                        _localConnection.Open();

                        ProcessUsers();

                        ProcessDevices();

                        ProcessDeviceFaults();

                        var sentO = ProcessOutOperations();

                        var sentT = ProcessOutTasks();

                        if (sentT + sentO > 0)
                        {
                            ProcessDevices();
                        }
                    }

                    _logger.Debug("SyncDatabases finished OK");
                }
                catch (Exception e)
                {
                    _logger.Error(e, "SyncDatabases error");
                    throw new SyncException(e.Message);
                }
            }
        }

        public void ClearDatabase()
        {
            lock (_locker)
            {
                var localDbConnString = ConfigurationManager.ConnectionStrings["LocalDbConnString"];
                using (var scope = new TransactionScope())
                {
                    using (_localConnection = new SqlConnection(localDbConnString.ConnectionString))
                    {
                        _localConnection.Execute(sqlDeleteOperations);
                        _localConnection.Execute(sqlDeleteTasks);
                        _localConnection.Execute(sqlDeleteDeviceFaults);
                        _localConnection.Execute(sqlDeleteDevices);
                        _localConnection.Execute(sqlDeleteUsers);

                        scope.Complete();
                    }
                }
            }
        }

        private void ProcessUsers()
        {
            var response = _remoteClient.GetAsync("/api/TabletIssueStationSync/GetAllUsers").Result;
            var content = response.Content.ReadAsStringAsync().Result;
            _logger.Debug($"ProcessUsers GetAllUsers content: {content}");
            var usersRemote = JsonConvert.DeserializeObject<List<User>>(content).ToDictionary(o => o.Id);
            //_logger.Debug($"ProcessUsers GetAllUsers response: {response.Content}");
            //var usersRemote = response.Content.ReadAsAsync<List<User>>().Result.ToDictionary(o => o.Id);

            //users
            var usersLocal = _localConnection.Query<User>(SqlGetLocalUsers).ToDictionary(o => o.Id);

            var userUpdates = GetUsersLists(usersRemote, usersLocal);

            using (var tran = _localConnection.BeginTransaction("users"))
            {
                if (userUpdates.UpdateList.Any())
                {
                    _logger.Debug($"ProcessUsers UpdateList count: {userUpdates.UpdateList.Count}");
                    foreach (var item in userUpdates.UpdateList)
                    {
                        //update
                        _logger.Debug($"ProcessUsers SQL: {SqlUpdateUser}, Login={item.Login}, Name={item.Name}, PersonNumber={item.PersonNumber}, IsBlocked={item.IsBlocked}, IsAdmin={item.IsAdmin}, UpdateDate={item.UpdateDate}, Id={item.Id}");
                        _localConnection.Execute(SqlUpdateUser,
                            new
                            {
                                item.Login,
                                item.Name,
                                item.PersonNumber,
                                item.PasswordHash,
                                item.IsBlocked,
                                item.IsAdmin,
                                item.UpdateDate,
                                item.Id
                            }, tran);
                    }
                }

                if (userUpdates.CreateList.Any())
                {
                    _logger.Debug($"ProcessUsers CreateList count: {userUpdates.CreateList.Count}");
                    //для вставки с вручную заданным Id
                    _localConnection.Execute("SET IDENTITY_INSERT Users ON", null, tran);

                    foreach (var item in userUpdates.CreateList)
                    {
                        //create
                        _logger.Debug($"ProcessUsers SQL: {SqlCreateUser}, Login={item.Login}, Name={item.Name}, PersonNumber={item.PersonNumber}, IsBlocked={item.IsBlocked}, IsAdmin={item.IsAdmin}, UpdateDate={item.UpdateDate}, Id={item.Id}");
                        _localConnection.Execute(SqlCreateUser,
                            new
                            {
                                item.Id,
                                item.Login,
                                item.Name,
                                item.PersonNumber,
                                item.PasswordHash,
                                item.IsBlocked,
                                item.IsAdmin,
                                item.UpdateDate
                            }, tran);
                    }

                    _localConnection.Execute("SET IDENTITY_INSERT Users OFF", null, tran);
                }

                tran.Commit();
            }
        }

        private static MyUpdates<User> GetUsersLists(Dictionary<int, User> usersRemote, Dictionary<int, User> usersLocal)
        {
            var ret = new MyUpdates<User>();

            foreach (var kv in usersRemote)
            {
                var userRemote = kv.Value;
                if (usersLocal.ContainsKey(kv.Key))
                {
                    //update?
                    var userLocal = usersLocal[kv.Key];
                    
                    if (userRemote.UpdateDate > userLocal.UpdateDate || userRemote.IsAdmin != userLocal.IsAdmin)
                    {
                        //update
                        ret.UpdateList.Add(userRemote);
                    }
                }
                else
                {
                    //add
                    ret.CreateList.Add(userRemote);
                }
            }

            return ret;
        }

        private void ProcessDevices()
        {
            var response = _remoteClient.GetAsync("/api/TabletIssueStationSync/GetAllDevices").Result;
            var content = response.Content.ReadAsStringAsync().Result;
            _logger.Debug($"ProcessDevices GetAllDevices content: {content}");
            var devicesRemote = JsonConvert.DeserializeObject<List<Device>>(content).ToDictionary(o => o.Id);
            //var devicesRemote = response.Content.ReadAsAsync<List<Device>>().Result.ToDictionary(o => o.Id);

            var devicesLocal = _localConnection.Query<Device>(SqlGetLocalDevices).ToDictionary(o => o.Id);
            //var devicesRemote = dbRemote.Query<Device>(SqlGetRemoteDevices).ToDictionary(o => o.Id);

            var deviceUpdates = GetDevicesLists(devicesRemote, devicesLocal);

            using (var tran = _localConnection.BeginTransaction("devices"))
            {
                if (deviceUpdates.UpdateList.Any())
                {
                    _logger.Debug($"ProcessDevices UpdateList count: {deviceUpdates.UpdateList.Count}");
                    foreach (var item in deviceUpdates.UpdateList)
                    {
                        //update
                        _logger.Debug($"ProcessDevices SQL: {SqlUpdateDevice}, Name={item.Name}, Serial={item.Serial}, CellNumber={item.CellNumber}, OpenTasksCount={item.OpenTasksCount}, LastOperation={item.LastOperation}, LastOperationUserId={item.LastOperationUserId}, LastCharge={item.LastCharge}, LastChargeDate={item.LastChargeDate}, UpdateDate={item.UpdateDate}, Id={item.Id}");
                        _localConnection.Execute(SqlUpdateDevice,
                            new
                            {
                                item.Name,
                                item.Serial,
                                item.CellNumber,
                                item.OpenTasksCount,
                                item.LastOperation,
                                item.LastOperationUserId,
                                item.LastOperationDate,
                                item.LastCharge,
                                item.LastChargeDate,
                                item.UpdateDate,
                                item.Id
                            }, tran);
                    }
                }

                if (deviceUpdates.CreateList.Any())
                {
                    _logger.Debug($"ProcessDevices CreateList count: {deviceUpdates.CreateList.Count}");
                    //для вставки с вручную заданным Id
                    _localConnection.Execute("SET IDENTITY_INSERT Devices ON", null, tran);

                    foreach (var item in deviceUpdates.CreateList)
                    {
                        //create
                        _logger.Debug($"ProcessDevices SQL: {SqlCreateDevice}, Name={item.Name}, Serial={item.Serial}, CellNumber={item.CellNumber}, OpenTasksCount={item.OpenTasksCount}, LastOperation={item.LastOperation}, LastOperationUserId={item.LastOperationUserId}, LastCharge={item.LastCharge}, LastChargeDate={item.LastChargeDate}, UpdateDate={item.UpdateDate}, Id={item.Id}");
                        _localConnection.Execute(SqlCreateDevice,
                            new
                            {
                                item.Id,
                                item.Name,
                                item.Serial,
                                item.CellNumber,
                                item.OpenTasksCount,
                                item.LastOperation,
                                item.LastOperationUserId,
                                item.LastOperationDate,
                                item.LastCharge,
                                item.LastChargeDate,
                                item.UpdateDate,
                            }, tran);
                    }

                    _localConnection.Execute("SET IDENTITY_INSERT Devices OFF", null, tran);
                }

                tran.Commit();
            }
        }

        private static MyUpdates<Device> GetDevicesLists(Dictionary<int, Device> devicesRemote, Dictionary<int, Device> devicesLocal)
        {
            var ret = new MyUpdates<Device>();

            foreach (var kv in devicesRemote)
            {
                var deviceRemote = kv.Value;
                if (devicesLocal.ContainsKey(kv.Key))
                {
                    //update?
                    var deviceLocal = devicesLocal[kv.Key];

                    var changed = deviceRemote.UpdateDate > deviceLocal.UpdateDate || deviceRemote.OpenTasksCount != deviceLocal.OpenTasksCount;

                    if (deviceRemote.LastChargeDate.HasValue && deviceLocal.LastChargeDate.HasValue)
                    {
                        changed = changed || deviceRemote.LastChargeDate.Value > deviceLocal.LastChargeDate.Value;
                    }
                    else
                    {
                        changed = changed || deviceRemote.LastChargeDate != deviceLocal.LastChargeDate;
                    }

                    if (deviceRemote.LastOperationDate.HasValue && deviceLocal.LastOperationDate.HasValue)
                    {
                        changed = changed || deviceRemote.LastOperationDate.Value > deviceLocal.LastOperationDate.Value;
                    }
                    else
                    {
                        changed = changed || deviceRemote.LastOperationDate != deviceLocal.LastOperationDate;
                    }

                    if (changed)
                    {
                        //update
                        ret.UpdateList.Add(deviceRemote);
                    }
                }
                else
                {
                    //add
                    ret.CreateList.Add(deviceRemote);
                }
            }

            return ret;
        }

        private void ProcessDeviceFaults()
        {
            var response = _remoteClient.GetAsync("/api/TabletIssueStationSync/GetAllDeviceFaults").Result;
            var content = response.Content.ReadAsStringAsync().Result;
            _logger.Debug($"ProcessDeviceFaults GetAllDeviceFaults content: {content}");
            var deviceFaultsRemote = JsonConvert.DeserializeObject<List<DeviceFault>>(content).ToDictionary(o => o.Id);
            //_logger.Debug($"ProcessDeviceFaults GetAllDeviceFaults response: {response.Content}");
            //var deviceFaultsRemote = response.Content.ReadAsAsync<List<DeviceFault>>().Result.ToDictionary(o => o.Id);

            var deviceFaultsLocal = _localConnection.Query<DeviceFault>(SqlGetLocalDeviceFaults).ToDictionary(o => o.Id);

            var deviceFaultUpdates = GetDeviceFaultsList(deviceFaultsRemote, deviceFaultsLocal);

            using (var tran = _localConnection.BeginTransaction("devicefaults"))
            {
                if (deviceFaultUpdates.UpdateList.Any())
                {
                    _logger.Debug($"ProcessDeviceFaults UpdateList count: {deviceFaultUpdates.UpdateList.Count}");

                    foreach (var item in deviceFaultUpdates.UpdateList)
                    {
                        //update
                        _logger.Debug($"ProcessDeviceFaults SQL: {SqlUpdateDeviceFault}, Name={item.Name}, Description={item.Description}, UpdateDate={item.UpdateDate}, Id={item.Id}");
                        _localConnection.Execute(SqlUpdateDeviceFault,
                            new
                            {
                                item.Name,
                                item.Description,
                                item.UpdateDate,
                                item.Id
                            }, tran);
                    }
                }

                if (deviceFaultUpdates.CreateList.Any())
                {
                    _logger.Debug($"ProcessDeviceFaults CreateList count: {deviceFaultUpdates.CreateList.Count}");
                    //для вставки с вручную заданным Id
                    _localConnection.Execute("SET IDENTITY_INSERT DeviceFaults ON", null, tran);

                    foreach (var item in deviceFaultUpdates.CreateList)
                    {
                        //create
                        _logger.Debug($"ProcessDeviceFaults SQL: {SqlCreateDeviceFault}, Name={item.Name}, Description={item.Description}, UpdateDate={item.UpdateDate}, Id={item.Id}");
                        _localConnection.Execute(SqlCreateDeviceFault,
                            new
                            {
                                item.Id,
                                item.Name,
                                item.Description,
                                item.UpdateDate,
                            }, tran);
                    }

                    _localConnection.Execute("SET IDENTITY_INSERT DeviceFaults OFF", null, tran);
                }

                tran.Commit();
            }
        }

        private static MyUpdates<DeviceFault> GetDeviceFaultsList(Dictionary<int, DeviceFault> devicesRemote, Dictionary<int, DeviceFault> devicesLocal)
        {
            var ret = new MyUpdates<DeviceFault>();

            foreach (var kv in devicesRemote)
            {
                var deviceRemote = kv.Value;
                if (devicesLocal.ContainsKey(kv.Key))
                {
                    //update?
                    var deviceLocal = devicesLocal[kv.Key];

                    if (deviceRemote.UpdateDate > deviceLocal.UpdateDate)
                    {
                        //update
                        ret.UpdateList.Add(deviceRemote);
                    }
                }
                else
                {
                    //add
                    ret.CreateList.Add(deviceRemote);
                }
            }

            return ret;
        }

        private int ProcessOutOperations()
        {
            var sentCount = 0;

            var outOperations = _localConnection.Query<OperationOut>(SqlGetUnsentOperations).OrderBy(o => o.CreateDate).ToList();

            if (outOperations.Any())
            {
                _logger.Debug($"ProcessOutOperations total operations to send: {outOperations.Count}");

                foreach (var item in outOperations)
                {
                    try
                    {
                        //отправляем операцию в удаленную базу
                        var operDto = new OperationOutDto
                        {
                            DeviceId = item.DeviceId,
                            Operation = item.Operation,
                            UserId = item.UserId,
                            CreateDate = item.CreateDate,
                            RefId = item.RefId
                        };

                        _logger.Debug($"ProcessOutOperations operDto: DeviceId={operDto.DeviceId}, Operation={operDto.Operation}, UserId={operDto.UserId}, CreateDate={operDto.CreateDate}, RefId={operDto.RefId}");

                        var response = _remoteClient
                            .PostAsJsonAsync("/api/TabletIssueStationSync/CreateOperation", operDto).Result;

                        var error = false;
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsAsync<InsertResDto>().Result;
                            if (result.IsSuccess == false && result.AlreadyExist == false)
                            {
                                //какая-то ошибка и это не повтор refid
                                _logger.Error($"ProcessOutOperations request error: {result.Error}");
                                error = true;
                            }
                        }
                        else
                        {
                            _logger.Error($"ProcessOutOperations non-success request status code: {response.StatusCode}");
                            error = true;
                        }

                        if (error)
                        {
                            throw new Exception();
                        }

                        sentCount++;
                        _localConnection.Execute(SqlMarkSentOperation, new {item.Id});
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "ProcessOutOperations error");
                    }
                }
            }

            return sentCount;
        }

        private int ProcessOutTasks()
        {
            var sentCount = 0;

            var outTasks = _localConnection.Query<TaskOut>(SqlGetUnsentTasks).OrderBy(o => o.CreateDate).ToList();

            if (outTasks.Any())
            {
                _logger.Debug($"ProcessOutTasks total tasks to send: {outTasks.Count}");

                foreach (var item in outTasks)
                {
                    //отправляем задачу в удаленную базу
                    try
                    {
                        var taskDto = new TaskOutDto
                        {
                            DeviceId = item.DeviceId,
                            UserId = item.UserId,
                            DeviceFaultId = item.DeviceFaultId,
                            CreateDate = item.CreateDate,
                            RefId = item.RefId
                        };

                        _logger.Debug($"ProcessOutTasks taskDto: DeviceId={taskDto.DeviceId}, DeviceFaultId={taskDto.DeviceFaultId}, UserId={taskDto.UserId}, CreateDate={taskDto.CreateDate}, RefId={taskDto.RefId}");

                        var response = _remoteClient.PostAsJsonAsync("/api/TabletIssueStationSync/CreateTask", taskDto)
                            .Result;

                        var error = false;
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsAsync<InsertResDto>().Result;
                            if (result.IsSuccess == false && result.AlreadyExist == false)
                            {
                                //какая-то ошибка и это не повтор refid
                                _logger.Error($"ProcessOutTasks request error: {result.Error}");
                                error = true;
                            }
                        }
                        else
                        {
                            _logger.Error($"ProcessOutTasks non-success request status code: {response.StatusCode}");
                            error = true;
                        }

                        if (error)
                        {
                            throw new Exception();
                        }

                        //если успешно - помечаем задачу как отправленную
                        sentCount++;
                        _localConnection.Execute(SqlMarkSentTask, new {item.Id});
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "ProcessOutTasks error");
                    }
                }
            }

            return sentCount;
        }

        //users
        private const string SqlGetLocalUsers = "SELECT * FROM [Users]";

        private const string SqlUpdateUser = "UPDATE [Users] SET [Login]=@Login, [Name]=@Name, [PersonNumber]=@PersonNumber, [PasswordHash]=@PasswordHash, [IsBlocked]=@IsBlocked, [IsAdmin]=@IsAdmin, [UpdateDate]=@UpdateDate WHERE Id=@Id";

        private const string SqlCreateUser = "INSERT INTO [Users] ([Id],[Login],[Name],[PersonNumber],[PasswordHash],[IsBlocked],[IsAdmin],[UpdateDate]) VALUES(@Id,@Login,@Name,@PersonNumber,@PasswordHash,@IsBlocked,@IsAdmin,@UpdateDate)";

        private const string sqlDeleteUsers = @"DELETE FROM Users";

        //devices
        private const string SqlGetLocalDevices = "SELECT * FROM [Devices]";

        private const string SqlUpdateDevice = "UPDATE [Devices] SET [Name]=@Name, [Serial]=@Serial, [CellNumber]=@CellNumber, [OpenTasksCount]=@OpenTasksCount," + "[LastOperation]=@LastOperation,[LastOperationUserId]=@LastOperationUserId,[LastOperationDate]=@LastOperationDate," + "[LastCharge]=@LastCharge,[LastChargeDate]=@LastChargeDate,UpdateDate=@UpdateDate WHERE Id=@Id";

        private const string SqlCreateDevice = "INSERT INTO [Devices] ([Id],[Name],[Serial],[CellNumber],[OpenTasksCount],[LastOperation],[LastOperationUserId],[LastOperationDate]," + "[LastCharge],[LastChargeDate],[UpdateDate]) VALUES(@Id,@Name,@Serial,@CellNumber,@OpenTasksCount,@LastOperation,@LastOperationUserId,@LastOperationDate," + "@LastCharge,@LastChargeDate,@UpdateDate)";

        private const string sqlDeleteDevices = @"DELETE FROM Devices";

        //devicefaults
        private const string SqlGetLocalDeviceFaults = "SELECT * FROM [DeviceFaults]";

        private const string SqlUpdateDeviceFault = "UPDATE [DeviceFaults] SET [Name]=@Name, [Description]=@Description, [UpdateDate]=@UpdateDate WHERE Id=@Id";

        private const string SqlCreateDeviceFault = "INSERT INTO [DeviceFaults] ([Id],[Name],[Description],[UpdateDate]) VALUES(@Id,@Name,@Description,@UpdateDate)";

        private const string sqlDeleteDeviceFaults = @"DELETE FROM DeviceFaults";

        //out operations
        private const string SqlGetUnsentOperations = "SELECT * FROM [OperationOut] WHERE IsSent=0";

        private const string SqlMarkSentOperation = "UPDATE [OperationOut] SET IsSent=1 WHERE Id=@Id";

        private const string sqlDeleteOperations = @"DELETE FROM OperationOut";

        //out tasks
        private const string SqlGetUnsentTasks = "SELECT * FROM [TaskOut] WHERE IsSent=0";

        private const string SqlMarkSentTask = "UPDATE [TaskOut] SET IsSent=1 WHERE Id=@Id";

        private const string sqlDeleteTasks = @"DELETE FROM TaskOut";
    }

    public class MyUpdates<T>
    {
        public MyUpdates()
        {
            CreateList = new List<T>();

            UpdateList = new List<T>();
        }

        public List<T> UpdateList { get; set; }

        public List<T> CreateList { get; set; }
    }

    public class SyncException : Exception
    {
        public SyncException(string message) : base(message)
        {
        }
    }
}
