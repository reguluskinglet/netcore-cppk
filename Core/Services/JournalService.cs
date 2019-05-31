using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Core.Extensions;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using Rzdppk.Model;
using Rzdppk.Model.Enums;
using static Rzdppk.Core.Other.Other;


namespace Rzdppk.Core.Services
{
    public class JournalService
    {

        private readonly ILogger _logger;

        public JournalService(ILogger logger)
        {
            _logger = logger;
        }


        //30 - Приёмка поезда ПР
        //31 - Выпуск поезда ПР
        //32 - Приёмка поезда ЛБ
        //33 - Сдача поезда ЛБ

        public async Task<JournalPaging> GetJournalInspectionAndTasks(int skip, int limit, string filter)
        {
            var sqlr = new InspectionRepository(_logger);

            var inspdatarep = new InspectionDataRepository(_logger);
            var sqlTrainR = new TrainRepository(_logger);
            var sqlUserR = new UserRepository(_logger);
            var iR = new InspectionRepository(_logger);
            var mR = new MeterageRepository(_logger);
            var sqlStatusR = new TaskStatusRepository(_logger);
            var sqlExecutorR = new ExecutorRepository(_logger);
            var sqlRTask = new TaskRepository(_logger);
            var sqlRBrigade = new BrigadeRepository(_logger);
            var sqlRJournal = new JournalRepository(_logger);

            var listFull = new List<JournalDateItem>();

            var fromSqlsInspections = await sqlr.GetAll();

            foreach (var fromSqlsInspection in fromSqlsInspections)
            {
                var train = (await sqlTrainR.ByIdWithStations(fromSqlsInspection.TrainId));
                var user = await sqlUserR.ById(fromSqlsInspection.UserId);
                var journalInspection = new JournalInspection
                {
                    Id = fromSqlsInspection.Id,
                    Type = "inspection",
                    InspectionId = fromSqlsInspection.Id,
                    StatusId = (int)fromSqlsInspection.Status,
                    TypeId = (int)fromSqlsInspection.CheckListType,
                    Date = fromSqlsInspection.DateStart,
                    TrainName = train.Name,
                    TrainId = train.Id,
                    Tabs = new JournalInspectionTabs(),
                    UserName = user.Name,
                };

                if (user.BrigadeId != null)
                    journalInspection.OwnerBrigadeType = (await sqlRBrigade.ById((int)user.BrigadeId)).BrigadeType;


                //дескрипшен
                var counters = await iR.GetCounters(fromSqlsInspection.Id);

                journalInspection.Tabs.Description = new JournalInspectionDescription
                {
                    TasksCount = counters.Tasks,
                    DataEnd = fromSqlsInspection.DateEnd,
                    //Выдает количество уникальных меток
                    AllLabelCount = counters.Labels,
                    TempCount = counters.Measurements
                };


                //лейблы
                await getInspectionLabels(mR, fromSqlsInspection, journalInspection);

                //измерения
                await getInspectionMeterage(mR, fromSqlsInspection, journalInspection);

                //счетчики
                await GetInspectionCouters(inspdatarep, fromSqlsInspection, journalInspection);

                //говнохак под ебанутые фильтры и вывод. TypeId == 2 - Приемка, 3 - сдача
                changeInspetionTypeForUi(journalInspection);


                //Фильтруем
                var isFiltered = await InspectionFilter(filter, sqlRTask, fromSqlsInspection, journalInspection);

                if (!isFiltered)
                    listFull.Add(new JournalDateItem
                    {
                        Date = journalInspection.Date,
                        Inspection = journalInspection
                    }
                    );
            }


            //Таски

            var fromSqlsT = await sqlRJournal.GetAllTaskForJournal();
            foreach (var fromSql in fromSqlsT)
            {
                var journalTask = new JournalTask
                {

                    Type = "Task",
                    TaskId = fromSql.TaskId,
                    TaskType = fromSql.TaskType,
                    TrainName = fromSql.TrainName,
                    TrainId = fromSql.TrainId,
                    UserName = fromSql.UserName,
                    Date = fromSql.CreateDate,
                    CarriageName = CreateCarriageName(fromSql.TrainName, fromSql.CarriageNumber),
                    CarriageNum = fromSql.CarriageNumber,
                    CarriageId = fromSql.CarriageId,
                    EquipmentName = fromSql.EquipmentName,
                    FaultName = fromSql.FaultName,
                    InspectionInfo = _GetInspectionInfo(fromSql.InspectionId, fromSql.InspectionType),
                    CarriagesSerialNumber = fromSql.CarriagesSerialNumber,
                    TaskAttributeId = fromSql.TaskAttributeId
                };

                var executor = (await sqlExecutorR.GetByTaskId(fromSql.TaskId)).LastOrDefault();
                if (executor != null)
                    journalTask.ExecutorBrigadeType = (int)executor.BrigadeType;


                var taskStatus = sqlStatusR.GetByTrainTaskId(fromSql.TaskId);
                if (taskStatus != null)
                    journalTask.StatusId = (int)sqlStatusR.GetByTrainTaskId(fromSql.TaskId).Status;
                //journalTask.StatusId = fromSql.TaskId;

                var isFiltered = TaskFilter(filter, fromSql, journalTask);
                if (!isFiltered)
                    //response.Add(journalTask.ToJson());
                    listFull.Add(new JournalDateItem
                    {
                        Date = journalTask.Date,
                        Task = journalTask
                    }
                    );
            }




            //sort by date
            listFull.Sort((x, y) => y.Date.CompareTo(x.Date));

            //пагинация
            List<JournalDateItem> listSkiplimit = paging(skip, ref limit, listFull);

            //Делаем жсон стрингу для вывода
            var pagingList = new List<string>();
            converToJsonString(listSkiplimit, pagingList);

            var result = new JournalPaging
            {
                Data = pagingList.ToArray(),
                Total = listFull.Count
            };

            return result;

        }

