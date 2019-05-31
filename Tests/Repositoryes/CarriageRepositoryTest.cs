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
//    public class CarriageRepositoryTest
//    {
//        private readonly IDb _db;
//        private readonly IMemoryCache _memoryCache;

//        public CarriageRepositoryTest()
//        {
//            _db = new Db();
//            _memoryCache = new MemoryCache(new MemoryCacheOptions());
//        }

//        //[Fact]
//        //public async void GetByIdWithEquipment_Success()
//        //{
//        //    var id = 1;
//        //    var cr = new CarriageRepository();
//        //    var ceq = await cr.GetByIdWithEquipment(id);
//        //    Assert.True(ceq.Equipment != null);
//        //    Assert.True(ceq.Equipment.Length > 0);
//        //    Assert.True(ceq.Carriage != null);
//        //}

//        [Fact]
//        public async void Add_Update_GetByTrain_GetById_Delete_Success()
//        {
//            var tr = new TrainRepository();
//            var cr = new CarriageRepository(_logger);
//            var mr = new ModelRepository();

//            var models = await mr.GetAll(0, 10);
//            var trains = await tr.GetAll(0, 10);

//            var model = models.Data[0];
//            var train = trains.Data[0];

//            var rnd = new Random(0);
//            var carriage = new Carriage
//            {
//                Number = rnd.Next(10),
//                Serial = Guid.NewGuid().ToString(),
//                ModelId = model.Id,
//                TrainId = train.Id
//            };

//            var carriage1 = await cr.Add(carriage);
//            var id_add = carriage1.Id;

//            var results = await cr.GetByTrain(train);
//            var found = false;
//            foreach (var result in results)
//            {
//                if (result.Id.Equals(id_add) && result.Number.Equals(carriage.Number) && result.Serial.Equals(carriage.Serial) && result.ModelId.Equals(carriage.ModelId) && result.TrainId.Equals(carriage.TrainId))
//                {
//                    found = true;
//                    break;
//                }
//            }
//            Assert.True(found);
//            //
//            carriage1.Serial += "1";
//            await cr.Update(carriage1);
//            //
//            results = await cr.GetByTrain(train);
//            found = false;
//            foreach (var result in results)
//            {
//                if (result.Id.Equals(id_add) && result.Number.Equals(carriage.Number) && result.Serial.Equals(carriage.Serial) && result.ModelId.Equals(carriage.ModelId) && result.TrainId.Equals(carriage.TrainId))
//                {
//                    found = true;
//                    break;
//                }
//            }
//            Assert.True(found);
//            //
//            var idToDel = carriage1.Id;
//            await cr.Delete(idToDel);
//            var result1 = cr.ByIdWithStations(idToDel);
//            Assert.True(result1 == null);
//        }
//    }
//}
