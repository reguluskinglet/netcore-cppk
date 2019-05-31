//using System;
//using System.Linq;
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
//    public class ModelRepositoryTest
//    {
//        private readonly IDb _db;
//        private readonly IMemoryCache _memoryCache;

//        public ModelRepositoryTest()
//        {
//            _db = new Db();
//            _memoryCache = new MemoryCache(new MemoryCacheOptions());
//        }

//        [Fact]
//        public async void Add_Success()
//        {
//            try
//            {
//                var mr = new ModelRepository();
//                var m = new Model.Model()
//                {
//                    Name = Guid.NewGuid().ToString(),
//                    Description = Guid.NewGuid().ToString(),
//                    ModelType = ModelType.HeadVagon
//                };
//                var m1 = mr.Add(m);
//                var results = await mr.GetAll(0, 10000);
//                bool found = false;
//                foreach (var result in results.Data)
//                {
//                    if (string.IsNullOrEmpty(result.Name) || string.IsNullOrEmpty(result.ModelType.ToString()))
//                        throw new NullException(result);

//                    if (result.Name.Equals(m.Name) && result.Description.Equals(m.Description) && result.ModelType.Equals(m.ModelType))
//                    {
//                        found = true;
//                        break;
//                    }
//                }
//                if (!found)
//                {
//                    throw new Exception("added model not found");
//                }
//            }
//            catch (Exception)
//            {
//                Assert.True(false);
//            }
//        }

//        [Fact]
//        public async void Get_All_Success()
//        {
//            var mr = new ModelRepository();
//            var results = await mr.GetAll(0, 50);
//            Assert.True(results.Data.Length > 0);
//            Assert.True(!string.IsNullOrEmpty(results.Data[0].Name) && !string.IsNullOrEmpty(results.Data[0].ModelType.ToString()));
//        }

//        [Fact]
//        public async void Delete_By_Id_Success()
//        {
//            try
//            {
//                var mr = new ModelRepository();
//                var results = await mr.GetAll(2, 10);
//                Assert.True(results.Data.Length > 0);
//                var idToDel = results.Data[0].Id;
//                await mr.Delete(idToDel);
//                var result = mr.ByIdWithStations(idToDel);
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

//        [Fact]
//        public async void Get_Equipment_By_Id_Success()
//        {
//            var mr = new ModelRepository();
//            var m = await mr.ByIdWithStations(1);
//            var results = await mr.GetEquipmentByModel(m);
//            Assert.True(results.Length > 0);
//            Assert.False(results[0].Equipment == null);
//        }

//        [Fact]
//        public async void Add_Equipment_To_Model_Success()
//        {
//            try
//            {
//                var mr = new ModelRepository();
//                var er = new EquipmentRepository();
//                var m = await mr.ByIdWithStations(1);
//                var e = er.ByIdWithStations(1);
//                var em = new EquipmentModel
//                {
//                    ModelId = m.Id,
//                    EquipmentId = 1,
//                    ParentId = 0
//                };
//                var em1 = mr.AddEquipmentToModel(em);
//                var results = await mr.GetEquipmentByModel(m);
//                bool found = false;
//                foreach (var result in results)
//                {
//                    if (result.Equipment == null)
//                        throw new NullException(result);

//                    if (result.Equipment.Id.Equals(e.Id) && result.Equipment.Name.Equals(e.Name) && result.ModelId.Equals(m.Id))
//                    {
//                        found = true;
//                        break;
//                    }
//                }
//                if (!found)
//                {
//                    throw new Exception("added record not found");
//                }
//                Assert.True(true);
//            }
//            catch (Exception)
//            {
//                Assert.True(false);
//            }
//        }

//        [Fact]
//        public async void Delete_Equipment_From_Model_By_Id_Success()
//        {
//            var mr = new ModelRepository();
//            var m = await mr.ByIdWithStations(1);
//            var results = await mr.GetEquipmentByModel(m);
//            Assert.True(results.Length > 0);
//            var idToDel = results[results.Length-1].Id;
//            await mr.DeleteEquipmentFromModel(idToDel);
//            var results1 = await mr.GetEquipmentByModel(m);
//            foreach (var result in results1)
//            {
//                Assert.False(result.Id.Equals(idToDel));
//            }
//        }
//    }
//}