        private static bool TaskFilter(string filter, JournalTaskFromSql fromSql, JournalTask journalTask)
        {
            var isFiltered = false;
            if (filter != null)
            {
                var filters = JsonConvert.DeserializeObject<TaskCommon.FilterBody[]>(filter);
                foreach (var item in filters)
                {
                    switch (item.Filter)
                    {
                        case "DateFrom":
                            DateTime.TryParse(item.Value, out var dateStart);
                            if (fromSql.CreateDate < dateStart)
                                isFiltered = true;
                            break;
                        case "DateTo":
                            DateTime.TryParse(item.Value, out var dateEnd);
                            dateEnd = dateEnd.AddDays(1);
                            if (fromSql.CreateDate > dateEnd)
                                isFiltered = true;
                            break;
                        case "InspectionId":
                            isFiltered = true;
                            break;
                        case "TrainName":
                            if (!fromSql.TrainName.ToLower().Contains(item.Value.ToLower()))
                                isFiltered = true;
                            break;
                        case "TrainId":
                            int.TryParse(item.Value, out var trainid);
                            if (fromSql.TrainId != trainid)
                                isFiltered = true;
                            break;
                        //по номеру вагона
                        case "CarriageNum":
                            if (int.TryParse(item.Value, out var intResult))
                                if (journalTask.CarriageNum != intResult)
                                    isFiltered = true;
                            break;
                        case "CarriageId":
                            if (int.TryParse(item.Value, out var carriageid))
                                if (journalTask.CarriageId != carriageid)
                                    isFiltered = true;
                            break;
                        //Инициатор
                        case "OwnerId":
                            if (!journalTask.UserName.ToLower().Contains(item.Value.ToLower()))
                                isFiltered = true;
                            break;
                        //Статус
                        case "StatusId":
                            int.TryParse(item.Value, out var intResultS);
                            if (journalTask.StatusId != intResultS)
                                isFiltered = true;
                            break;
                        case "BrigadeId":
                            int.TryParse(item.Value, out var intResultB);
                            if (journalTask.ExecutorBrigadeType != intResultB)
                                isFiltered = true;
                            break;
                        //Нет тут мероприятей блядь Есть тип. Но хуй его знает У задач он одит
                        case "TypeId":
                            int.TryParse(item.Value, out var intResultT);
                            //Мы будем гореть в аду. Ебаные аналитики хуевы
                            var realTaskType = 999;
                            if (intResultT == 40)
                                realTaskType = 0;
                            if (intResultT == 41)
                                realTaskType = 1;
                            if (intResultT == 42)
                                realTaskType = 2;
                            if (journalTask.TaskType != realTaskType)
                                isFiltered = true;
                            break;

                        case "TaskOrInspectionId":
                            if (!journalTask.TaskId.ToString().Equals(item.Value))
                                isFiltered = true;
                            break;

                        case "Equipment":
                            if (!journalTask.EquipmentName.ToLower().Contains(item.Value.ToLower()))
                                isFiltered = true;
                            break;
                    }
                }
            }

            return isFiltered;
        }

