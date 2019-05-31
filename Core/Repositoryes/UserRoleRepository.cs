using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Rzdppk.Core.Extensions;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public class UserRoleRepository : IUserRoleRepository, IDisposable
    {
        private static  IDb _db;
        

        public UserRoleRepository()
        {
            _db = new Db();
        }


        public async Task<UserRolePaging> GetAll(int skip, int limit)
        {
            UserRolePaging output;

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["UserRole.All"];
                var result =
                    (await conn.QueryAsync<UserRole>(sql, new {skip = skip, limit = limit})).ToArray();
                var sqlc = Sql.SqlQueryCach["UserRole.CountAll"];
                var count = await conn.ExecuteScalarAsync<int>(sqlc);

                var toData = new List<UserRoleUi>();
                foreach (var item in result)
                {
                    var permissionsArray = ConvertPermissionsToUi(item.Permissions);
                    var toUi = new UserRoleUi
                    {
                        Role = item,
                        PermissionsArray = permissionsArray
                    };
                    toData.Add(toUi);
                }

                output = new UserRolePaging()
                {
                    Data = toData.ToArray(),
                    Total = count
                };
            }

            return output;
        }

        public Dictionary<int, int> ConvertPermissionsToUi (int input)
        {
            Dictionary<int, int> output = new Dictionary<int, int>();

            var binary = Convert.ToString(input, 2);
            var binaryCharArray = binary.ToCharArray();

            var reverce = binaryCharArray.Reverse();

            var count = 0;
            foreach (var binaryChar in reverce)
            {
                int.TryParse(binaryChar.ToString(), out var intBit);
                output.Add(count, intBit);
                count++;
            }
            if (count < 32)
                for (int i = count+1; i < 32; i++)
                {
                    output.Add(i, 0);
                }

            return output;
        }


        public  int ConvertPermissionsToInt(Dictionary<int, int> input)
        {
            string stringToConvert = null;
            var reverce = input.Reverse();
            foreach (var item in reverce)
            {
                stringToConvert = $"{stringToConvert}{item.Value}";
            }
            //var wcwc = Convert.ToInt32(qweqwe, 2);
            var binary = Convert.ToInt32(stringToConvert, 2);

            var stringInt = Convert.ToString(binary, 10);

            int.TryParse(stringInt, out var output);

            return output;
        }


        public class UserRoleUi
        {
            public UserRole Role { get; set;}
            public Dictionary<int,int> PermissionsArray { get; set; }

        }

        public UserRole GetById(int id)
        {
            UserRole res;

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["UserRole.ById"];
                var result = conn.Query<UserRole>(sql, new {id = id});
                res = result.FirstOrDefault();
            }

            return res;
        }

        //ебохардкод
        public Dictionary<int,string> GetAuthorityArray()
        {
            var result = new Dictionary<int,string>();
            result.Add(0, "Доступ в журнал");
            result.Add(1, "Доступ в задачи");
            result.Add(2, "Доступ в расписание");
            result.Add(3, "Доступ в справочники");
            result.Add(4, "Доступ в метки");
            result.Add(5, "Доступ в отчеты");
            result.Add(6, "Сотрудник локомотивной бригады");
            result.Add(7, "Сотрудник бригады депо");
            result.Add(8, "Сотрудник бригады приемки");
            result.Add(9, "Разрешить высталять статус в работе");
            result.Add(10, "Разрешить высталять статус принято к исполнению");
            result.Add(11, "Разрешить высталять статус выполнено");
            result.Add(12, "Разрешить высталять статус не подтверждено");
            result.Add(13, "Разрешить высталять статус к переделке");
            result.Add(14, "Разрешить высталять статус к подтверждению");
            result.Add(15, "Разрешить высталять статус закрыто");
            result.Add(16, "Разрешить менять исполнителя");
            result.Add(17, "Разрешить добавлять задачи");
            result.Add(18, "Сотрудник администрации");
            result.Add(19, "Печать акта");

            return result;
        }

        public async Task<int> Add(UserRoleUi input)
        {
            input.Role.Permissions = ConvertPermissionsToInt(input.PermissionsArray);
            var id = await _db.Connection.QueryAsync<int>(Sql.SqlQueryCach["UserRole.Add"], new { name = input.Role.Name, permissions = input.Role.Permissions });
            return id.FirstOrDefault();
        }

        public async Task AddUpdateUserRole(UserRoleUi input)
        {
            if (input.Role.Id == 0)
                await Add(input);
            else
            {
                input.Role.Permissions = ConvertPermissionsToInt(input.PermissionsArray);
                await _db.Connection.ExecuteAsync(Sql.SqlQueryCach["UserRole.Update"], new { id = input.Role.Id, name = input.Role.Name, permissions = input.Role.Permissions });
            }
        }

        public async Task Delete(int id)
        {
            var sql = Sql.SqlQueryCach["UserRole.Delete"];
            await _db.Connection.ExecuteAsync(sql, new { id = id });
        }

        public class UserRolePaging
        {
            public UserRoleUi[] Data { get; set; }
            public int Total { get; set; }
        }

        public void Dispose()
        {
            _db.Connection.Close();
        }
    }
}
