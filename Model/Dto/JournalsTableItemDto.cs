using System;
using Rzdppk.Model.Enums;

namespace Rzdppk.Model.Dto
{
    public class JournalsTableItemDto
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public string CarriageName { get; set; }
        public string EquipmentName { get; set; }
        public string TrainName { get; set; }
        public string Type { get; set; }
        public string Date { get; set; }
        public DateTime OrderDate { get; set; }
        public bool HasInspection { get; set; }
    }
}