        private static void converToJsonString(List<JournalDateItem> listSkiplimit, List<string> pagingList)
        {
            foreach (var item in listSkiplimit)
            {
                if (item.Task != null)
                {
                    pagingList.Add(item.Task.ToJson());
                }
                if (item.Inspection != null)
                {
                    pagingList.Add(item.Inspection.ToJson());
                }
            }
        }

        private static List<JournalDateItem> paging(int skip, ref int limit, List<JournalDateItem> listFull)
        {
            var listSkiplimit = new List<JournalDateItem>();
            if (skip < listFull.Count)
            {
                if (limit + skip > listFull.Count)
                    limit = listFull.Count - skip;

                for (int i = skip; i < limit + skip; i++)
                {
                    listSkiplimit.Add(listFull[i]);
                }
            }

            return listSkiplimit;
        }

        private static void changeInspetionTypeForUi(JournalInspection journalInspection)
        {
            if (journalInspection.OwnerBrigadeType != null)
            {
                //30 - Приёмка поезда ПР
                if (journalInspection.OwnerBrigadeType == BrigadeType.Receiver && journalInspection.TypeId == 2)
                    journalInspection.TypeId = 30;
                //31 - Выпуск поезда ПР
                if (journalInspection.OwnerBrigadeType == BrigadeType.Receiver && journalInspection.TypeId == 3)
                    journalInspection.TypeId = 31;
                //32 - Приёмка поезда ЛБ
                if (journalInspection.OwnerBrigadeType == BrigadeType.Locomotiv && journalInspection.TypeId == 2)
                    journalInspection.TypeId = 32;
                //33 - Сдача поезда ЛБ
                if (journalInspection.OwnerBrigadeType == BrigadeType.Locomotiv && journalInspection.TypeId == 3)
                    journalInspection.TypeId = 33;
            }
        }

        private async Task GetInspectionCouters(InspectionDataRepository inspdatarep, Inspection fromSqlsInspection, JournalInspection journalInspection)
        {
            var countersFromSql = await inspdatarep.GetByInspectionId(fromSqlsInspection.Id);
            if (countersFromSql.Any())
            {
                journalInspection.Tabs.Description.KmPerShift = String.Join(", ",
                    countersFromSql.Where(o => o.Type == InspectionDataType.KmPerShift)
                        .Select(o => o.Value));
                journalInspection.Tabs.Description.KmTotal = String.Join(", ",
                    countersFromSql.Where(o => o.Type == InspectionDataType.KmTotal)
                        .Select(o => o.Value));
                journalInspection.Tabs.Description.KwHours = String.Join(", ",
                    countersFromSql.Where(o => o.Type == InspectionDataType.KwHours)
                        .Select(o => _GetCarriageNumCanBeNull(o.Carriage) + ":" + o.Value));
                var brakeShoes = String.Join(", ",
                    countersFromSql.Where(o => o.Type == InspectionDataType.BrakeShoes)
                        .Select(o => o.Text));
                journalInspection.Tabs.BrakeShoesSerial = brakeShoes;
            }
        }

        private static async Task getInspectionMeterage(MeterageRepository mR, Inspection fromSqlsInspection, JournalInspection journalInspection)
        {
            var temps = new List<JournalInspectionTemp>();
            var tempsFromSql = await mR.GetMeterages(fromSqlsInspection.Id);
            foreach (var tempFromSql in tempsFromSql)
            {
                temps.Add(new JournalInspectionTemp
                {
                    Value = tempFromSql.Value,
                    TimeStamp = tempFromSql.Date
                });
            }
            journalInspection.Tabs.Temps = temps.ToArray();
        }

