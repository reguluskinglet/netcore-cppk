using Microsoft.Extensions.Caching.Memory;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Xunit;

namespace Rzdppk.Core.Tests.Repositoryes
{
    public class InspectionRepositoryTest
    {
        private readonly IDb _db;
        private readonly IMemoryCache _memoryCache;

        public InspectionRepositoryTest()
        {
            _db = new Db();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        //[Fact]
        //public async void Get_Counters_ByInspectionId_Success()
        //{
        //    var tr = new InspectionRepository();
        //    var result = await tr.GetCounters(2);
        //    Assert.True(result != null);
        //}
    }
}
