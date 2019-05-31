//using System;
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
//    public class TrainRepositoryTest
//    {
//        private readonly IDb _db;
//        private readonly IMemoryCache _memoryCache;

//        public TrainRepositoryTest()
//        {
//            _db = new Db();
//            _memoryCache = new MemoryCache(new MemoryCacheOptions());
//        }

//        [Fact]
//        public async void Add_Update_GetAll_GetById_Delete_Success()
//        {
//            var tr = new TrainRepository();
//            var sr = new StantionsRepository();
//            var stantions = await sr.GetAll(0, 50);
//            var stantionId = (from stantion in stantions.Data where stantion.StantionType == StantionType.Depo select stantion.Id).FirstOrDefault();
//            Assert.False(stantionId == 0);
//            var train = new Train
//            {
//                Name = Guid.NewGuid().ToString(),
//                Description = Guid.NewGuid().ToString(),
//                StantionId = stantionId
//            };
//            var train1 = await tr.Add(train);
//            var id_add = train1.Id;
//            var results = await tr.GetAll(0, 10000);
//            var found = false;
//            foreach (var result in results.Data)
//            {
//                if (string.IsNullOrEmpty(result.Name) || string.IsNullOrEmpty(result.StantionId.ToString()))
//                    throw new NullException(result);

//                if (result.Id.Equals(train1.Id) && result.Name.Equals(train.Name) && result.Description.Equals(train.Description) && result.StantionId.Equals(train.StantionId))
//                {
//                    found = true;
//                    break;
//                }
//            }
//            Assert.True(found);
//            //
//            train1.Description += "1";
//            train1.Name += "1";
//            await tr.Update(train1);
//            //
//            results = await tr.GetAll(0, 10000);
//            found = false;
//            foreach (var result in results.Data)
//            {
//                if (string.IsNullOrEmpty(result.Name) || string.IsNullOrEmpty(result.StantionId.ToString()))
//                    throw new NullException(result);

//                if (result.Id.Equals(id_add) && result.Name.Equals(train1.Name) && result.Description.Equals(train1.Description) && result.StantionId.Equals(train1.StantionId))
//                {
//                    found = true;
//                    break;
//                }
//            }
//            Assert.True(found);
//            //
//            var idToDel = train1.Id;
//            await tr.Delete(idToDel);
//            var result1 = tr.ByIdWithStations(idToDel);
//            Assert.True(result1 == null);
//        }
//    }
//}
