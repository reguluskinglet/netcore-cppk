using Microsoft.Extensions.Caching.Memory;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using Rzdppk.Model.Enums;
using Xunit;

namespace Rzdppk.Core.Tests.Repositoryes
{
    public class ExecutorRepositoryTest
    {
        private readonly IDb _db;
        private readonly IMemoryCache _memoryCache;

        public ExecutorRepositoryTest()
        {
            _db = new Db();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        //[Fact]
        //public async void Get_Counters_ByInspectionId_Success()
        //{
        //    var er = new ExecutorRepository();
        //    var ex = new TrainTaskExecutor
        //    {
        //        BrigadeType = BrigadeType.Depo,
        //        TrainTaskId = 2,
        //        UserId = 1
        //    };
        //    var result = await er.Add(ex);
        //    Assert.True(result != null);
        //    Assert.True(result.Id != 0);
        //}
    }
}
