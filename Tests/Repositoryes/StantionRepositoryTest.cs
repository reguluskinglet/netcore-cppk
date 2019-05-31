//using System;
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
//    public class StantionRepositoryTest
//    {
//        private readonly IDb _db;
//        private readonly IMemoryCache _memoryCache;

//        public StantionRepositoryTest()
//        {
//            _db = new Db();
//            _memoryCache = new MemoryCache(new MemoryCacheOptions());
//        }

//        [Fact]
//        public async void Add_Success()
//        {
//            var sr = new StantionRepository(_lo);
//            var st = new Stantion
//            {
//                Name = Guid.NewGuid().ToString(),
//                Description = Guid.NewGuid().ToString(),
//                StantionType = StantionType.RailwayStation
//            };
//            await sr.Add(st);
//            var results = await sr.GetAll(0, 10000);
//            var found = false;
//            foreach (var result in results.Data)
//            {
//                if (string.IsNullOrEmpty(result.Name) || string.IsNullOrEmpty(result.StantionType.ToString()))
//                    throw new NullException(result);

//                if (result.Name.Equals(st.Name) && result.Description.Equals(st.Description) && result.StantionType.Equals(st.StantionType))
//                {
//                    found = true;
//                    break;
//                }
//            }
//            Assert.True(found);
//        }

//        [Fact]
//        public async void Get_All_Success()
//        {
//            var sr = new StantionRepository();
//            var results = await sr.GetAll(0, 50);
//            Assert.True(results.Data.Length > 0);
//            Assert.True(!string.IsNullOrEmpty(results.Data[0].Name) && !string.IsNullOrEmpty(results.Data[0].StantionType.ToString()));
//        }

//        [Fact]
//        public async void Delete_By_Id_Success()
//        {
//            try
//            {
//                var sr = new StantionRepository();
//                var results = await sr.GetAll(2, 10);
//                Assert.True(results.Data.Length > 0);
//                var idToDel = results.Data[results.Data.Length-1].Id;
//                await sr.Delete(idToDel);
//                var result = sr.ById(idToDel);
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
