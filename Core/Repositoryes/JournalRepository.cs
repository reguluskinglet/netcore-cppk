//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Dapper;
//using Microsoft.AspNetCore.Mvc;

using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Sqls;
using static Rzdppk.Core.Services.JournalService;


namespace Rzdppk.Core.Repositoryes
{
    public class JournalRepository 
    {
        private static IDb _db;
        private readonly ILogger _logger;

        public JournalRepository(ILogger logger)
        {
            _db = new Db();
            _logger = logger;
        }


        public async Task<JournalTaskFromSql[]> GetAllTaskForJournal()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Journal.AllTask"];
                var result =  await conn.QueryAsync<JournalTaskFromSql>(sql);
                return result.ToArray();
            }
        }

    }
}


//        public async Task<JournalPaging> GetAll(int skip, int limit, string filter)
//        {
//            try
//            {
//                //Залупу себе в пул добавь и поверни
//                var listFull = new List<JournalDateItem>();
//                //var response = new List<string>();
//                //var fromSqlsI = await _db.Connection.QueryAsync<JournalInspectionFromSql>(sql);
//                var sqlr = new InspectionRepository(_db, _logger);
//                var inspdatarep = new InspectionDataRepository(_db);
//                var sqlTrainR = new TrainRepository(_db,  _logger);
//                var sqlUserR = new UserRepository(_db);
//                var iR = new InspectionRepository(_db, _logger);
//                var mR = new MeterageRepository(_db);
//                var sqlStatusR = new TaskStatusRepository(_db, _logger);
//                var sqlExecutorR = new ExecutorRepository(_db);
//                var sqlRTask = new TaskRepository(_db, _logger);
//                var sqlRBrigade = new BrigadeRepository(_db, _logger);

//                var fromSqlsInspections = await sqlr.GetAll();
//                foreach (var fromSqlsInspection in fromSqlsInspections)
//                {
//                    var train = (await sqlTrainR.ByIdWithStations(fromSqlsInspection.TrainId));
//                    var user = await sqlUserR.ById(fromSqlsInspection.UserId);
//                    var journalInspection = new JournalInspection
//                    {
//                        Id = fromSqlsInspection.Id,
//                        Type = "inspection",
//                        InspectionId = fromSqlsInspection.Id,
//                        StatusId = (int)fromSqlsInspection.Status,
//                        TypeId = (int)fromSqlsInspection.CheckListType,
//                        Date = fromSqlsInspection.DateStart,
//                        TrainName = train.Name,
//                        TrainId = train.Id,
//                        Tabs = new JournalInspectionTabs(),
//                        UserName = user.Name,
//                    };

//                    if (user.BrigadeId != null)
//                        journalInspection.OwnerBrigadeType = (await sqlRBrigade.ByIdWithStations((int) user.BrigadeId)).BrigadeType;


//                    //дескрипшен

//                    var counters = await iR.GetCounters(fromSqlsInspection.Id);

//                    journalInspection.Tabs.Description = new JournalInspectionDescription
//                    {
//                        TasksCount = counters.Tasks,
//                        DataEnd = fromSqlsInspection.DateEnd,
//                        //Выдает количество уникальных меток
//                        AllLabelCount = counters.Labels, //counters.LabelsAll,
//                        //TaskLabelCount = counters.Labels,
//                        TempCount = counters.Measurements
//                    };


//                    //лейблы


//                    var labels = new List<JournalInspectionLabel>();
//                    var labelsFromSql = await mR.GetLabels(fromSqlsInspection.Id);
//                    foreach (var labelFromSql in labelsFromSql) { 
//                        labels.Add(new JournalInspectionLabel
//                        {
//                            CarriageName = CreateCarriageName(labelFromSql.Label.Carriage.Train.Name,labelFromSql.Label.Carriage.Number),
//                            EquipmentName = labelFromSql.Label.EquipmentModel.Equipment.Name,
//                            LabelSerial = labelFromSql.Label.Rfid,
//                            TimeStamp = labelFromSql.Date
//                        });
//                    }
//                    journalInspection.Tabs.Labels = labels.ToArray();

//                    //измерения
//                    var temps = new List<JournalInspectionTemp>();
//                    var tempsFromSql = await mR.GetMeterages(fromSqlsInspection.Id);
//                    foreach (var tempFromSql in tempsFromSql)
//                    {
//                        temps.Add(new JournalInspectionTemp
//                        {
//                            Value = tempFromSql.Value,
//                            TimeStamp = tempFromSql.Date
//                        });
//                    }

