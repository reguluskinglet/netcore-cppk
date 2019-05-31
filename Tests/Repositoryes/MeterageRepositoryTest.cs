using Microsoft.Extensions.Caching.Memory;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Xunit;

namespace Rzdppk.Core.Tests.Repositoryes
{
    public class MeterageRepositoryTest
    {
        private readonly IDb _db;
        private readonly IMemoryCache _memoryCache;

        public MeterageRepositoryTest()
        {
            _db = new Db();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        //[Fact]
        //public async void Get_Labels_ByInspectionId_Success()
        //{
        //    var tr = new MeterageRepository();
        //    var results = await tr.GetLabels(2);
        //    Assert.True(results.Length > 0);
        //    Assert.True(results[0].Label != null);
        //}

        //[Fact]
        //public async void Get_Meterages_ByInspectionId_Success()
        //{
        //    var tr = new MeterageRepository();
        //    var results = await tr.GetMeterages(2);
        //    Assert.True(results.Length > 0);
        //}
    }
}
