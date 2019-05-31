using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Caching.Memory;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Xunit;
using Xunit.Sdk;

namespace Rzdppk.Core.Tests.Repositoryes
{
    public class UserRoleTest
    {
        private readonly IDb _db;
        private readonly IMemoryCache _memoryCache;

        public UserRoleTest()
        {
            _db = new Db();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        [Fact]
        public async void Get_GetAll_Success()
        {
            var sqlR = new UserRoleRepository();
            var result = await sqlR.GetAll(0, 100);

            Assert.True(result.Data.Length > 0);
            Assert.False(result.Data[0].Role.Name is null);
            Assert.False(result.Data[0].Role.Permissions == 0);
        }


        [Fact]
        public async void Add_Success()
        {
            var sqlR = new UserRoleRepository();
            var e = new UserRole
            {

                Name = Guid.NewGuid().ToString(),
                Permissions = 123
                
            };
            var id = await sqlR.Add(new UserRoleRepository.UserRoleUi{Role = e});
            var results = await sqlR.GetAll(0, 10000);
            foreach (var result in results.Data)
            {
                if (string.IsNullOrEmpty(result.Role.Name) ||  result.Role.Permissions == 0)
                    throw new NullException(result);

                if (result.Role.Name.Equals(e.Name) && result.Role.Id == id)
                {
                    Assert.True(true);
                }
            }
        }

        [Fact]
        public async void Update_Success()
        {
            var sqlR = new UserRoleRepository();
            var results = await sqlR.GetAll(0, 10000);
            var itemToUpdate = results.Data.LastOrDefault();
            itemToUpdate.Role.Name = Guid.NewGuid().ToString();

            await sqlR.AddUpdateUserRole(itemToUpdate);

            var itemUpdated = sqlR.GetById(itemToUpdate.Role.Id);

            Assert.True(itemToUpdate.Role.Name == itemUpdated.Name);
        }


        [Fact]
        public async void Del_Success()
        {
            var sqlR = new UserRoleRepository();
            var e = new UserRole
            {

                Name = Guid.NewGuid().ToString(),
                Permissions = 123

            };
            var id = await sqlR.Add(new UserRoleRepository.UserRoleUi{Role = e});
            var results = await sqlR.GetAll(0, 10000);
            await sqlR.Delete(results.Data.LastOrDefault().Role.Id);
            var resultsAfterDel = await sqlR.GetAll(0, 10000);
            Assert.True(results.Data.ToArray().Length > resultsAfterDel.Data.ToArray().Length);
        }


        [Fact]
        public void Get_Int_Permissions()
        {
            var sqlR = new UserRoleRepository();
            var mass = new Dictionary<int, int>();
            for (int i = 0; i < 32; i++)
            {
                if (i == 0)
                {
                    mass.Add(i, 1);
                    continue;
                }
                mass.Add(i, 0);
            }

            var res = sqlR.ConvertPermissionsToInt(mass);
            Assert.True(false);
        }

        [Fact]
        public void Get_Int_Permissions_all_bit_1()
        {
            var sqlR = new UserRoleRepository();
            var mass = new Dictionary<int, int>();
            for (int i = 0; i < 32; i++)
            {
                mass.Add(i, 1);
            }

            var res = sqlR.ConvertPermissionsToInt(mass);
            Assert.True(false);
        }



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