//                    journalInspection.Tabs.Temps = temps.ToArray();

//                    //счетчики
//                    var countersFromSql = await inspdatarep.GetByInspectionId(fromSqlsInspection.Id);
//                    if (countersFromSql.Length > 0)
//                    {
//                        journalInspection.Tabs.Description.KmPerShift = String.Join(", ",
//                            countersFromSql.Where(o => o.Type == InspectionDataType.KmPerShift)
//                                .Select(o => o.Value));
//                        journalInspection.Tabs.Description.KmTotal = String.Join(", ",
//                            countersFromSql.Where(o => o.Type == InspectionDataType.KmTotal)
//                                .Select(o => o.Value));
//                        journalInspection.Tabs.Description.KwHours = String.Join(", ",
//                            countersFromSql.Where(o => o.Type == InspectionDataType.KwHours)
//                                .Select(o => _GetCarriageNumCanBeNull(o.Carriage) + ":" + o.Value));
//                    }

//                    //говнохак под ебанутые фильтры и вывод. TypeId == 2 - Приемка, 3 - сдача
//                    //30 - Приёмка поезда ПР
//                    if (journalInspection.OwnerBrigadeType != null)
//                    {
//                        if (journalInspection.OwnerBrigadeType == BrigadeType.Receiver && journalInspection.TypeId == 2)
//                            journalInspection.TypeId = 30;
//                        //31 - Выпуск поезда ПР
//                        if (journalInspection.OwnerBrigadeType == BrigadeType.Receiver && journalInspection.TypeId == 3)
//                            journalInspection.TypeId = 31;
//                        //32 - Приёмка поезда ЛБ
//                        if (journalInspection.OwnerBrigadeType == BrigadeType.Locomotiv && journalInspection.TypeId == 2)
//                            journalInspection.TypeId = 32;
//                        //33 - Сдача поезда ЛБ
//                        if (journalInspection.OwnerBrigadeType == BrigadeType.Locomotiv && journalInspection.TypeId == 3)
//                            journalInspection.TypeId = 33;
//                    }


//                    //тут должен быть фильтр
//                    var isFiltered = false;
//                    if (filter != null)
//                    {

//                        var filters = JsonConvert.DeserializeObject<TaskCommon.FilterBody[]>(filter);
//                        foreach (var item in filters)
//                        {
//                            switch (item.Filter)
//                            {
//                                case "DateFrom":
//                                    DateTime.TryParse(item.Value, out var dateStart);
//                                    if (fromSqlsInspection.DateStart < dateStart)
//                                        isFiltered = true;
//                                    break;

//                                case "DateTo":
//                                    DateTime.TryParse(item.Value, out var dateEnd);
//                                    dateEnd = dateEnd.AddDays(1);
//                                    if (fromSqlsInspection.DateStart > dateEnd)
//                                        isFiltered = true;
//                                    break;
//                                case "InspectionId":
//                                    int.TryParse(item.Value, out var filterId);
//                                    if (fromSqlsInspection.Id != filterId)
//                                        isFiltered = true;
//                                    break;
//                                case "TrainName":
//                                    if (!journalInspection.TrainName.ToLower().Contains(item.Value.ToLower()))
//                                        isFiltered = true;
//                                    break;
//                                case "TrainId":
//                                    int.TryParse(item.Value, out var trainid);
//                                    if (journalInspection.TrainId != trainid)
//                                        isFiltered = true;
//                                    break;
//                                //Нет вагонов у инспекций
//                                case "CarriageId":
//                                        isFiltered = true;
//                                    break;
//                                //Инициатор
//                                case "OwnerId":
//                                    if (!journalInspection.UserName.ToLower().Contains(item.Value.ToLower()))
//                                        isFiltered = true;
//                                    break;
//                                //TODO узнать про статусы инспекций
//                                //Статус инспекции, ахуеть
//                                case "StatusId":
//                                    int.TryParse(item.Value, out var intResult);
//                                    if (journalInspection.StatusId != intResult)
//                                        isFiltered = true;
//                                    break;

