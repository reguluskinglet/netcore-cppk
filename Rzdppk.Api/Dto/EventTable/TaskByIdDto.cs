using System;
using System.Collections.Generic;

namespace Rzdppk.Api.Dto.EventTable
{
    public class TaskByIdDto
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string InitiatorName { get; set; }
        public string TrainName { get; set; }
        public string EquipmentName { get; set; }
        public string CarriageSerial { get; set; }
        public int TaskType { get; set; }
        public int StatusId { get; set; }
        public int BrigadeType { get; set; }
        public int TaskLevel { get; set; }
        public List<int> PossibleTaskStatuses { get; set; }
        public List<FaultTaskDto> Faults { get; set; }
        public List<InspectionTaskDto> Inspections { get; set; }
        public List<HistoryTaskDto> History { get; set; }



        public class FaultTaskDto
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public string User { get; set; }
            public string Text { get; set; }
        }

        public class InspectionTaskDto
        {
            public int Id { get; set; }
            public DateTime DateStart { get; set; }
            public DateTime? DateEnd { get; set; }
            public string User { get; set; }
            public int Type { get; set; }
            public int? BrigadeType { get; set; }
            public List<string> Texts { get; set; }
        }

        public class FileTaskDto
        {
            public int DocumentType { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class HistoryTaskDto
        {
            public DateTime Date { get; set; }
            public string User { get; set; }
            public int? UserBrigadeType { get; set; }
            public string Type { get; set; }
            public int? OldStatus { get; set; }
            public int? NewStatus { get; set; }
            public int? OldExecutorBrigadeType { get; set; }
            public int? NewExecutorBrigadeType { get; set; }
            public string Text { get; set; }
            public List<FileTaskDto> Files { get; set; }
        }


    }
}