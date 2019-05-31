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
//    public class BrigadeRepositoryTest
//    {
//        private readonly IDb _db;
//        private readonly IMemoryCache _memoryCache;

//        public BrigadeRepositoryTest()
//        {
//            _db = new Db();
//            _memoryCache = new MemoryCache(new MemoryCacheOptions());
//        }

//        //[Fact]
//        //public async void Get_GetAll_Success()
//        //{
//        //    var eq = new BrigadeRepository(_);
//        //    var categories = await eq.GetAll(0, 2);
            
//        //    Assert.True(categories.Data.Length>0);
//        //    //Assert.False(categories.Data[0].BrigadeType is null);
//        //    //Assert.False(categories.Data[0].BrigadeType.Name is null);
//        //}


//        [Fact]
//        public async void Add_Success()
//        {
//            var cer = new BrigadeRepository();
//            var e = new Brigade()
//            {
//                Name = Guid.NewGuid().ToString(),
//                Description = Guid.NewGuid().ToString(),
//                BrigadeType = BrigadeType.Depo
//            };
//            await cer.Add(e);
//            var results = await cer.GetAll(0, 10000);
//            foreach (var result in results.Data)
//            {
//                if (string.IsNullOrEmpty(result.Name) || string.IsNullOrEmpty(result.Description) || result.BrigadeType == BrigadeType.Depo)
//                    throw new NullException(result);

//                if (result.Name.Equals(e.Name) && result.Description.Equals(e.Description) && result.BrigadeType == e.BrigadeType)
//                {
//                    Assert.True(true);
//                }
//            }
//        }

//        [Fact]
//        public async void Delete_By_Id_Success()
//        {
//            try
//            {
//                var er = new BrigadeRepository();
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
