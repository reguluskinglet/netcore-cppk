using System;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Sqls;

namespace Rzdppk.Core.Repositoryes
{
    public class PermissionRepository : IDisposable
    {
        private static  IDb _db;
        
        public PermissionRepository()
        {
            _db = new Db();
        }

        public int GetPermissionBits(string controller, string action)
        {
            var sql = Sql.SqlQueryCach["Permission.ByControllerAction"];

            int res;

            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {

                try
                {
                    var result = conn.Query<int>(sql, new {controller = controller, action = action});


                    if (result.Any())
                    {
                        res = result.First();
                    }
                    else
                    {
                        const int newPermission = -1;
                        conn.Execute(Sql.SqlQueryCach["Permission.CreateNew"],
                            new {controller = controller, action = action, value = newPermission});
                        res = newPermission;
                    }

                }
                catch (Exception)
                {
                    const int newPermission = -1;
                    conn.Execute(Sql.SqlQueryCach["Permission.CreateNew"],
                        new { controller = controller, action = action, value = newPermission });
                    res = newPermission;
                }
            }

            return res;
        }


        public void Dispose()
        {
            _db.Connection.Close();
        }
    }
}