        private static async Task getInspectionLabels(MeterageRepository mR, Inspection fromSqlsInspection, JournalInspection journalInspection)
        {
            var labels = new List<JournalInspectionLabel>();
            var labelsFromSql = await mR.GetLabels(fromSqlsInspection.Id);
            foreach (var labelFromSql in labelsFromSql)
            {
                labels.Add(new JournalInspectionLabel
                {
                    CarriageName = CreateCarriageName(labelFromSql.Label.Carriage.Train.Name, labelFromSql.Label.Carriage.Number),
                    EquipmentName = labelFromSql.Label.EquipmentModel.Equipment.Name,
                    LabelSerial = labelFromSql.Label.Rfid,
                    TimeStamp = labelFromSql.Date
                });
            }
            journalInspection.Tabs.Labels = labels.ToArray();
        }

        private static async Task<bool> InspectionFilter(string filter, TaskRepository sqlRTask, Inspection fromSqlsInspection, JournalInspection journalInspection)
        {
            var isFiltered = false;
            if (filter != null)
            {

                var filters = JsonConvert.DeserializeObject<TaskCommon.FilterBody[]>(filter);
                foreach (var item in filters)
                {
                    switch (item.Filter)
                    {
                        case "DateFrom":
                            DateTime.TryParse(item.Value, out var dateStart);
                            if (fromSqlsInspection.DateStart < dateStart)
                                isFiltered = true;
                            break;

                        case "DateTo":
                            DateTime.TryParse(item.Value, out var dateEnd);
                            dateEnd = dateEnd.AddDays(1);
                            if (fromSqlsInspection.DateStart > dateEnd)
                                isFiltered = true;
                            break;
                        case "InspectionId":
                            int.TryParse(item.Value, out var filterId);
                            if (fromSqlsInspection.Id != filterId)
                                isFiltered = true;
                            break;
                        case "TrainName":
                            if (!journalInspection.TrainName.ToLower().Contains(item.Value.ToLower()))
                                isFiltered = true;
                            break;
                        case "TrainId":
                            int.TryParse(item.Value, out var trainid);
                            if (journalInspection.TrainId != trainid)
                                isFiltered = true;
                            break;
                        //Нет вагонов у инспекций
                        case "CarriageId":
                            isFiltered = true;
                            break;
                        //Инициатор
                        case "OwnerId":
                            if (!journalInspection.UserName.ToLower().Contains(item.Value.ToLower()))
                                isFiltered = true;
                            break;
                        //TODO узнать про статусы инспекций
                        //Статус инспекции, ахуеть
                        case "StatusId":
                            int.TryParse(item.Value, out var intResult);
                            if (journalInspection.StatusId != intResult)
                                isFiltered = true;
                            break;

                        //нет у инспекций исполнителя
                        case "BrigadeId":
                            isFiltered = true;
                            break;
                        //Мероприятие блядь или тип инспекции в UI
                        case "TypeId":
                            int.TryParse(item.Value, out var intResultT);
                            if (journalInspection.TypeId != intResultT)
                                isFiltered = true;
                            break;
                        case "TaskOrInspectionId":
                            if (!journalInspection.Id.ToString().Equals(item.Value.ToLower()))
                                isFiltered = true;
                            break;

                        case "Equipment":
                            var internalFilters = new List<FilterBody>
                                    {
                                        new FilterBody { Filter = "Equipment", Value = item.Value},
                                        new FilterBody { Filter = "InspectionId", Value = journalInspection.Id.ToString()},
                                    };
                            var tasksFiltered = await sqlRTask.GetAll(0, 9999999, internalFilters.ToArray().ToJson());
                            bool foreachFiltered = false;
                            foreach (var task in tasksFiltered.Data)
                            {
                                if (task.EquipmentName.ToLower().Contains(item.Value.ToLower()))
                                    foreachFiltered = true;
                            }
                            if (!foreachFiltered)
                                isFiltered = true;
                            break;
                    }

                }
            }

            return isFiltered;
        }

