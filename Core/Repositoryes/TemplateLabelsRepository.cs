using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Rest;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public class TemplateLabelsRepository : IDisposable //: IBrigadeTypeRepository
    {
        private readonly IDb _db;

        public TemplateLabelsRepository()
        {
            _db = new Db();
        }

        public async Task<TemplateLabelPaging> GetAll(int skip, int limit)
        {
            var sql = Sql.SqlQueryCach["TemplateLabels.All"];
            var result =
                (await _db.Connection.QueryAsync<TemplateLabel>(sql, new {skip = skip, limit = limit})).ToArray();
            var sqlc = Sql.SqlQueryCach["TemplateLabels.CountAll"];
            var count = (await _db.Connection.QueryAsync<int>(sqlc)).FirstOrDefault();
            var output = new TemplateLabelPaging
            {
                Data = result,
                Total = count
            };

            return output;
        }

        public async Task<int> AddOrUpdate(TemplateLabel input)
        {
            var all = await GetAll(0, Int32.MaxValue);
            if (input.Id == 0 && all.Data.Any(x => x.Name.Equals(input.Name)))
                throw new ValidationException(Error.AlreadyAdd);

            var sql = Sql.SqlQueryCach["TemplateLabels.Add"];
            int id = 0;

            if (input.Id == 0)
                id = (await _db.Connection.QueryAsync<int>(sql, new {name = input.Name, template = input.Template}))
                    .FirstOrDefault();
            else
            {
                sql = Sql.SqlQueryCach["TemplateLabels.Update"];
                id = (await _db.Connection.QueryAsync<int>(sql,
                    new {name = input.Name, template = input.Template, id = input.Id})).FirstOrDefault();
            }

            return id;
        }

        public async Task Delete(int id)
        {
            await _db.Connection.ExecuteAsync(Sql.SqlQueryCach["TemplateLabels.Delete"], new {id = id});
        }

        public class TemplateLabelPaging
        {
            public TemplateLabel[] Data { get; set; }
            public int Total { get; set; }
        }

        public void Dispose()
        {
            _db.Connection.Close();
        }
    }
}