//                                //нет у инспекций исполнителя
//                                case "BrigadeId":
//                                        isFiltered = true;
//                                    break;
//                                //Мероприятие блядь или тип инспекции в UI
//                                case "TypeId":
//                                    int.TryParse(item.Value, out var intResultT);
//                                    if (journalInspection.TypeId != intResultT)
//                                        isFiltered = true;
//                                    break;
//                                case "TaskOrInspectionId":
//                                    if (!journalInspection.Id.ToString().Equals(item.Value.ToLower()))
//                                        isFiltered = true;
//                                    break;

//                                case "Equipment":
//                                    var internalFilters = new List<FilterBody>
//                                    {
//                                        new FilterBody { Filter = "Equipment", Value = item.Value},
//                                        new FilterBody { Filter = "InspectionId", Value = journalInspection.Id.ToString()},
//                                    };
//                                    var tasksFiltered = await sqlRTask.GetAll(0, 9999999, internalFilters.ToArray().ToJson());
//                                    bool foreachFiltered = false;
//                                    foreach (var task in tasksFiltered.Data)
//                                    {
//                                        if (task.EquipmentName.ToLower().Contains(item.Value.ToLower()))
//                                            foreachFiltered = true;
//                                    }
//                                    if (!foreachFiltered)
//                                        isFiltered = true;
//                                    break;
//                            }

//                        }
//                    }
//                    if (!isFiltered)
//                        //response.Add(journalInspection.ToJson());
//                        listFull.Add(new JournalDateItem
//                            {
//                                Date = journalInspection.Date,
//                                Inspection = journalInspection
//                            }
//                        );
//                }

//                var sql = Sql.SqlQueryCach["Journal.AllTask"];
//                var fromSqlsT = await _db.Connection.QueryAsync<JournalTaskFromSql>(sql);
//                //var other = new Other.Other();
//                foreach (var fromSql in fromSqlsT)
//                {
//                    var journalTask = new JournalTask
//                    {

//                        Type = "Task",
//                        TaskId = fromSql.TaskId,
//                        TaskType = fromSql.TaskType,
//                        TrainName = fromSql.TrainName,
//                        TrainId = fromSql.TrainId,
//                        UserName = fromSql.UserName,
//                        Date = fromSql.CreateDate,
//                        CarriageName = CreateCarriageName(fromSql.TrainName,fromSql.CarriageNumber),
//                        CarriageNum = fromSql.CarriageNumber,
//                        CarriageId = fromSql.CarriageId,
//                        EquipmentName = fromSql.EquipmentName,
//                        FaultName = fromSql.FaultName,
//                        InspectionInfo = _GetInspectionInfo(fromSql.InspectionId, fromSql.InspectionType),
//                        CarriagesSerialNumber = fromSql.CarriagesSerialNumber
//                    };

//                    var executor = sqlExecutorR.ByTaskId(fromSql.TaskId).LastOrDefault();
//                    if (executor != null)
//                        journalTask.ExecutorBrigadeType = (int) executor.BrigadeType;


//                    var taskStatus = sqlStatusR.GetByTrainTaskId(fromSql.TaskId);
//                    if (taskStatus != null)
//                        journalTask.StatusId = (int) sqlStatusR.GetByTrainTaskId(fromSql.TaskId).Status;
//                    //journalTask.StatusId = fromSql.TaskId;

