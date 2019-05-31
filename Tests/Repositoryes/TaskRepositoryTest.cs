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
//    public class TaskRepositoryTest
//    {
//        private readonly IDb _db;
//        private readonly IMemoryCache _memoryCache;

//        public TaskRepositoryTest()
//        {
//            _db = new Db();
//            _memoryCache = new MemoryCache(new MemoryCacheOptions());
//        }

//        //[Fact]
//        //public void GetHistoryByTaskId_Success()
//        //{
//        //    int id = 1;
//        //    var tr = new TaskRepository();
//        //    var records = tr.AddHistoryData(1);
//        //    Assert.True(records.Length > 0);
//        //}

//        [Fact]
//        public async void GetAll_Success()
//        {
//            var tr = new TaskRepository();
//            var results = await tr.GetAll(0, 10000, null);
//            Assert.True(results.Total > 0);
//            Assert.True(results.Data.Length > 0);
//        }

//        [Fact]
//        public async void GetAllForPdf_Success()
//        {
//            var tr = new TaskRepository();
//            var results = await tr.GetInspectionTasksForPdf(2);
//            Assert.True(results.Length > 0);
//        }
//    }
//}
