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
//    public class ReportRepositoryTest
//    {
//        private readonly IDb _db;
//        private readonly IMemoryCache _memoryCache;

//        public ReportRepositoryTest()
//        {
//            _db = new Db();
//            _memoryCache = new MemoryCache(new MemoryCacheOptions());
//        }

//        [Fact]
//        public void GetReports_Success()
//        {
//            var rr = new ReportRepository(_logger);
//            var res = rr.GetList();
//            Assert.True(res.Length > 0);
//        }

//        [Fact]
//        public async void GetAllReports_Success()
//        {
//            var rr = new ReportRepository(_logger);
//            var res = await rr.Get(1, 0, 100);
//            Assert.True(res.Total > 0);
//            Assert.True(res.Columns.Length > 0);
//            Assert.True(res.Columns.Length == res.Rows[0].Values.Length);
//        }
//    }
//}
