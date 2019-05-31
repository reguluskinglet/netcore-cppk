using System;
using System.Collections.Generic;
using System.Text;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Helpers
{
    public static class EnumStringHelper
    {
        public static string GetDescription(this TripType type)
        {
            switch (type)
            {
                case TripType.Transfer:
                    return "Перегонный";
                default:
                    return "";
            }
        }

        public static string GetDescription(this BrigadeType type)
        {
            switch (type)
            {
                case BrigadeType.Locomotiv:
                    return "Локомотивная бригада";
                case BrigadeType.Depo:
                    return "Бригада Депо";
                case BrigadeType.Receiver:
                    return "Приемщики";
                default:
                    throw new NotImplementedException();
            }
        }

        public static string ShortDescription(this BrigadeType type)
        {
            switch (type)
            {
                case BrigadeType.Locomotiv:
                    return "ЛБ";
                case BrigadeType.Depo:
                    return "БД";
                case BrigadeType.Receiver:
                    return "ПР";
                default:
                    throw new NotImplementedException();
            }
        }

        public static string GetDescription(this TaskStatus status)
        {
            switch (status)
            {
                case TaskStatus.New:
                    return "Новая";
                case TaskStatus.Confirmation:
                    return "К подтверждению";
                case TaskStatus.Remake:
                    return "Не прошла проверку";
                case TaskStatus.ToCheck:
                    return "К проверке";
                case TaskStatus.Done:
                    return "Выполнено";
                case TaskStatus.NotConfirmed:
                    return "Не подтверждено";
                case TaskStatus.AcceptedForExecution:
                    return "Принято к исполнению";
                case TaskStatus.Viewed:
                    return "Просмотрено";
                case TaskStatus.InWork:
                    return "В работе";
                case TaskStatus.Log:
                    return "В журнале";
                default:
                    return "Закрыта";
            }
        }

        public static string GetDescription(this InspectionStatus status)
        {
            switch (status)
            {
                case InspectionStatus.Create:
                    return "Создан";
                case InspectionStatus.Canseled:
                    return "Отменен";
                default:
                    return "Выполнен";
            }
        }

        public static string GetDescription(this CheckListType? type)
        {
            if(!type.HasValue)
                return "Тех.";

            return GetDescription(type.Value);
        }

        public static string GetDescription(this TaskType type)
        {
            switch (type)
            {
                case TaskType.Service:
                    return "Сервис.";
                case TaskType.Cto:
                    return "СТО";
                default:
                    return "Тех.";
            }
        }

        public static string GetDescription(this CheckListType type)
        {
            switch (type)
            {
                case CheckListType.TO1:
                    return "ТО-1";
                case CheckListType.TO2:
                    return "ТО-2";
                case CheckListType.Inspection:
                    return "Приемка";
                case CheckListType.Surrender:
                    return "Сдача";
                case CheckListType.CTO:
                    return "Санитарная";
                default:
                    return "Тех.";
            }
        }

        public static string GetDescription(this CheckListType type, BrigadeType brigade)
        {
            switch (type)
            {
                case CheckListType.TO1:
                    return "ТО-1";
                case CheckListType.TO2:
                    return "ТО-2";
                case CheckListType.Inspection:
                    return "Приемка";
                case CheckListType.Surrender:
                    return brigade== BrigadeType.Locomotiv ? "Сдача":"Выпуск";
                default:
                    throw new NotImplementedException();
            }
        }

        public static string GetDescription(this TaskLevel level)
        {
            switch (level)
            {
                case TaskLevel.Critical:
                    return "Критичный";
                case TaskLevel.Low:
                    return "Низкий";
                case TaskLevel.Normal:
                    return "Нормальный";
                default:
                    throw new NotImplementedException();
            }
        }


        public static string GetDescription(this InspectionDataType type)
        {
            switch (type)
            {
                case InspectionDataType.KmTotal:
                    return "Км общий";
                case InspectionDataType.KmPerShift:
                    return "Км за смену";
                case InspectionDataType.BrakeShoes:
                    return "Тормозные башмаки";
                case InspectionDataType.KwHours:
                    return "Квт час";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