        private string _GetInspectionInfo(int? Id, int? type)
        {
            string ret = null;
            if (Id != null && type != null)
            {
                string typeStr = null;
                var typeE = (CheckListType)type;
                switch (typeE)
                {
                    case CheckListType.Inspection:
                        typeStr = "Приемка поезда";
                        break;
                    case CheckListType.Surrender:
                        typeStr = "Сдача поезда";
                        break;
                    case CheckListType.TO1:
                        typeStr = "ТО-1";
                        break;
                    case CheckListType.TO2:
                        typeStr = "ТО-2";
                        break;
                    default:
                        typeStr = "неизв. тип инспекции";
                        break;
                }

                ret = $"{typeStr} ({Id})";
            }

            return ret;
        }

        public class JournalDateItem
        {
            public DateTime Date { get; set; }
            public JournalInspection Inspection { get; set; }
            public JournalTask Task { get; set; }
        }

        public class JournalPaging : ErrorResponse
        {
            public string[] Data { get; set; }
            public int Total { get; set; }
        }


        private class JournalInspectionFromSql
        {

        }

        public class JournalInspection
        {
            public int Id { get; set; }
            public string Type { get; set; }
            public int InspectionId { get; set; }
            public int StatusId { get; set; }
            public int TypeId { get; set; }
            public string TrainName { get; set; }
            public int TrainId { get; set; }
            public string UserName { get; set; }
            public DateTime Date { get; set; }
            public BrigadeType? OwnerBrigadeType { get; set; }

            //public string CarriageName { get; set; }
            //public string EventName { get; set; }
            //public string EquipmentName { get; set; }
            //public string FaultName { get; set; }
            public JournalInspectionTabs Tabs { get; set; }
        }

        public class JournalTaskFromSql
        {
            public int TaskType { get; set; }
            public DateTime CreateDate { get; set; }
            public int TaskId { get; set; }
            public string UserName { get; set; }
            public string TrainName { get; set; }
            public int CarriageNumber { get; set; }
            public int CarriageId { get; set; }
            public string EquipmentName { get; set; }
            public string FaultName { get; set; }
            public int TrainId { get; set; }

            public int? InspectionId { get; set; }
            public int? InspectionType { get; set; }
            public string CarriagesSerialNumber { get; set; }

            public int TaskAttributeId { get; set; }
            //public DateTime InspectionsDateEnd { get; set; }
            //public DateTime InspectionsDateStart { get; set; }
            //public int InspectionsStatus { get; set; }
        }



        public class Journal
        {
            public JournalTask Task { get; set; }
            //public JournalInspection Inspection { get; set; }
        }

        public class JournalTask
        {
            public string Type { get; set; }
            public int TaskId { get; set; }
            public int? StatusId { get; set; }
            public int TaskType { get; set; }
            public string TrainName { get; set; }
            public int TrainId { get; set; }
            public string UserName { get; set; }
            public DateTime Date { get; set; }
            public string CarriageName { get; set; }
            public int CarriageNum { get; set; }
            public int CarriageId { get; set; }
            public string EquipmentName { get; set; }
            public string FaultName { get; set; }
            public int ExecutorBrigadeType { get; set; }
            //public int? InspectionId { get; set; }
            //public int? InspectionTypeId { get; set; }
            public string InspectionInfo { get; set; }
            public string CarriagesSerialNumber { get; set; }
            public int TaskAttributeId { get; set; }
        }




        public class JournalInspectionTabs
        {
            public JournalInspectionDescription Description { get; set; }
            public JournalInspectionLabel[] Labels { get; set; }
            public JournalInspectionTemp[] Temps { get; set; }
            public string BrakeShoesSerial { get; set; }
        }

        public class JournalInspectionDescription
        {
            public int Number { get; set; }
            public DateTime? DataEnd { get; set; }
            public int TasksCount { get; set; }
            public int AllLabelCount { get; set; }
            public int TaskLabelCount { get; set; }
            public int TempCount { get; set; }
            public string KmPerShift { get; set; }
            public string KmTotal { get; set; }
            public string KwHours { get; set; }
        }

        public class JournalInspectionLabel
        {
            public string CarriageName { get; set; }
            public string EquipmentName { get; set; }
            public DateTime TimeStamp { get; set; }
            public string LabelSerial { get; set; }
        }

        public class JournalInspectionTemp
        {
            public int Value { get; set; }
            public DateTime TimeStamp { get; set; }
        }

        private string _GetCarriageNumCanBeNull(Carriage car)
        {
            if (car == null)
                return "NULL";

            return car.Serial;
        }

    }
}