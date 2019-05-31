//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.EntityFrameworkCore.Storage.Internal;
//using Microsoft.Extensions.Caching.Memory;
//using Rzdppk.Core.Repositoryes;
//using Rzdppk.Core.Repositoryes.Base;
//using Rzdppk.Core.Repositoryes.Base.Interface;
//using Rzdppk.Model;
//using Rzdppk.Model.Enums;
//using Xunit;
//using Xunit.Sdk;

//namespace Rzdppk.Core.Tests.Repositoryes
//{
//    public class EquipmentRepositoryTest
//    {
//        private readonly IDb _db;
//        private readonly IMemoryCache _memoryCache;

//        public EquipmentRepositoryTest()
//        {
//            _db = new Db();
//            _memoryCache = new MemoryCache(new MemoryCacheOptions());
//        }

//        //[Fact]
//        //public async void GetEquipmentWithCheckLists_Success()
//        //{
//        //    var er = new EquipmentRepository(_logger);
//        //    var modelId = 1;
//        //    var parentId = 1;
//        //    var res = await er.GetEquipmentWithCheckLists(modelId, parentId, 0, 10);
//        //    Assert.True(res.Data.Length > 0);
//        //    var ceq = res.Data[1];
//        //    ceq.Algorithms[0].NameTask = "update";
//        //    var algos = ceq.Algorithms.ToList();
//        //    var algo = new EquipmentRepository.Algorithm
//        //    {
//        //        CheckListType = CheckListType.TO2,
//        //        FaultType = TaskType.Technical,
//        //        NameTask = "insert task",
//        //        Value = 1,
//        //        ValueType = CheckListValueType.Bool
//        //    };
//        //    algos.Add(algo);
//        //    ceq.Algorithms = algos.ToArray();
//        //    var res1 = await er.AddOrUpdateEquipmentWithCheckLists(ceq);
//        //}

//        //[Fact]
//        //public async void Add_Get_Delete_Checklist_Success()
//        //{
//        //    var er = new EquipmentRepository();
//        //    var EquipmentModelId = 3;
//        //    var ce = new EquipmentRepository.CheckListEquipmentUI
//        //    {
//        //        Id = EquipmentModelId,
//        //        Algorithms = new []
//        //        {
//        //            new EquipmentRepository.Algorithm {CheckListType = (CheckListType) 0, FaultType = (TaskType) 1 , NameTask = "task1", Value = 10, ValueType = 0},
//        //            new EquipmentRepository.Algorithm {CheckListType = (CheckListType) 1, FaultType = (TaskType) 1 , NameTask = "task1", Value = 1, ValueType = 0}
//        //        }
//        //    };
//        //    var res = await er.AddCheckListsToEquipment(ce);
//        //    //
//        //    var ce1 = await er.GetCheckListByEquipmentModelId(EquipmentModelId);
//        //    Assert.True(ce1 != null);
//        //    Assert.True(ce1.Algorithms.Length > 0);
//        //    Assert.True(ce1.Algorithms[0].NameTask != null);
//        //    //
//        //    foreach (var algo in ce.Algorithms)
//        //    {
//        //        var c = new CheckListEquipment
//        //        {
//        //            EquipmentModelId = ce.Id,
//        //            CheckListType = algo.CheckListType
//        //        };
//        //        er.DeleteCheckListFromEquipment(c);
//        //    }
//        //    var ce2 = await er.GetCheckListByEquipmentModelId(EquipmentModelId);
//        //    Assert.True(ce2 == null);
//        //}

//        [Fact]
//        public async void Get_GetAll_Success()
//        {
//            var sqlr = new EquipmentRepository();
//            var result = await sqlr.GetAll(0, 2);
            
//            Assert.True(result.Data.Length>0);
//            Assert.False(result.Data[0].Category is null);
//            Assert.False(result.Data[0].Category.Name is null);
//        }

//        /*[Fact]
//        public async void Get_GetByCategoryId_Success()
//        {
//            try
//            {
//                EquipmentRepository.EquipmentPaging result;
//                var id = 1;
//                var er = new EquipmentRepository();
//                var cer = new CategoryEquipmentRepository();
//                var category = cer.ByIdWithStations(id);
//                if (category != null)
//                {
//                    result = await er.GetByCategory(category, 0, 10, null);
//                }
//                else
//                {
//                    throw new Exception("category_id not exist");
//                }
//                Assert.True(result.Data.Length > 0);
//                Assert.False(result.Data[0].Category is null);
//                Assert.False(result.Data[0].Category.Name is null);
//                foreach (var item in result.Data)
//                {
//                    Assert.True(item.CategoryId==id);
//                }
//            } catch (Exception)
//            {
//                Assert.True(false);
//            }
//        }

//        [Fact]
//        public async void Add_Success()
//        {
//            var cer = new EquipmentRepository();
//            var e = new Equipment
//            {
//                Name = Guid.NewGuid().ToString(),
//                Description = Guid.NewGuid().ToString(),
//                CategoryId = 1
//            };
//            cer.Add(e);
//            var results = await cer.GetAll(0, 10000);
//            foreach (var result in results.Data)
//            {
//                if (string.IsNullOrEmpty(result.Name) || string.IsNullOrEmpty(result.Description) || result.CategoryId == 0)
//                    throw new NullException(result);

//                if (result.Name.Equals(e.Name) && result.Description.Equals(e.Description) && result.Id == e.CategoryId)
//                {
//                    Assert.True(true);
//                }
//            }
//        }

//        [Fact]
//        public async void Double_Name_Add_Check_Success()
//        {
//            var cer = new EquipmentRepository();
//            var e = new Equipment
//            {
//                Name = Guid.NewGuid().ToString(),
//                Description = Guid.NewGuid().ToString(),
//                CategoryId = 1
//            };
//            cer.Add(e);
//            e.Id = 0;
//            try
//            {
//                cer.Add(e);
//                Assert.True(false);
//            }
//            catch (Exception)
//            {
//                Assert.True(true);
//            }
//        }*/

//        [Fact]
//        public async void Delete_By_Id_Success()
//        {
//            try
//            {
//                var er = new EquipmentRepository();
//                var results = await er.GetAll(2, 10);
//                Assert.True(results.Data.Length > 0);
//                var idToDel = results.Data[0].Id;
//                await er.Delete(idToDel);
//                var result = er.ByIdWithStations(idToDel);
//                if (result != null)
//                {
//                    throw new Exception("not deleted");
//                }
//                Assert.True(true);
//            }
//            catch (Exception)
//            {
//                Assert.True(false);
//            }
//        }
//    }
//}
