using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rzdppk.Api.Dto.EventTable
{
    public class InspectionByIdDto
    {
        /// <summary>
        /// ИД мероприятия с префиксом М
        /// </summary>
        public string Id { get; set; }

        //Для выборки из sql
        public int InspectionId { get; set; }

        /// <summary>
        /// Тип ТО-1
        /// </summary>
        public string Type { get; set; }
        [JsonIgnore]
        public int TypeInt { get; set; }

        public string TrainName { get; set; }

        public string Status { get; set; }
        [JsonIgnore]
        public int StatusInt { get; set; }

        /// <summary>
        /// Кто создал
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Какая Бригада выполнила работы
        /// </summary>
        public string BrigadeName { get; set; }
        [JsonIgnore]
        public int BrigadeTypeInt { get; set; }

        /// <summary>
        /// Дата начала и окночания мероприятия, если DateEnd=NUll - пишем только дату начала
        /// </summary>
        public string Date { get; set; }
        //Для выборки из sql
        [JsonIgnore]
        public DateTime DateStart { get; set; }
        [JsonIgnore]
        //Для выборки из sql
        public DateTime? DateEnd { get; set; }

        public int TaskCount { get; set; }

        public List<InspectionLabelDto> Labels { get; set; } = new List<InspectionLabelDto>();
        public List<InspectionDataCarriageDto> InspectionDataCarriages { get; set; } = new List<InspectionDataCarriageDto>();
        public List<InspectionDataDto> InspectionDataUis { get; set; } = new List<InspectionDataDto>();

        /// <summary>
        /// Подпись при приемке\сдаче поезда  Table - Signatures поле CaptionImage
        /// </summary>
        public string Signature { get; set; }



        //Данные по считанным меткам
        public class InspectionLabelDto
        {
            public string CarriageName { get; set; }
            public string EquipmentName { get; set; }
            public string Date { get; set; }
            public string Label { get; set; }
        }

        //Данные по Квт ч, приходят по вагонам
        public class InspectionDataCarriageDto
        {
            public string CarriageName { get; set; }
            public int Value { get; set; }
        }

        //Остальные доп. данные по инспекции (все кроме InspectionDataType=KwHours)
        public class InspectionDataDto
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }

    }
}