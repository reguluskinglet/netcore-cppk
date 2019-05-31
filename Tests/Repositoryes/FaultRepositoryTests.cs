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
//    public class FaultRepositoryTests
//    {
//        private readonly IDb _db;
//        private readonly IMemoryCache _memoryCache;

//        public FaultRepositoryTests()
//        {
//            _db = new Db();
//            _memoryCache = new MemoryCache(new MemoryCacheOptions());
//        }

//        [Fact]
//        public async void Add_Success()
//        {
//            var sqlr = new FaultsRepository();
//            var obj = new Fault()
//            {
//                Name = Guid.NewGuid().ToString(),
//                Description = Guid.NewGuid().ToString()
//            };
//            await sqlr.Add(obj);
//            var results = await sqlr.GetAll(0, 10000);
//            foreach (var result in results.Data)
//            {
//                if (string.IsNullOrEmpty(result.Name) || string.IsNullOrEmpty(obj.Description))
//                    throw new NullException(result);

//                if (result.Name.Equals(result.Name) && result.Description.Equals(obj.Description))
//                {
//                    Assert.True(true);
//                }
//            }

//        }

//        [Fact]
//        public async void Get_All_Success()
//        {
//            var cer = new FaultsRepository();
//            var results = await cer.GetAll(0, 50);
//            Assert.True(results.Data.Length > 0);
//            Assert.True(!string.IsNullOrEmpty(results.Data[0].Name) && !string.IsNullOrEmpty(results.Data[0].Description));
//        }


//        [Fact]
//        public async void Get_GetByEquipmentId_Success()
//        {
//            var cer = new FaultsRepository();
//            var results = await cer.GetByEquipmentId(10);
//            Assert.True(results.Length > 0);
//            Assert.True(!string.IsNullOrEmpty(results[0].Name) && results[0].Id != 0);
//        }

//        [Fact]
//        public async void Delete_By_Id_Success()
//        {
//            try
//            {
//                var cer = new FaultsRepository();
//                var results = await cer.GetAll(2, 10);
//                Assert.True(results.Data.Length > 0);
//                var idToDel = results.Data[0].Id;
//                await cer.Delete(idToDel);
//                var result = cer.ById(idToDel);
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
