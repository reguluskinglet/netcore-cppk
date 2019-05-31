using System;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Caching.Memory;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using Rzdppk.Model.Enums;
using Xunit;
using Xunit.Sdk;

namespace Rzdppk.Core.Tests
{
    public class AddSomeShitToDb
    {
        private readonly IDb _db;
        private readonly IMemoryCache _memoryCache;

        public AddSomeShitToDb()
        {
            _db = new Db();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }


        /// <summary>
        /// Добавляет немного говнеца в базу. Не советую его юзать после остальных тестов. т.к. ид внем захардкодены
        /// </summary>
        /*[Fact]
        public void Add_Shit()
        {
            Random rnd = new Random();
            try
            {

                var cer = new CategoryEquipmentRepository();
                for (int i = 0; i < 10; i++)
                {
                    var ec = new EquipmentCategory
                    {
                        Name = Guid.NewGuid().ToString(),
                        Description = Guid.NewGuid().ToString()
                    };
                    cer.Add(ec);
                }

                var er = new EquipmentRepository();
                for (int i = 1; i < 10; i++)
                { 
                    var e = new Equipment
                    {
                        Name = Guid.NewGuid().ToString(),
                        Description = Guid.NewGuid().ToString(),
                        CategoryId = i
                    };
                    er.Add(e);
                }

                var br = new BrigadeRepository();
                for (int i = 1; i < 10; i++)
                {
                    var e = new Brigade()
                    {
                        Name = Guid.NewGuid().ToString(),
                        Description = Guid.NewGuid().ToString(),
                        BrigadeType = BrigadeType.Depo
                    };
                    br.Add(e);
                }



                Assert.True(true);
            }
            catch (Exception)
            {
                Assert.True(false);
            }


        }*/



    }
}
