using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Core.Helpers;
using Rzdppk.Core.Options;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Dto;

namespace Rzdppk.Core.Repositoryes
{
    public class UserRepository : IUserRepository, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IDb _db;

        public UserRepository(IDb db)
        {
            _db = db;
        }

        public UserRepository(ILogger logger)
        {
            _db = new Db();
            _logger = logger;
        }

        public UserRepository()
        {
            _db = new Db();
        }

        public async Task AddStaff(User user)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var roleId = 1;
                var isBlocked = 1;
                var sql = Sql.SqlQueryCach["Users.AddStaff"];
                await conn.ExecuteAsync(sql, new
                {
                    login = user.Login,
                    name = user.Name,
                    personNumber = user.PersonNumber,
                    personPosition = user.PersonPosition,
                    roleId = roleId,
                    isBlocked = isBlocked,
                    brigadeId = user.BrigadeId
                });
            }
        }

        public async Task<User> AddOrUpdate(User user)
        {

            if (string.IsNullOrWhiteSpace(user.Login))
                throw new ValidationException(Error.NotFilledOptionalField);

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                if (user.Id <= 0)
                {
                    var sqlRaw = $"select * from [auth_users] where [Login] = '{user.Login}'";
                    var validResult = await conn.QueryAsync(sqlRaw);
                    if (validResult.ToList().Any())
                        throw new Microsoft.Rest.ValidationException(Error.AlreadyAddWithThisName);
                }
                else
                {
                    var byId = await ById(user.Id);
                    if (byId.Login != user.Login)
                    {
                        var sqlRaw = $"select * from [auth_users] where [Login] = '{user.Login}'";
                        var validResult = await conn.QueryAsync(sqlRaw);
                        if (validResult.ToList().Any())
                            throw new Microsoft.Rest.ValidationException(Error.AlreadyAddWithThisName);
                    }
                }

                if (user.RoleId == 0)
                    throw new ValidationException(Error.NotFilledOptionalField);

                var sql = Sql.SqlQueryCach["Users.Add"];
                if (user.Id > 0)
                {
                    await Update(user);
                }
                else
                {
                    if (user.PasswordHash != null)
                        user.PasswordHash = CryptoHelper.HashPassword(user.PasswordHash);

                    var old = await ByPersonNumber(user.PersonNumber);
                    if (old != null)
                    {
                        throw new Exception("Табельный номер уже занят");
                    }

                    var userAfterAdd = (await conn.QueryFirstOrDefaultAsync<User>(sql, new
                    {
                        brigadeId = user.BrigadeId,
                        isBlocked = user.IsBlocked,
                        login = user.Login,
                        name = user.Name,
                        passwordHash = user.PasswordHash,
                        personNumber = user.PersonNumber,
                        personPosition = user.PersonPosition,
                        roleid = user.RoleId
                    }));
                    return userAfterAdd;
                }
                return user;
            }

        }

        public async Task Update(User user)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var currentUser = await ById(user.Id);
                if (user.BrigadeId == null) user.BrigadeId = currentUser.BrigadeId;
                if (user.PersonNumber == null) user.PersonNumber = currentUser.PersonNumber;
                if (user.PersonPosition == null) user.PersonPosition = currentUser.PersonPosition;
                if (user.Name == null) user.Name = currentUser.Name;

                var sql = Sql.SqlQueryCach["Users.Update"];
                if (user.PasswordHash != null)
                {
                    user.PasswordHash = CryptoHelper.HashPassword(user.PasswordHash);
                    sql = Sql.SqlQueryCach["Users.UpdateWithPassword"];
                }

                var old = await ByPersonNumber(user.PersonNumber);
                if (old != null && old.Id != user.Id)
                {
                    throw new Exception("Табельный номер уже занят");
                }

                await conn.ExecuteAsync(sql, new
                {
                    brigadeId = user.BrigadeId,
                    isBlocked = user.IsBlocked,
                    login = user.Login,
                    name = user.Name,
                    passwordHash = user.PasswordHash,
                    personNumber = user.PersonNumber,
                    personPosition = user.PersonPosition,
                    roleid = user.RoleId,
                    id = user.Id
                });
            }


        }

        public async Task UpdateStaff(User user)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var currentUserData = await ById(user.Id);

                var roleId = currentUserData.RoleId;
                var isBlocked = currentUserData.IsBlocked;
                if (roleId == 0)
                    throw new ValidationException("Неудалось получить роль пользователя");

                var old = await ByPersonNumber(user.PersonNumber);
                if (old != null && old.Id != user.Id)
                {
                    throw new Exception("Табельный номер уже занят");
                }

                var sql = Sql.SqlQueryCach["Users.UpdateStaff"];
                await conn.ExecuteAsync(sql, new
                {
                    name = user.Name,
                    personNumber = user.PersonNumber,
                    personPosition = user.PersonPosition,
                    roleId = roleId,
                    isBlocked = isBlocked,
                    id = user.Id,
                    brigadeId = user.BrigadeId
                });
            }
        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Users.Delete"], new {id = id});
            }
        }

        public async Task DeleteStaff(User user)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Users.DeleteStaff"],
                    new { user_id = user.Id });
            }
        }

        /// <summary>
        /// Возврат списка пользователей с пагинацией
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<UserPaging> GetAll(int skip, int limit)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Users.AllPaging"];
                var result = (await conn.QueryAsync<User, Brigade, User>(sql,
                    (user, brigade) => {
                        user.Brigade = brigade;
                        return user;
                    },
                    new {skip = skip, limit = limit})
                ).ToArray();

                foreach (var item in result)
                {
                    item.PasswordHash = null;
                }
                var sqlc = Sql.SqlQueryCach["Users.CountAll"];
                var count = conn.ExecuteScalar<int>(sqlc);
                var output = new UserPaging()
                {
                    Data = result,
                    Total = count
                };

                return output;
            }
        }

        /// <summary>
        /// Возврат списка пользователей с пагинацией, фильтром или сортировкой
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<UserPaging> GetAll(int skip, int limit, string filter)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                string sqlfilter, sql, sqlc;
                CreateFilter(filter, out sqlfilter, out sql, out sqlc);

                var result = (await conn.QueryAsync<User, Brigade, User>(sql,
                    (user, brigade) => {
                        user.Brigade = brigade;
                        return user;
                    },
                    new { skip = skip, limit = limit })
                ).ToArray();

                foreach (var item in result)
                {
                    item.PasswordHash = null;
                }

                var count = conn.ExecuteScalar<int>(sqlc);
                var output = new UserPaging()
                {
                    SortOptions = new SortOptions(),
                    Data = result,
                    Total = count
                };

                return output;
            }
        }

        public async Task<UserDto[]> GetAllForSync()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = "SELECT u.[Id], u.[Login], u.[Name], u.[PersonNumber], u.[PasswordHash], u.[UpdateDate], u.[IsBlocked], CASE WHEN r.[Permissions] = -1 THEN 1 ELSE 0 END as IsAdmin FROM auth_users u left join auth_roles r on r.Id=u.RoleId";
                var result = (await conn.QueryAsync<UserDto>(sql, new {})).ToArray();

                return result;
            }
        }

        public async Task<List<User>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = "SELECT * FROM auth_users";
                var result = await conn.QueryAsync<User>(sql);
                return result.ToList();
            }
        }

        public async Task<UserPaging> GetAllWithLogin (int skip, int limit,string search = null, string sort = null)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                SortOptions sortOptions = new SortOptions();
                string orderBy = "ORDER BY id";

                if(sort != null)
                {
                    sortOptions = JsonConvert.DeserializeObject<SortOptions>(sort);
                    if (sortOptions.Column != null)
                    {
                        orderBy = $"ORDER BY {sortOptions.Column} {(sortOptions.Direction == 1 ? "DESC" : "ASC")}";
                    }
                }              

                var sql = Sql.SqlQueryCach["Users.AllWithLoginPaging"];
                if (search != null)
                    sql = sql.Replace("--and", $"AND (Login LIKE '%' + @search + '%' OR Name LIKE  '%' + @search + '%') ");

                sql += $"{Environment.NewLine} {orderBy} OFFSET @skip ROWS FETCH NEXT @limit ROWS ONLY;";

                var result = (await conn.QueryAsync<User>(sql, new {skip = skip, limit = limit, search})).ToArray();
                var sqlRoleR = new UserRoleRepository();
                foreach (var item in result)
                {
                    item.PasswordHash = null;
                    item.Role = sqlRoleR.GetById(item.RoleId);
                }

                sqlRoleR.Dispose();
                var sqlc = Sql.SqlQueryCach["Users.CountAllWithLogin"];
                if (search != null)
                    sqlc = sqlc.Replace("--and", $"AND (Login LIKE '%' + @search + '%' OR Name LIKE  '%' + @search + '%') ");

                var count = conn.ExecuteScalar<int>(sqlc, new {search});

                var output = new UserPaging()
                {
                    SortOptions = sortOptions,
                    Data = result,
                    Total = count
                };
                return output;
            }            
        }

        public async Task<UserPaging> GetAllWithOutLogin(int skip, int limit)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Users.AllWithOutLoginPaging"];
                var result = (await conn.QueryAsync<User>(sql, new {skip = skip, limit = limit})).ToArray();
                foreach (var item in result)
                {
                    item.PasswordHash = null;
                }

                var sqlc = Sql.SqlQueryCach["Users.CountAllWithOutLogin"];
                var count = conn.ExecuteScalar<int>(sqlc);
                var output = new UserPaging()
                {
                    Data = result,
                    Total = count
                };

                return output;
            }
        }


        public async Task<User> GetUserByLogin(string login)
        {
            //TODO при смене роли у узера надо ебать из кеша. При смене прав у роли надо ебать ее юзеров из кеша. Или просто не раскоменчивать.
            //if (!_memoryCache.TryGetValue(login, out User user))
            //{
            var user = await FindByLoginAsync(new { login = login }, Sql.SqlQueryCach["Users.All"]);

            //    if (user != null)
            //        _memoryCache.Set(login, user);
            //}

            return user;
        }

        //public User GetUserByLoginSync(string login)
        //{
        //    var user = FindByLoginSync(new { login = login }, Sql.SqlQueryCach["Users.All"]);

        //    return user;
        //}

        public async Task<User> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Users.ById"];
                var user = await conn.QueryFirstOrDefaultAsync<User>(sql, new {id = id});
                return user;
            }
        }

        public async Task<User> ByPersonNumber(string number)
        {
            if (number == null)
                return null;

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = "select * from auth_users where [PersonNumber]=@PersonNumber";
                var user = await conn.QueryFirstOrDefaultAsync<User>(sql, new { PersonNumber = number });
                return user;
            }
        }


        //public User GetUserByIdSync(int id)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        var sql = Sql.SqlQueryCach["Users.ById"];
        //        var user = conn.Query<User>(sql, new {id = id});
        //        return user.FirstOrDefault();
        //    }
        //}

        //public User FindByLoginSync(object filterData, string query)
        //{
        //    var lookup = new Dictionary<int, User>();

        //    var result = _db.Connection.Query<User, UserRole, Brigade, User>(query,
        //        (u, role, brigade) =>
        //        {
        //            if (!lookup.TryGetValue(u.Id, out User user))
        //            {
        //                lookup.Add(u.Id, user = u);
        //            }

        //            user.Role = role;
        //            user.Brigade = brigade;

        //            return user;

        //        }, filterData);

        //    return result.FirstOrDefault();
        //}

        public async Task<User> FindByLoginAsync(object filterData, string query)
        {

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var lookup = new Dictionary<int, User>();
                var result = await conn.QueryAsync<User, UserRole, Brigade, User>(query,
                    (u, role, brigade) =>
                    {
                        if (!lookup.TryGetValue(u.Id, out User user))
                        {
                            lookup.Add(u.Id, user = u);
                        }

                        user.Role = role;
                        user.Brigade = brigade;

                        return user;

                    }, filterData);
                var res = result.FirstOrDefault();
                return res;
            }
           
        }

        public async Task<User> GetStaffById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Users.StaffById"];
                var result = await conn.QueryAsync<User, Brigade, User>(
                    sql,
                    (user, brig) =>
                    {
                        user.Brigade = brig;
                        return user;
                    }, new {user_id = id});
                return result.FirstOrDefault();
            }
        }

        //public async Task<User[]> GetStaffByBrigade(Brigade brigade)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        var sql = Sql.SqlQueryCach["Users.StaffByBrigadeId"];
        //        var result = await conn.QueryAsync<User, Brigade, User>(
        //            sql,
        //            (user, brig) =>
        //            {
        //                user.Brigade = brig;
        //                return user;
        //            }, new {brigade_id = brigade.Id});

        //        return result.ToArray();
        //    }
        //}

        //public async Task AddStaffToBrigade(User user, Brigade brigade)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        await conn.ExecuteAsync(Sql.SqlQueryCach["Users.AddStaffToBrigade"],
        //            new {brigade_id = brigade.Id, user_id = user.Id});
        //    }
        //}

        //public async Task DeleteStaffFromBrigade(User user)
        //{
        //    using (var conn = new SqlConnection(AppSettings.ConnectionString))
        //    {
        //        await conn.ExecuteAsync(Sql.SqlQueryCach["Users.DeleteStaffFromBrigade"],
        //            new {user_id = user.Id});
        //    }
        //}

        private static void CreateFilter(string filter, out string sqlfilter, out string sql, out string sqlc)
        {
            sql = $"SELECT u.*, b.* FROM auth_users u LEFT JOIN Brigades b ON b.Id = u.BrigadeId ";

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var filters = JsonConvert.DeserializeObject<Other.Other.FilterBody[]>(filter);
                sqlfilter = "WHERE ";
                for (var index = 0; index < filters.Length; index++)
                {
                    var item = filters[index];
                    sqlfilter = $"{sqlfilter} u.{item.Filter} LIKE '%{item.Value}%' ";
                    if (index < (filters.Length - 1))
                        sqlfilter = $"{sqlfilter} AND ";

                }
                sql += $"{ sqlfilter } ";                

                string orderBy = "ORDER BY u.Name";
                //var sortOptions = JsonConvert.DeserializeObject<SortOptions>(sort);
                //if (sortOptions.Column != null)
                //{
                //    orderBy = $"ORDER BY u.{sortOptions.Column} {(sortOptions.Direction == 1 ? "ASC" : "DESC")}";
                //}

                sql += $"{orderBy} OFFSET @skip ROWS FETCH NEXT @limit ROWS ONLY";

                sqlc = $"select count(*) from auth_users u { sqlfilter }";
            }
            
        }

        public class SortOptions
        {
            public string Column { get; set; }
            public int Direction { get; set; }
        }

        public class UserPaging
        {
            public SortOptions SortOptions { get; set; }
            public User[] Data { get; set; }
            public int Total { get; set; }
        }

        public void Dispose()
        {
            _db.Connection.Close();
        }
    }
}
