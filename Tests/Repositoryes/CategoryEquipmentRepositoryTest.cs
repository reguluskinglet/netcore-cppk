//using System;
//using Microsoft.EntityFrameworkCore.Storage.Internal;
//using Microsoft.Extensions.Caching.Memory;
//using Rzdppk.Core.Repositoryes;
//using Rzdppk.Core.Repositoryes.Base;
//using Rzdppk.Core.Repositoryes.Base.Interface;
//using Rzdppk.Model;
//using Xunit;
//using Xunit.Sdk;

//namespace Rzdppk.Core.Tests.Repositoryes
//{
//    public class CategoryEquipmentRepositoryTest
//    {
//        private readonly IDb _db;
//        private readonly IMemoryCache _memoryCache;

//        public CategoryEquipmentRepositoryTest()
//        {
//            _db = new Db();
//            _memoryCache = new MemoryCache(new MemoryCacheOptions());
//        }

//        [Fact]
//        public async void Add_Success()
//        {
//            var cer = new CategoryEquipmentRepository(_logger);
//            var ec = new EquipmentCategory
//            {
//                Name = Guid.NewGuid().ToString(),
//                Description = Guid.NewGuid().ToString()
//            };
//            await cer.Add(ec);
//            var results = await cer.GetAll(0, 10000);
//            foreach (var result in results.Data)
//            {
//                if (string.IsNullOrEmpty(result.Name) || string.IsNullOrEmpty(result.Description))
//                    throw new NullException(result);

//                if (result.Name.Equals(ec.Name) && result.Description.Equals(ec.Description))
//                {
//                    Assert.True(true);
//                }
//            }

//        }

//        [Fact]
//        public async void Get_All_Success()
//        {
//            var cer = new CategoryEquipmentRepository();
//            var results = await cer.GetAll(0, 50);
//            Assert.True(results.Data.Count > 0);
//            Assert.True(!string.IsNullOrEmpty(results.Data[0].Name) && !string.IsNullOrEmpty(results.Data[0].Description));
//        }

//        [Fact]
//        public async void Delete_By_Id_Success()
//        {
//            try
//            {
//                var cer = new CategoryEquipmentRepository();
//                var results = await cer.GetAll(2, 10);
//                Assert.True(results.Data.Count > 0);
//                var idToDel = results.Data[0].Id;
//                await cer.Delete(idToDel);
//                var result = cer.ByIdWithStations(idToDel);
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
