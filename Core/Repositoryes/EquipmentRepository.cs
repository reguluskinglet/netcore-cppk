using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using Rzdppk.Model;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly EquipmentsSql _sql;
        private readonly ILogger _logger;

        public EquipmentRepository(ILogger logger)
        {

            _sql = new EquipmentsSql();
            _logger = logger;
        }

        public async Task<NewEquipmentUIPaging> GetEquipment(int modelId, int parentId, int skip, int limit)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                _logger.LogTrace(
                    $"start GetEquipmentWithCheckLists modelId = {modelId}, parentId = {parentId}, skip = {skip}, limit = {limit}");

                List<EquipmentModel> result;
                int count = 0;
                if (parentId == 0)
                {
                    var sql = Sql.SqlQueryCach["Equipment.GetEquipmentModelByModelAndParentNull"];
                    result = (await conn.QueryAsync<EquipmentModel>(
                        sql,
                        new { model_id = modelId, skip = skip, limit = limit, parent_id = parentId })).ToList();

                    var sqlc = Sql.SqlQueryCach["Equipment.CountEquipmentModelByModelAndParentNull"];
                    count = (await conn.QueryAsync<int>(sqlc, new { model_id = modelId })).FirstOrDefault();
                }
                else
                {
                    var sql = Sql.SqlQueryCach["Equipment.GetEquipmentModelByModelAndParent"];
                    result = (await conn.QueryAsync<EquipmentModel>(
                        sql,

                        new { model_id = modelId, skip = skip, limit = limit, parent_id = parentId })).ToList();
                    var sqlc = Sql.SqlQueryCach["Equipment.CountEquipmentModelByModelAndParent"];
                    count = (await conn.QueryAsync<int>(sqlc, new { model_id = modelId, parent_id = parentId }))
                        .FirstOrDefault();
                }

                var list = new List<EquipmentUI>();
                foreach (var eqm in result)
                {
                    var eqWithCheckLists = await GetEquipmentModelById(eqm.Id);
                    eqWithCheckLists.ParentId = eqm.ParentId;
                    eqWithCheckLists.Id = eqm.Id;
                    eqWithCheckLists.ModelId = eqm.ModelId;
                    eqWithCheckLists.IsMark = eqm.IsMark;
                    list.Add(eqWithCheckLists);
                }

                var sqlrM = new ModelRepository(_logger);
                var model = await sqlrM.GetById(modelId);

                var ret = new NewEquipmentUIPaging
                {
                    Data = list.ToArray(),
                    Model = model,
                    Total = count
                };
                return ret;
            }
        }

        public async Task<NewEquipmentUIPaging> GetEquipment(int modelId, int parentId, int skip, int limit, string filter)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var all = await GetEquipment(modelId, parentId, 0, Int32.MaxValue);
                var filters = JsonConvert.DeserializeObject<Other.Other.FilterBody[]>(filter);
                var list = new List<EquipmentUI>();
                foreach (var itemAll in all.Data)
                {

                    if (itemAll.Equipment.Name.ToLower().Contains(filters.FirstOrDefault().Value.ToLower()))
                        list.Add(itemAll);
                }

                var listSkiplimit = new List<EquipmentUI>();
                if (skip < list.Count)
                {
                    if (limit + skip > list.Count)
                        limit = list.Count - skip;
                    for (int i = skip; i < limit + skip; i++)
                    {
                        listSkiplimit.Add(list[i]);
                    }
                }

                var sqlrM = new ModelRepository(_logger);
                var model = await sqlrM.GetById(modelId);

                var result = new NewEquipmentUIPaging
                {
                    Data = listSkiplimit.ToArray(),
                    Model = model,
                    Total = list.Count
                };
                return result;
            }
        }

        [Obsolete]
        public async Task<EquipmentUIPaging> GetEquipmentWithCheckLists(int modelId, int parentId, int skip, int limit)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                _logger.LogTrace(
                    $"start GetEquipmentWithCheckLists modelId = {modelId}, parentId = {parentId}, skip = {skip}, limit = {limit}");

                List<EquipmentModel> result;
                int count = 0;
                if (parentId == 0)
                {
                    var sql = Sql.SqlQueryCach["Equipment.GetEquipmentModelByModelAndParentNull"];
                    result = (await conn.QueryAsync<EquipmentModel>(
                        sql,
                        new { model_id = modelId, skip = skip, limit = limit, parent_id = parentId })).ToList();

                    var sqlc = Sql.SqlQueryCach["Equipment.CountEquipmentModelByModelAndParentNull"];
                    count = (await conn.QueryAsync<int>(sqlc, new { model_id = modelId })).FirstOrDefault();
                }
                else
                {
                    var sql = Sql.SqlQueryCach["Equipment.GetEquipmentModelByModelAndParent"];
                    result = (await conn.QueryAsync<EquipmentModel>(
                        sql,

                        new { model_id = modelId, skip = skip, limit = limit, parent_id = parentId })).ToList();
                    var sqlc = Sql.SqlQueryCach["Equipment.CountEquipmentModelByModelAndParent"];
                    count = (await conn.QueryAsync<int>(sqlc, new { model_id = modelId, parent_id = parentId }))
                        .FirstOrDefault();
                }

                var list = new List<CheckListEquipmentUI>();
                foreach (var eqm in result)
                {
                    var eqWithCheckLists = await GetCheckListByEquipmentModelId(eqm.Id);
                    eqWithCheckLists.ParentId = eqm.ParentId;
                    eqWithCheckLists.Id = eqm.Id;
                    eqWithCheckLists.ModelId = eqm.ModelId;
                    eqWithCheckLists.IsMark = eqm.IsMark;
                    list.Add(eqWithCheckLists);
                }

                var sqlrM = new ModelRepository(_logger);
                var model = await sqlrM.GetById(modelId);

                var ret = new EquipmentUIPaging
                {
                    Data = list.ToArray(),
                    Model = model,
                    Total = count
                };
                return ret;
            }
        }

        [Obsolete]
        public async Task<EquipmentUIPaging> GetEquipmentWithCheckLists(int modelId, int parentId, int skip, int limit,
            string filter)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var all = await GetEquipmentWithCheckLists(modelId, parentId, 0, Int32.MaxValue);
                var filters = JsonConvert.DeserializeObject<Other.Other.FilterBody[]>(filter);
                var list = new List<CheckListEquipmentUI>();
                foreach (var itemAll in all.Data)
                {

                    if (itemAll.Equipment.Name.ToLower().Contains(filters.FirstOrDefault().Value.ToLower()))
                        list.Add(itemAll);
                }

                var listSkiplimit = new List<CheckListEquipmentUI>();
                if (skip < list.Count)
                {
                    if (limit + skip > list.Count)
                        limit = list.Count - skip;
                    for (int i = skip; i < limit + skip; i++)
                    {
                        listSkiplimit.Add(list[i]);
                    }
                }

                var sqlrM = new ModelRepository(_logger);
                var model = await sqlrM.GetById(modelId);

                var result = new EquipmentUIPaging
                {
                    Data = listSkiplimit.ToArray(),
                    Model = model,
                    Total = list.Count
                };
                return result;
            }
        }

        public async Task<EquipmentUI> GetEquipmentModelById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Equipment.GetCheckListsByEquipmentId"];
                var result = (await conn
                    .QueryAsync<CheckListEquipment, EquipmentModel, Equipment, CheckListEquipment>(
                        sql,
                        (ce, em, equipment) =>
                        {
                            em.Equipment = equipment;
                            ce.EquipmentModel = em;

                            return ce;
                        }, new { equipment_model_id = id })).ToArray();

                var ret = new EquipmentUI();
                if (result.Any())
                {
                    var first = result.FirstOrDefault();
                    ret.Id = first.EquipmentModel.Id;
                    ret.Equipment = first.EquipmentModel.Equipment;
                }
                else
                {
                    ret = null;
                }

                return ret;
            }
        }

        [Obsolete]
        public async Task<CheckListEquipmentUI> GetCheckListByEquipmentModelId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Equipment.GetCheckListsByEquipmentId"];
                var result = (await conn
                    .QueryAsync<CheckListEquipment, EquipmentModel, Equipment, CheckListEquipment>(
                        sql,
                        (ce, em, equipment) =>
                        {
                            em.Equipment = equipment;
                            ce.EquipmentModel = em;

                            return ce;
                        }, new { equipment_model_id = id })).ToArray();

                var algos = new List<Algorithm>();
                var ret = new CheckListEquipmentUI();
                if (result.Any())
                {
                    var first = result.FirstOrDefault();
                    ret.Id = first.EquipmentModel.Id;
                    ret.Equipment = first.EquipmentModel.Equipment;
                    algos.AddRange(result.Select(row => new Algorithm
                    {
                        CheckListType = row.CheckListType,
                        FaultType = row.FaultType,
                        NameTask = row.NameTask,
                        Value = row.Value,
                        ValueType = row.ValueType
                    }));
                    //добавим алгоритмы (нулл), которые не сохранены в бд, но нужны UI
                    foreach (CheckListType cType in Enum.GetValues(typeof(CheckListType)))
                    {
                        if (cType == CheckListType.Surrender)
                            continue;

                        var already = algos.Where(item => item.CheckListType == cType).ToList();
                        if (already.Count != 0) continue;
                        algos.Add(new Algorithm
                        {
                            CheckListType = cType
                        });
                    }

                    ret.Algorithms = algos.ToArray();
                }
                else
                {
                    ret = null;
                }

                return ret;
            }
        }

        public async Task<EquipmentUI> AddOrUpdateEquipment(EquipmentUI ces)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                if (ces.Id == 0)
                {
                    //insert new EquipmentModel, get Id
                    var em = new EquipmentModel
                    {
                        ModelId = ces.ModelId,
                        EquipmentId = ces.Equipment.Id,
                        ParentId = ces.ParentId,
                        IsMark = ces.IsMark

                    };
                    var mr = new ModelRepository(_logger);
                    em = await mr.AddEquipmentToModel(em);
                    ces.Id = em.Id;
                }
                else
                {
                    var mr = new ModelRepository(_logger);
                    await mr.UpdateEquipment(new EquipmentModel
                    {
                        EquipmentId = ces.Equipment.Id,
                        Id = ces.Id,
                        IsMark = ces.IsMark
                    });
                }

                var shit = await GetEquipmentModelById(ces.Id);
                return shit;
            }
        }

        [Obsolete]
        public async Task<CheckListEquipmentUI> AddOrUpdateEquipmentWithCheckLists(CheckListEquipmentUI ces)
        {
            if (ces == null)
            {
                throw new Exception("Не указано наименование");
            }

            //Начинаем блядь КВН
            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var conn = new SqlConnection(AppSettings.ConnectionString))
                {
                    if (ces.Id == 0)
                    {
                        //insert new EquipmentModel, get Id
                        var em = new EquipmentModel
                        {
                            ModelId = ces.ModelId,
                            EquipmentId = ces.Equipment.Id,
                            ParentId = ces.ParentId,
                            IsMark = ces.IsMark

                        };
                        var mr = new ModelRepository(_logger);
                        em = await mr.AddEquipmentToModel(em);
                        ces.Id = em.Id;
                        ces.Algorithms = new List<Algorithm>().ToArray();
                    }
                    else
                    {
                        var mr = new ModelRepository(_logger);
                        await mr.UpdateEquipment(new EquipmentModel
                        {
                            EquipmentId = ces.Equipment.Id,
                            Id = ces.Id,
                            IsMark = ces.IsMark
                        });
                    }

                    //
                    var listAdded = new List<CheckListType>();
                    foreach (var algo in ces.Algorithms)
                    {
                        //if (string.IsNullOrEmpty(algo.NameTask) || algo.FaultType == null)
                        //    throw new Exception("Не заполнены Алгоритмы");
                        if (string.IsNullOrEmpty(algo.NameTask) || algo.Value == null || algo.ValueType == null)
                            continue;

                        await AddOrUpdateCheckListToEquipment(ces.Id, algo);
                        listAdded.Add(algo.CheckListType);
                    }

                    foreach (CheckListType cType in Enum.GetValues(typeof(CheckListType)))
                    {
                        if (cType == CheckListType.Surrender)
                            continue;

                        var already = listAdded.Where(item => item == cType).ToList();
                        if (already.Count == 0)
                        {
                            await DeleteCheckListFromEquipment(new CheckListEquipment
                            {
                                CheckListType = cType,
                                EquipmentModelId = ces.Id
                            });
                            /*var algo = new Algorithm
                            {
                                CheckListType = cType
                            };
                            await AddOrUpdateCheckListToEquipment(ces.Id, algo);*/
                        }
                    }

                    var shit = await GetCheckListByEquipmentModelId(ces.Id);
                    transaction.Complete();
                    return shit;


                }
            }
        }

        //public async Task<CheckListEquipmentUI> AddCheckListsToEquipment(CheckListEquipmentUI ces)
        //{
        //    _db.Transaction.BeginTransaction();
        //    try
        //    {
        //        _DeleteCheckListsFromEquipment(ces.Id);
        //        //
        //        foreach (var ce in ces.Algorithms)
        //        {
        //           await AddCheckListToEquipment(ces.Id, ce);
        //        }
        //        _db.Transaction.CommitTransaction();
        //    }
        //    catch (Exception e)
        //    {
        //        _db.Transaction.RollBackTransaction();
        //        throw e;
        //    }
        //    return ces;
        //}

        private async Task AddOrUpdateCheckListToEquipment(int equipmentModelId, Algorithm ce)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Equipment.AddOrUpdateCheckListToEquipment"];

                var id = await conn.QueryAsync<int>(sql,
                    new
                    {
                        checklist_type = (int)ce.CheckListType,
                        equipment_model_id = equipmentModelId,
                        fault_type = (int)ce.FaultType,
                        name_task = ce.NameTask,
                        value = ce.Value,
                        value_type = ce.ValueType
                    });
            }
        }

        //private async Task<int> AddCheckListToEquipment(int equipmentModelId, Algorithm ce)
        //{
        //    var sql = Sql.SqlQueryCach["Equipment.AddCheckListToEquipment"];
        //    var id = await conn.QueryAsync<int>(sql,
        //        new
        //        {
        //            checklist_type = (int)ce.CheckListType,
        //            equipment_model_id = equipmentModelId,
        //            fault_type = (int)ce.FaultType,
        //            name_task = ce.NameTask,
        //            value = ce.Value,
        //            value_type = (int)ce.ValueType
        //        }, _db.Transaction.Transaction);
        //    return 1;
        //}

        public async Task DeleteCheckListFromEquipment(CheckListEquipment ce)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Equipment.DeleteCheckListFromEquipment"],
                    new { equipment_model_id = ce.EquipmentModelId, checklist_type = (int)ce.CheckListType });
            }
        }

        private async Task _DeleteCheckListsFromEquipment(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Equipment.DeleteCheckListsFromEquipment"],
                    new { equipment_model_id = id });
            }
        }

        public async Task DeleteEquipmentWithCheckLists(CheckListEquipmentUI eq)
        {
            //Начинаем блядь КВН
            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var conn = new SqlConnection(AppSettings.ConnectionString))
                {
                    var mr = new ModelRepository(_logger);

                    //сначала удалим чеклисты
                    await _DeleteCheckListsFromEquipment(eq.Id);
                    //затем связку с моделями
                    await mr.DeleteEquipmentFromModel(eq.Id);
                }

                transaction.Complete();
            }


        }

        public bool IsEquipmentChecklistsEmpty(CheckListEquipmentUI eq)
        {
            bool check = true;
            foreach (var algo in eq.Algorithms)
            {
                if (algo.NameTask != null)
                {
                    check = false;
                }
            }

            return check;
        }        

        public async Task RemoveFaultFromEquipment(FaultEquipment faultEquipment)
        {
            //Начинаем блядь КВН
            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var conn = new SqlConnection(AppSettings.ConnectionString))
                {
                    var sql = Sql.SqlQueryCach["Equipment.RemoveFaultFromEquipment"];
                    await conn.ExecuteAsync(sql,
                        new { faultId = faultEquipment.FaultId, equipmentId = faultEquipment.EquipmentId });
                }
                transaction.Complete();
            }
        }

        public async Task AddFaultToEquipment(FaultEquipment faultEquipment)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Equipment.AddFaultToEquipment"];
                await conn.ExecuteAsync(sql,
                    new { faultId = faultEquipment.FaultId, equipmentId = faultEquipment.EquipmentId });
            }
        }

        public async Task<bool> AddNewFaultToEquipment(string faultName, int equipmentId, int faultType)
        {

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sqlRf = new FaultsRepository(_logger);
                var newFault = await sqlRf.Add(new Fault { Name = faultName, FaultType = (TaskType)faultType }, true);
                var sql = Sql.SqlQueryCach["Equipment.AddFaultToEquipment"];
                await conn.ExecuteAsync(sql, new {faultId = newFault.Id, equipmentId});
                return true;
            }

        }

        public async Task<EquipmentPaging> GetAll(int skip, int limit)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Equipment.All"];
                var result = (await conn.QueryAsync<EquipmentWitcAct, EquipmentCategory, EquipmentWitcAct>(
                    sql,
                    (equipment, category) =>
                    {
                        equipment.Category = category;
                        return equipment;
                    }, new { skip = skip, limit = limit })).ToList();

                var sqlc = Sql.SqlQueryCach["Equipment.CountAll"];
                var count = (await conn.QueryAsync<int>(sqlc)).FirstOrDefault();

                var sqlRAct = new ActCategoriesRepository();
                foreach (var item in result)
                {
                    var act = await sqlRAct.GetByEquipmentId(item.Id);
                    if (act.Count > 0 && act.FirstOrDefault().Id != 0)
                    {
                        item.EquipmentActCategoryDescription = act.FirstOrDefault().Description;
                        item.EquipmentActCategoryName = act.FirstOrDefault().Name;
                        item.EquipmentActCategoryId = act.FirstOrDefault().Id;
                    }
                }

                var output = new EquipmentPaging
                {
                    Data = result.ToArray(),
                    Total = count
                };

                return output;
            }
        }

        public async Task<EquipmentPaging> GetByCategory(EquipmentCategory cat, int skip = 0,
            int limit = Int32.MaxValue, string filter = null)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Equipment.ByCategoryId"];
                var result = await conn.QueryAsync<EquipmentWitcAct, EquipmentCategory, EquipmentWitcAct>(
                    sql,
                    (equipment, category) =>
                    {
                        equipment.Category = category;
                        return equipment;
                    }, new { category_id = cat.Id, skip = skip, limit = limit });
                var sqlc = Sql.SqlQueryCach["Equipment.CountByCategoryId"];
                var count = conn.ExecuteScalar<int>(sqlc, new
                {
                    category_id = cat.Id
                });

                TaskCommon.FilterBody[] filters;
                var filterOutput = new List<EquipmentWitcAct>();
                if (filter != null)
                {
                    filters = JsonConvert.DeserializeObject<TaskCommon.FilterBody[]>(filter);
                    if (filters.Length > 0)
                    {

                        foreach (var value in result)
                        {
                            foreach (var item in filters)
                            {
                                switch (item.Filter)
                                {
                                    case "Name":
                                        if (value.Name.ToLower().Contains(item.Value.ToLower()))
                                            filterOutput.Add(value);
                                        break;
                                }
                            }
                        }
                    }
                }

                if (filter != null)
                {
                    result = filterOutput;
                    count = filterOutput.Count;
                }

                //var newFuckingResult = new List<EquipmentWitcAct>();
                var sqlRAct = new ActCategoriesRepository();
                foreach (var item in result.ToArray())
                {
                    var actCategory = await sqlRAct.GetByEquipmentId(item.Id);
                    if (actCategory.ToArray().Length > 0)
                    {
                        item.EquipmentActCategoryDescription = actCategory.FirstOrDefault().Description;
                        item.EquipmentActCategoryName = actCategory.FirstOrDefault().Name;
                        item.EquipmentActCategoryId = actCategory.FirstOrDefault().Id;
                    }
                }

                var output = new EquipmentPaging
                {
                    Data = result.ToArray(),
                    Total = count
                };

                return output;
            }
        }

        public Equipment GetByParentId(int parentId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Equipment.ParentId"];
                var result = conn.Query<Equipment>(
                    sql, new { id = parentId });
                return result.FirstOrDefault();
            }
        }

        public Equipment GetById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Equipment.ById"];
                var result = conn.Query<Equipment, EquipmentCategory, Equipment>(
                    sql,
                    (equipment, category) =>
                    {
                        equipment.Category = category;
                        return equipment;
                    }, new { equipment_id = id });
                return result.FirstOrDefault();
            }
        }

        public async Task<Equipment> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = new TaskSqls();
                var result = await conn.QueryFirstOrDefaultAsync<Equipment>(_sql.ById(id));
                return result;
            }
        }

        public async Task<EquipmentWitcAct> Update(EquipmentWitcAct input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Equipment.Update"];
                await conn.ExecuteAsync(sql,
                    new
                    {
                        name = input.Name,
                        description = input.Description,
                        categoryId = input.CategoryId,
                        id = input.Id
                    });

                if (input.EquipmentActCategoryId != null)
                {
                    var sqlRAct = new ActCategoriesRepository();
                    var result = await sqlRAct.GetByEquipmentId(input.Id);
                    if (result.Count > 0)
                        await sqlRAct.UpdateEquipmentToAct(input.EquipmentActCategoryId.Value, input.Id);
                    else
                        await sqlRAct.AddEquipmentToAct(input.EquipmentActCategoryId.Value, input.Id);
                    result = await sqlRAct.GetByEquipmentId(input.Id);
                    input.EquipmentActCategoryDescription = result.FirstOrDefault().Description;
                    input.EquipmentActCategoryName = result.FirstOrDefault().Name;
                    input.EquipmentActCategoryId = result.FirstOrDefault().Id;
                }

                return input;
            }
        }


        //public Equipment GetByNameAndDescription(string description, string name)
        //{
        //    var sql = Sql.SqlQueryCach["Equipment.ByNameAndDescription"];
        //    var result = conn.Query<Equipment>(sql, new {name, description});
        //    return result.FirstOrDefault();
        //}

        public async Task<EquipmentWitcAct> Add(EquipmentWitcAct input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sqlc = Sql.SqlQueryCach["Equipment.CountByName"];
                var count = await conn.QueryAsync<int>(sqlc, new { name = input.Name });

                if (count.FirstOrDefault() > 0)
                    throw new Exception("Equipment name already exist");

                var sql = Sql.SqlQueryCach["Equipment.Add"];
                var id = await conn.QueryAsync<int>(sql,
                    new { name = input.Name, description = input.Description, category_id = input.CategoryId });

                //var result = GetByNameAndDescription(equipment.Description, equipment.Name);

                input.Id = id.First();

                if (input.EquipmentActCategoryId != null)
                {
                    var sqlRAct = new ActCategoriesRepository();
                    await sqlRAct.AddEquipmentToAct(input.EquipmentActCategoryId.Value, input.Id);
                    sqlRAct.Dispose();
                }

                return input;
            }

        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Equipment.Delete"], new { id = id });
            }
        }        

        public class EquipmentUIPaging
        {
            public CheckListEquipmentUI[] Data { get; set; }
            public Model.Model Model { get; set; }
            public int Total { get; set; }
        }

        public class NewEquipmentUIPaging
        {
            public EquipmentUI[] Data { get; set; }
            public Model.Model Model { get; set; }
            public int Total { get; set; }
        }

        public class CheckListEquipmentUI
        {
            public int Id { get; set; }
            public int ModelId { get; set; }

            public int? ParentId { get; set; }

            //public string EquipmentName { get; set; }
            //public int EquipmentId { get; set; }
            public Equipment Equipment { get; set; }
            public Algorithm[] Algorithms { get; set; }

            /// <summary>
            /// Признак подлежит маркировке
            /// </summary>
            public bool? IsMark { get; set; }

            /// <summary>
            /// Дочернее оборудование
            /// </summary>
            //public EquipmentUIPaging Childrens { get; set; }
        }

        public class EquipmentUI
        {
            public int Id { get; set; }
            public int ModelId { get; set; }

            public int? ParentId { get; set; }

            public Equipment Equipment { get; set; }

            /// <summary>
            /// Признак подлежит маркировке
            /// </summary>
            public bool? IsMark { get; set; }
        }

        public class Algorithm
        {
            public CheckListType CheckListType { get; set; }
            public TaskType? FaultType { get; set; }
            public string NameTask { get; set; }
            public int? Value { get; set; }
            public CheckListValueType? ValueType { get; set; }
        }

        public class EquipmentPaging
        {
            public EquipmentWitcAct[] Data { get; set; }
            public int Total { get; set; }
        }

        public class EquipmentWitcAct : Equipment
        {
            public string EquipmentActCategoryName { get; set; }
            public string EquipmentActCategoryDescription { get; set; }
            public int? EquipmentActCategoryId { get; set; }

        }
    }

}

