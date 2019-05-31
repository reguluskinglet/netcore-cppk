using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Model;
using Rzdppk.Model.Enums;

namespace Rzdppk.Core.Repositoryes
{
    public class DocumentRepository : IDocumentRepository, IDisposable
    {
        private readonly IDb _db;
        private readonly ILogger _logger;

        public DocumentRepository(ILogger logger)
        {
            _db = new Db();
            _logger = logger;
        }

        public DocumentRepository(IDb db, ILogger logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Document> GetById(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Document.ById"];
                var result = await conn.QueryAsync<Document, TrainTaskComment, Document>(
                    sql,
                    (doc, comment) =>
                    {
                        doc.TrainTaskComment = comment;
                        return doc;
                    }, new {id = id});

                return result.FirstOrDefault();
            }
        }

        public async Task<Document[]> GetByTaskId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Document.ByTaskId"];
                var result = await conn.QueryAsync<Document>(
                    sql, new {task_id = id});

                return result.ToArray();
            }
        }

        public async Task<Document[]> Add(Document[] docs)
        {

            using (var transaction = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var conn = new SqlConnection(AppSettings.ConnectionString))
                {
                    var ret = new List<Document>();

                    foreach (var doc in docs)
                    {
                        var id = await conn.QueryFirstOrDefaultAsync<int>(Sql.SqlQueryCach["Document.Add"],
                            new
                            {
                                name = doc.Name,
                                description = doc.Description,
                                comment_id = doc.TrainTaskCommentId,
                                documentType = doc.DocumentType
                            });

                        doc.Id = id;
                        ret.Add(doc);
                    }

                    transaction.Complete();
                    return ret.ToArray();
                }
                
            }
        }


        public async Task AddToCommentId(int[] docsId, int trainTaskCommentId)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                foreach (var docId in docsId)
                {
                    var sql = Sql.SqlQueryCach["Document.AddToCommentId"];
                    await conn.ExecuteAsync(sql,new {id = docId, trainTaskCommentId = trainTaskCommentId});

                }
            }

        }

        public async Task Delete(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                await conn.ExecuteAsync(Sql.SqlQueryCach["Document.Delete"], new {id = id});
            }
        }


        public void Dispose()
        {
            _db.Connection.Close();
        }
    }
}