//                    var isFiltered = false;
//                    if (filter != null)
//                    {
//                        var filters = JsonConvert.DeserializeObject<TaskCommon.FilterBody[]>(filter);
//                        foreach (var item in filters)
//                        {
//                            switch (item.Filter)
//                            {
//                                case "DateFrom":
//                                    DateTime.TryParse(item.Value, out var dateStart);
//                                    if (fromSql.CreateDate < dateStart)
//                                        isFiltered = true;
//                                    break;
//                                case "DateTo":
//                                    DateTime.TryParse(item.Value, out var dateEnd);
//                                    dateEnd = dateEnd.AddDays(1);
//                                    if (fromSql.CreateDate > dateEnd)
//                                        isFiltered = true;
//                                    break;
//                                case "InspectionId":
//                                    isFiltered = true;
//                                    break;
//                                case "TrainName":
//                                    if (!fromSql.TrainName.ToLower().Contains(item.Value.ToLower())) 
//                                        isFiltered = true;
//                                    break;
//                                case "TrainId":
//                                    int.TryParse(item.Value, out var trainid);
//                                    if (fromSql.TrainId != trainid)
//                                        isFiltered = true;
//                                    break;
//                                //по номеру вагона
//                                case "CarriageNum":
//                                    if (int.TryParse(item.Value, out var intResult))
//                                        if (journalTask.CarriageNum != intResult)
//                                            isFiltered = true;
//                                    break;
//                                case "CarriageId":
//                                    if (int.TryParse(item.Value, out var carriageid))
//                                        if (journalTask.CarriageId != carriageid)
//                                            isFiltered = true;
//                                    break;
//                                //Инициатор
//                                case "OwnerId":
//                                    if (!journalTask.UserName.ToLower().Contains(item.Value.ToLower()))
//                                        isFiltered = true;
//                                    break;
//                                //Статус
//                                case "StatusId":
//                                    int.TryParse(item.Value, out var intResultS);
//                                    if (journalTask.StatusId != intResultS)
//                                        isFiltered = true;
//                                    break;
//                                case "BrigadeId":
//                                    int.TryParse(item.Value, out var intResultB);
//                                    if (journalTask.ExecutorBrigadeType != intResultB)
//                                        isFiltered = true;
//                                    break;
//                                //Нет тут мероприятей блядь Есть тип. Но хуй его знает У задач он одит
//                                case "TypeId":
//                                    int.TryParse(item.Value, out var intResultT);
//                                    //Мы будем гореть в аду. Ебаные аналитики хуевы
//                                    var realTaskType = 999;
//                                    if (intResultT == 40)
//                                        realTaskType = 0;
//                                    if (intResultT == 41)
//                                        realTaskType = 1;
//                                    if (intResultT == 42)
//                                        realTaskType = 2;
//                                    if (journalTask.TaskType != realTaskType)
//                                        isFiltered = true;
//                                    break;

//                                case "TaskOrInspectionId":
//                                    if (!journalTask.TaskId.ToString().Equals(item.Value))
//                                        isFiltered = true;
//                                    break;

//                                case "Equipment":
//                                    if (!journalTask.EquipmentName.ToLower().Contains(item.Value.ToLower()))    
//                                        isFiltered = true;
//                                    break;
//                            }
//                        }
//                    }
//                    if (!isFiltered)
//                        //response.Add(journalTask.ToJson());
//                        listFull.Add(new JournalDateItem
//                            {
//                                Date = journalTask.Date,
//                                Task = journalTask
//                        }
//                        );
//                }

//                //sort by date
//                listFull.Sort((x, y) => y.Date.CompareTo(x.Date));

//                var listSkiplimit = new List<JournalDateItem>();
//                if (skip < listFull.Count)
//                {
//                    if (limit + skip > listFull.Count)
//                        limit = listFull.Count - skip;

//                    for (int i = skip; i < limit + skip; i++)
//                    {
//                        listSkiplimit.Add(listFull[i]);
//                    }
//                }

//                var response = new List<string>();
//                foreach (var item in listSkiplimit)
//                {
//                    if (item.Task != null)
//                    {
//                        response.Add(item.Task.ToJson());
//                    }
//                    if (item.Inspection != null)
//                    {
//                        response.Add(item.Inspection.ToJson());
//                    }
//                }

//                var result = new JournalPaging();

//                result.Data = response.ToArray();
//                result.Total = listFull.Count;

//                //mR.Dispose();
//                //iR.Dispose();
//                //sqlr.Dispose();
//                //inspdatarep.Dispose();
//                //sqlTrainR.Dispose();
//                //sqlUserR.Dispose();
//                //sqlStatusR.Dispose();
//                //sqlExecutorR.Dispose();


//                return result;
//            }
//            catch (Exception e)
//            {
//                //var error = new Other.Other.ErrorResponse
//                //{
//                //    Message = "Произошла ошибка",
//                //    Exception = e
//                //};
//                //return new List<string>() {JsonConvert.SerializeObject(error)}.ToArray();
//                var result = new JournalPaging();
//                result.Error = true;
//                result.Exception = e;
//                return result;
//            }
//        }





//        //public void Dispose()
//        //{
//        //    _db.Connection.Close();
//        //}

//        //private string _GetCarriageNumCanBeNull(Carriage car)
//        //{
//        //    if (car == null)
//        //        return "NULL";

//        //    return car.Serial;
//        //}
//    }
//}
