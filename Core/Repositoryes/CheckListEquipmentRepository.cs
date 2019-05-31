using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public class CheckListEquipmentRepository : ICheckListEquipmentRepository
    {
        private readonly IDb _db;

        public CheckListEquipmentRepository(IDb db)
        {
            _db = db;
        }

        public async Task<List<CheckListEquipmentTableItem>> GetList(int equipmentModleId)
        {
            const string sql = "select * from [CheckListEquipments] where EquipmentModelId=@EquipmentModelId";

            var items = (await _db.Connection.QueryAsync<CheckListEquipment>(sql, new { EquipmentModelId = equipmentModleId })).ToList();

            return items.Select(o => new CheckListEquipmentTableItem
            {
                Id = o.Id,
                NameTask = o.NameTask,
                FaultType = FaultTypeEnumToString(o.FaultType),
                TaskLevel = TaskLevelEnumToString(o.TaskLevel),
                Value = o.Value.ToString(),
                ValueType = CheckListValueTypeEnumToString(o.ValueType),
                CheckListType = (int) o.CheckListType
            }).ToList();
        }

        public async Task<CheckListEquipmentTableItem> AddChecklist(CheckListEquipmentDto input)
        {
            var res = ValidateChecklistDto(input);

            if (!res.IsValid)
            {
                throw new Exception("обнаружены ошибки: " + string.Join(", ", res.Errors));
            }

            const string sql =
                @"insert into [CheckListEquipments] ([CheckListType],[EquipmentModelId],[FaultType],[NameTask],[Value],[ValueType],[TaskLevel])
                                values(@CheckListType,@EquipmentModelId,@FaultType,@NameTask,@Value,@ValueType,@TaskLevel) select scope_identity()";
            var id = await _db.Connection.QuerySingleAsync<int>(sql, new
            {
                CheckListType = input.CheckListType,
                EquipmentModelId = input.EquipmentModelId,
                FaultType = input.FaultType,
                NameTask = input.NameTask,
                Value = input.Value,
                ValueType = input.ValueType,
                TaskLevel = input.TaskLevel
            });

            //загрузить и вернуть запись по ид? или УИ сам перезапросит всю таблицу?
            var item = await GetById(id);

            return new CheckListEquipmentTableItem
            {
                Id = item.Id,
                NameTask = item.NameTask,
                FaultType = FaultTypeEnumToString(item.FaultType),
                TaskLevel = TaskLevelEnumToString(item.TaskLevel),
                Value = item.Value.ToString(),
                ValueType = CheckListValueTypeEnumToString(item.ValueType),
                CheckListType = (int) item.CheckListType
            };
        }

        private async Task<CheckListEquipment> GetById(int id)
        {
            const string sql = "select * from [CheckListEquipments] where id=@Id";

            var item = (await _db.Connection.QueryAsync<CheckListEquipment>(sql, new { Id = id })).First();

            return item;
        }

        //id CheckListEquipment
        public async Task DeleteChecklist(int id)
        {
            var name = await _db.Connection
                                .ExecuteScalarAsync<string>(@"select top 1 cle.NameTask from [TrainTaskAttributes] tta
                                                                  join [CheckListEquipments] cle
                                                                  on cle.Id = tta.CheckListEquipmentId
                                                                  where tta.CheckListEquipmentId = @Id",
                                                            new {Id = id});
            if (name!=null)
            {
                throw new Other.Other.CantDeleteException($"Вы не можете удалить запись. Имеются задачи по чеклисту \"{name}\"");
            }
            const string sql = "delete from [CheckListEquipments] where Id=@Id";

            await _db.Connection.ExecuteAsync(sql, new {Id = id});
        }

        private CheckListEquipmentDtoValidationResult ValidateChecklistDto(CheckListEquipmentDto input)
        {
            var ret = new CheckListEquipmentDtoValidationResult();

            if (string.IsNullOrEmpty(input.NameTask))
            {
                ret.IsValid = false;
                ret.Errors.Add("Значение наименования не может быть пустым");
            }

            if(input.ValueType <= 0)
            {
                ret.IsValid = false;
                ret.Errors.Add("Не указан тип проверки");
            }

            if (input.ValueType == CheckListValueType.Bool)
            {
                if (input.Value < 0 || input.Value > 1)
                {
                    ret.IsValid = false;
                    ret.Errors.Add("Недопустимое значение для типа булево");
                }
            }

            if (input.ValueType == CheckListValueType.Int)
            {
                if (input.Value < 0)
                {
                    ret.IsValid = false;
                    ret.Errors.Add("Недопустимое значение для типа число");
                }
            }           

            return ret;
        }

        public class CheckListEquipmentDtoValidationResult
        {
            public bool IsValid { get; set; } = true;

            public List<string> Errors { get; set; } = new List<string>();
        }

        private static string FaultTypeEnumToString(TaskType type)
        {
            var ret = $"неизвестный тип ({type.ToString()})";

            switch (type)
            {
                case TaskType.Cto:
                    ret = "Санитарная";
                    break;
                case TaskType.Service:
                    ret = "???";
                    break;
                case TaskType.Technical:
                    ret = "Техническая";
                    break;
            }

            return ret;
        }

        private static string CheckListValueTypeEnumToString(CheckListValueType type)
        {
            var ret = $"неизвестный тип ({type.ToString()})";

            switch (type)
            {
                case CheckListValueType.Null:
                    ret = "Не выбрано";
                    break;
                case CheckListValueType.Int:
                    ret = "Число";
                    break;
                case CheckListValueType.Bool:
                    ret = "Булево";
                    break;
                case CheckListValueType.UserSelect:
                    //???
                    break;
            }

            return ret;
        }

        private static string TaskLevelEnumToString(TaskLevel type)
        {
            var ret = $"неизвестный тип ({type.ToString()})";

            switch (type)
            {
                case TaskLevel.Low:
                    ret = "Низкийё";
                    break;
                case TaskLevel.Critical:
                    ret = "Нормальный";
                    break;
                case TaskLevel.Normal:
                    ret = "Критичный";
                    break;
            }

            return ret;
        }


        public class CheckListEquipmentDto
        {
            public CheckListType CheckListType { get; set; }

            public int EquipmentModelId { get; set; }

            public TaskType FaultType { get; set; }

            public string NameTask { get; set; }

            public int Value { get; set; }

            public CheckListValueType ValueType { get; set; } 

            public TaskLevel TaskLevel { get; set; }
        }

        public class CheckListEquipmentTableItem
        {
            public int Id { get; set; }

            public string FaultType { get; set; }

            public string NameTask { get; set; }

            public string Value { get; set; }

            public string ValueType { get; set; }

            public string TaskLevel { get; set; }

            public int CheckListType { get; set; }
        }
    }
}