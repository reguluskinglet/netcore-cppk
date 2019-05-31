//using System;
//using System.Collections.Generic;
//using Microsoft.EntityFrameworkCore.Storage.Internal;
//using Microsoft.Extensions.Caching.Memory;
//using Rzdppk.Core.Repositoryes;
//using Rzdppk.Core.Repositoryes.Base;
//using Rzdppk.Core.Repositoryes.Base.Interface;
//using Rzdppk.Model.Auth;
//using Xunit;
//using Xunit.Sdk;

//namespace Rzdppk.Core.Tests.Repositoryes
//{
//    public class UserRepositoryTest
//    {
//        private readonly IDb _db;
//        private readonly IMemoryCache _memoryCache;

//        public UserRepositoryTest()
//        {
//            _db = new Db();
//            _memoryCache = new MemoryCache(new MemoryCacheOptions());
//        }


//        [Fact]
//        public async void Get_All_Success()
//        {
//            var cer = new UserRepository();
//            var results = await cer.GetAll(0, 50);
//            Assert.True(results.Data.Length > 0);
//            Assert.True(!string.IsNullOrEmpty(results.Data[0].Name) && !string.IsNullOrEmpty(results.Data[0].PasswordHash));
//        }


//        [Fact]
//        public async void Get_User_By_Login_Success()
//        {
//            var u = new UserRepository();
//            var user = await u.GetUserByLogin("Admin");
            
//            Assert.Equal(user.PasswordHash, "AGKV67cx5v3d4q8I7+avS/nphKdCUB1HS2FGPz6SfLjzb6WN6Au3Db9cKusLouKqxQ==");

//        }

//        [Fact]
//        public async void Get_GetByBrigadeId_Success()
//        {
//            try
//            {
//                var id = 1;
//                var ur = new UserRepository();
//                var br = new BrigadeRepository();
//                var brigade = await br.ByIdWithStations(id);
//                if (brigade == null)
//                    throw new Exception("brigade not exist");

//                var result = await ur.GetStaffByBrigade(brigade);

//                Assert.True(result.Length > 0);
//                Assert.False(result[0].Brigade is null);
//                Assert.False(result[0].Brigade.Name is null);
//                foreach (var item in result)
//                {
//                    Assert.True(item.Brigade.Id == id);
//                }
//            }
//            catch (Exception)
//            {
//                Assert.True(false);
//            }
//        }

//        [Fact]
//        public async void AddToBrigade_Success()
//        {
//            var ur = new UserRepository();
//            var br = new BrigadeRepository();

//            var user = await ur.GetStaffById(2);
//            if (user == null)
//                throw new Exception("user not exist");

//            var brigade = br.GetAll(0, 5).Result.Data[0];

//            await ur.AddStaffToBrigade(user, brigade);

//            var results = await ur.GetStaffByBrigade(brigade);

//            var found = false;
//            foreach (var result in results)
//            {
//                if (string.IsNullOrEmpty(result.Name) || result.BrigadeId == null || result.BrigadeId == 0)
//                    throw new NullException(result);

//                if (result.Name.Equals(user.Name) && result.BrigadeId.Equals(brigade.Id))
//                {
//                    found = true;
//                    break;
//                }
//            }
//            Assert.True(found);
//        }

//        [Fact]
//        public async void DeleteFromBrigade_Success()
//        {
//            var ur = new UserRepository();

//            var user = await ur.GetStaffById(2);

//            Assert.NotNull(user);
//            Assert.NotNull(user.BrigadeId);

//            var brigade = user.Brigade;
//            await ur.DeleteStaffFromBrigade(user);

//            var results = await ur.GetStaffByBrigade(brigade);
//            foreach (var result in results)
//            {
//                Assert.False(result.BrigadeId == brigade.Id);
//            }
//            Assert.True(true);
//        }

//        [Fact]
//        public async void AddUser_Success()
//        {
//            var sqlR = new UserRepository();
//            var all = await sqlR.GetAll(0, 10000);
//            var user = new User
//            {
//                Login = Guid.NewGuid().ToString(),
//                Name = "TestName",
//                IsBlocked = false,
//                PasswordHash = "asfasfassfas",
//                RoleId = 1
//            };
//            await sqlR.AddOrUpdate(user);
//            var allAfterAdd = await sqlR.GetAll(0, 10000);
//            Assert.True(all.Data.Length < allAfterAdd.Data.Length);

//        }

//        [Fact]
//        public async void UpdateUser_Success()
//        {
//            var sqlR = new UserRepository();
//            var user = new User
//            {
//                Login = Guid.NewGuid().ToString(),
//                Name = "TestName",
//                IsBlocked = false,
//                PasswordHash = "asfasfassfas",
//                RoleId = 1
//            };
//            var id =  await sqlR.AddOrUpdate(user);

//            user.Id = id;
//            user.Name = Guid.NewGuid().ToString();
//            sqlR.Update(user);

//            var newUser = await sqlR.ById(id);
//            Assert.True(newUser.Name == user.Name);

//        }


//    }
//}
