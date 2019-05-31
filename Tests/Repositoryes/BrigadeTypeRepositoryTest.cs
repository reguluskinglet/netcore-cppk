using System;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Caching.Memory;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using Xunit;
using Xunit.Sdk;

namespace Rzdppk.Core.Tests.Repositoryes
{
    public class BrigadeTypeRepositoryTest
    {
        private readonly IDb _db;
        private readonly IMemoryCache _memoryCache;

        public BrigadeTypeRepositoryTest()
        {
            _db = new Db();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        //[Fact]
        //public async void Get_GetAll_Success()
        //{
        //    var eq = new BrigadeTypeRepository();
        //    var categories = await eq.GetAll(0, 1);
            
        //    Assert.True(categories.BrigadeType.Length>0);
        //    Assert.False(categories.BrigadeType[0].Description is null);
        //    Assert.False(categories.BrigadeType[0].Name is null);
        //}


        //[Fact]
        //public async void Add_Success()
        //{
        //    var cer = new BrigadeRepository();
        //    var e = new Brigade()
        //    {
        //        Name = Guid.NewGuid().ToString(),
        //        Description = Guid.NewGuid().ToString(),
        //        BrigadeTypeId = 1
        //    };
        //    cer.Add(e);
        //    var results = await cer.GetAll(0, 10000);
        //    foreach (var result in results.Brigades)
        //    {
        //        if (string.IsNullOrEmpty(result.Name) || string.IsNullOrEmpty(result.Description) || result.BrigadeTypeId == 0)
        //            throw new NullException(result);

        //        if (result.Name.Equals(e.Name) && result.Description.Equals(e.Description) && result.Id == e.BrigadeTypeId)
        //        {
        //            Assert.True(true);
        //        }
        //    }
        //}

        //[Fact]
        //public async void Delete_By_Id_Success()
        //{
        //    try
        //    {
        //        var er = new BrigadeRepository();
        //        var results = await er.GetAll(2, 10);
        //        Assert.True(results.Brigades.Length > 0);
        //        var idToDel = results.Brigades[0].Id;
        //        er.Delete(idToDel);
        //        var result = er.ByIdWithStations(idToDel);
        //        if (result != null)
        //        {
        //            throw new Exception("not deleted");
        //        }
        //        Assert.True(true);
        //    }
        //    catch (Exception)
        //    {
        //        Assert.True(false);
        //    }
        //}
    }
}
