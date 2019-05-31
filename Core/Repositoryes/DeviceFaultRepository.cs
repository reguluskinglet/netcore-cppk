using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Rzdppk.Core.Helpers;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;

namespace Rzdppk.Core.Repositoryes
{
    public class DeviceFaultRepository : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IDb _db;

        public DeviceFaultRepository(ILogger logger)
        {
            _db = new Db();
            _logger = logger;
        }

        public DeviceFaultRepository(IDb db)
        {
            _db = db;
        }

        public DeviceFaultRepository()
        {
            _db = new Db();
        }

        public async Task<DeviceFault[]> GetAllForSync()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                const string sql = "SELECT [Id],[Description],[Name],[UpdateDate] FROM [DeviceFaults]";

                var result = (await conn.QueryAsync<DeviceFault>(sql, new {})).ToArray();

                return result;
            }
        }

        public async Task<List<DeviceFault>> GetAll()
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                const string sql = "SELECT * FROM [DeviceFaults]";

                var result = (await conn.QueryAsync<DeviceFault>(sql, new { })).ToList();

                return result;
            }
        }

        public async Task<DeviceFault> Add(DeviceFault input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                const string sql = "INSERT INTO [DeviceFaults] ([Name],[Description]) VALUES(@Name, @Description) SELECT SCOPE_IDENTITY()";

                var id = await conn.QueryFirstOrDefaultAsync<int>(sql, new {Name = input.Name, Description = input.Description});

                return await ById(id);
            }
        }

        public async Task<DeviceFault> Update(DeviceFault input)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                const string sql = "UPDATE [DeviceFaults] SET [Name]=@Name, [Description]=@Description, [UpdateDate]=GETDATE() WHERE id=@Id";

                await conn.ExecuteAsync(sql, new {Name = input.Name, Description = input.Description, Id = input.Id});

                return await ById(input.Id);
            }
        }

        public async Task<DeviceFault> ById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                const string sql = "SELECT * FROM [DeviceFaults] WHERE id=@Id";

                return await conn.QueryFirstOrDefaultAsync<DeviceFault>(sql, new {Id = id});
            }
        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                const string sql = "DELETE FROM [DeviceFaults] WHERE id=@Id";

                try
                {
                    await conn.ExecuteAsync(sql, new {Id = id});
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547) //foreign key violation
                    {
                        throw new Exception("ошибка удаления, объект имеет связи в системе");
                    }

                    throw;
                }
            }
        }

        public void Dispose()
        {
            _db.Connection?.Close();
        }
    }
}
