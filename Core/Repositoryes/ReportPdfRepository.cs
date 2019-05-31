using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public class ReportPdfRepository
    {
        private readonly ILogger _logger;
        
        public ReportPdfRepository(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<List<Tu152TaskFaultFinalExtended>> GetTu152PdfTasksData(int[] taskIds)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                const string sqlFaults = @"select t.*,u.*,c.*,tr.*,e.*, ta.*,f.*
                    from TrainTaskAttributes ta
                    left join TrainTasks t on t.Id=ta.TrainTaskId
                    left join auth_users u on u.Id=t.UserId
                    left join Carriages c on c.Id=t.CarriageId
                    left join Trains tr on tr.id=c.TrainId
                    left join EquipmentModels em on em.Id=t.EquipmentModelId
                    left join Equipments e on e.Id=em.EquipmentId
                    left join Faults f on f.Id=ta.FaultId where t.Id in @ids";

                var taskFaults = (await conn.QueryAsync<TrainTask, User, Carriage, Train, Equipment, TrainTaskAttribute, Fault, Tu152TaskData>(
                    sqlFaults,
                    (task, user, carriage, train, equipment, attribute, fault) =>
                    {
                        var ret = new Tu152TaskData();
                        ret.Task = task;
                        ret.Task.User = user;
                        ret.Attribute = attribute;
                        ret.Carriage = carriage;
                        ret.Carriage.Train = train;
                        ret.Equipment = equipment;
                        ret.Attribute.Fault = fault;

                        return ret;
                    }, new { ids = taskIds })).ToList();

                var res = taskFaults.GroupBy(o => o.Task.Id)
                    .Select(o => new Tu152TaskFaultFinalExtended
                    {
                        TaskId = o.First().Task.Id,
                        Date = o.First().Task.CreateDate,
                        UserName = o.First().Task.User.Name,
                        TrainName = o.First().Carriage.Train.Name,
                        CarriageSerial = o.First().Carriage.Serial,
                        EquipmentName = o.First().Equipment.Name,
                        Faults = string.Join(", ", o.Select(f => GetFaultName(f.Attribute.Fault?.Name, f.Attribute.Description)))
                    }).ToList();

                return res;
            }
        }

        public async Task<List<Tu152PdfItem>> GetTu152PdfData(int[] inspectionIds)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var ret = new List<Tu152PdfItem>();

                const string sql =
                    @"select i.*,TechPass.DateDrawUp as ActDateDrawUp,TechPass.Number as ActNumber,t.*,u.*,s.*
                    from Inspections i
                    left join Trains t on t.Id=i.TrainId
					OUTER APPLY (select top 1 * from TechPasses tp where tp.TrainId=t.Id order by tp.DateDrawUp desc) as TechPass
                    left join auth_users u on u.Id=i.UserId
                    left join Signatures s on s.InspectionId=i.id and s.UserId=i.UserId
                    where i.id in @ids";

                var inspections = (await conn.QueryAsync<InspectionWithAct, Train, User, Signature, InspectionWithAct>(
                    sql,
                    (inspection, train, user, signature) =>
                    {
                        inspection.User = user;
                        inspection.Train = train;
                        inspection.Signatures = new List<Signature> { signature };
                        return inspection;
                    }, new { ids = inspectionIds })).ToList();

                //get meterages, shoes, km
                const string sqlData = "select * from InspectionData d left join Carriages c on c.Id=d.CarriageId where d.InspectionId in @ids";
                var datas = (await conn.QueryAsync<InspectionData, Carriage, InspectionData>(
                    sqlData,
                    (data, carriage) =>
                    {
                        data.Carriage = carriage;
                        return data;
                    }, new {ids = inspectionIds})).ToList();

                //task values
                var ids1 = inspections.Where(o => o.CheckListType == CheckListType.Inspection || o.CheckListType == CheckListType.Surrender).Select(o => o.Id).ToArray();
                
                const string sqlTaskValues = @"select t.Id as TaskId,c.Number as CarriageNumber, c.Serial as CarriageSerial, e.Name as EquipmentName, cl.Value as ValueNorm, ta.Value as ValueFact, cl.ValueType, cl.NameTask as NameChecklist, ta.InspectionId
                    from TrainTaskAttributes ta
                    left join TrainTasks t on t.Id=ta.TrainTaskId
                    left join Carriages c on c.Id=t.CarriageId
                    left join EquipmentModels em on em.Id=t.EquipmentModelId
                    left join Equipments e on e.Id=em.EquipmentId
                    left join CheckListEquipments cl on cl.Id=ta.CheckListEquipmentId
                    where ta.Value != cl.Value and cl.ValueType in @Types and ta.InspectionId in @ids";
                var taskValues = (await conn.QueryAsync<Tu152TaskValue>(sqlTaskValues, new {Types = new[] {CheckListValueType.Int, CheckListValueType.Bool}, ids = ids1})).ToList();

                //task faults
                var ids2 = inspections.Where(o => o.CheckListType == CheckListType.TO1 || o.CheckListType == CheckListType.TO2).Select(o => o.Id).ToArray();

                const string sqlFaults = @"select t.Id as TaskId,c.Number as CarriageNumber, c.Serial as CarriageSerial, e.Name as EquipmentName, ta.Description as FaultDescription, f.Name as FaultName, ta.InspectionId
                    from TrainTaskAttributes ta
                    left join TrainTasks t on t.Id=ta.TrainTaskId
                    left join Carriages c on c.Id=t.CarriageId
                    left join EquipmentModels em on em.Id=t.EquipmentModelId
                    left join Equipments e on e.Id=em.EquipmentId
                    left join Faults f on f.Id=ta.FaultId where ta.InspectionId in @ids";

                var taskFaults = (await conn.QueryAsync<Tu152TaskFault>(sqlFaults, new { ids = ids2 })).ToList();

                foreach (var inspection in inspections)
                {
                    if (inspection.CheckListType == CheckListType.Inspection ||
                        inspection.CheckListType == CheckListType.TO1 ||
                        inspection.CheckListType == CheckListType.TO2 || 
                        inspection.CheckListType == CheckListType.Surrender)
                    {
                        var item = new Tu152PdfItem
                        {
                            User = inspection.User,
                            Train = inspection.Train,
                            Signature = inspection.Signatures.FirstOrDefault()?.CaptionImage,
                            Type = inspection.CheckListType,
                            Date = inspection.DateStart,
                            DateAct = inspection.ActDateDrawUp,
                            NumberAct = inspection.ActNumber,

                            CarriageKwMeterages = datas.Where(o => o.InspectionId == inspection.Id && o.Type == InspectionDataType.KwHours)
                                .Select(
                                    o => new Tu152KwCarriage
                                    {
                                        Carriage = o.Carriage,
                                        Value = o.Value
                                    }).ToList()
                        };

                        var kmPerShiftData = datas.FirstOrDefault(o =>
                            o.InspectionId == inspection.Id && o.Type == InspectionDataType.KmPerShift);
                        if (kmPerShiftData != null)
                        {
                            item.KmPerShift = kmPerShiftData.Value;
                        }

                        var kmTotalData = datas.FirstOrDefault(o =>
                            o.InspectionId == inspection.Id && o.Type == InspectionDataType.KmTotal);
                        if (kmTotalData != null)
                        {
                            item.KmTotal = kmTotalData.Value;
                        }

                        item.BrakeShoes = datas
                            .Where(o => o.InspectionId == inspection.Id && o.Type == InspectionDataType.BrakeShoes)
                            .Select(o => o.Text).ToList();

                        item.TaskValues = taskValues.Where(o => o.InspectionId == inspection.Id).ToList();

                        item.TaskFaults = taskFaults.Where(o => o.InspectionId == inspection.Id).GroupBy(o => o.TaskId)
                            .Select(o => new Tu152TaskFaultFinal
                            {
                                TaskId = o.First().TaskId,
                                CarriageSerial = o.First().CarriageSerial,
                                EquipmentName = o.First().EquipmentName,
                                Faults = string.Join(", ", o.Select(f => GetFaultName(f.FaultName, f.FaultDescription)))
                            }).ToList();

                        ret.Add(item);
                    }
                }

                return ret;
            }
        }

        private static string GetFaultName(string name, string description)
        {
            string ret = "";

            if (string.IsNullOrEmpty(name))
            {
                ret = description;
            }
            else if (string.IsNullOrEmpty(description))
            {
                ret = name;
            }
            else
            {
                ret = $"{name} ({description})";
            }

            return ret;
        }
        
        //tu152 pdf
        public class Tu152PdfItem
        {
            public Train Train { get; set; }

            public User User { get; set; }

            public string Signature { get; set; }

            public DateTime Date { get; set; }

            public CheckListType Type { get; set; }

            public List<Tu152KwCarriage> CarriageKwMeterages { get; set; }

            public int KmTotal { get; set; }

            public int KmPerShift { get; set; }

            public List<string> BrakeShoes { get; set; }

            public DateTime DateAct { get; set; }

            public string NumberAct { get; set; }

            public List<Tu152TaskValue> TaskValues { get; set; }

            public List<Tu152TaskFaultFinal> TaskFaults { get; set; }
        }

        public class Tu152KwCarriage
        {
            public Carriage Carriage { get; set; }

            public int Value { get; set; }
        }

        public class Tu152TaskValue
        {
            public int TaskId { get; set; }

            public int CarriageNumber { get; set; }

            public string CarriageSerial { get; set; }

            public string EquipmentName { get; set; }

            public int ValueNorm { get; set; }

            public int ValueFact { get; set; }

            public string NameChecklist { get; set; }

            public CheckListValueType ValueType { get; set; }

            public int InspectionId { get; set; }
        }

        public class Tu152TaskFault
        {
            public int TaskId { get; set; }

            public int CarriageNumber { get; set; }

            public string CarriageSerial { get; set; }

            public string EquipmentName { get; set; }

            public int InspectionId { get; set; }

            public string FaultName { get; set; }

            public string FaultDescription { get; set; }
        }

        public class Tu152TaskData
        {
            public TrainTask Task { get; set; }

            public TrainTaskAttribute Attribute { get; set; }

            public Carriage Carriage { get; set; }

            public Equipment Equipment { get; set; }
        }

        public class Tu152TaskFaultFinal
        {
            public int TaskId { get; set; }

            public string CarriageSerial { get; set; }

            public string EquipmentName { get; set; }

            public string Faults { get; set; }
        }

        public class Tu152TaskFaultFinalExtended
        {
            public int TaskId { get; set; }

            public string CarriageSerial { get; set; }

            public string EquipmentName { get; set; }

            public string Faults { get; set; }

            public DateTime Date { get; set; }

            public string TrainName { get; set; }

            public string UserName { get; set; }
        }

        public class InspectionWithAct: Inspection
        {
            public DateTime ActDateDrawUp { get; set; }
            
            public string ActNumber { get; set; }
        }
    }
}
