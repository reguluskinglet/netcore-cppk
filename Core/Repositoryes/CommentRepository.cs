using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Core.Repositoryes
{
    public class CommentRepository : ICommentRepository, IDisposable
    {
        private readonly IDb _db;
        private readonly ILogger _logger;

        public CommentRepository(ILogger logger)
        {
            _db = new Db();
            _logger = logger;
        }

        public CommentRepository(IDb db, ILogger logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<int> AddCommentByTrainTaskId(int taskId, string text, User user)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var item = new TrainTaskComment
                {
                    TrainTaskId = taskId,
                    Text = text,
                    UserId = user.Id,
                    Date = DateTime.Now
                };

                var sql = Sql.SqlQueryCach["Comment.Add"];
                var id = await conn.QueryFirstOrDefaultAsync<int>(sql,
                    new
                    {
                        date = item.Date,
                        text = item.Text,
                        trainTaskId = item.TrainTaskId,
                        userId = item.UserId
                    });
                return id;
            }
        }



        public async Task<TrainTaskComment[]> GetByTaskId(int id)
        {
            using (var conn = new SqlConnection(AppSettings.ConnectionString))
            {
                var sql = Sql.SqlQueryCach["Comment.ByTaskId"];
                var result = await conn.QueryAsync<TrainTaskComment, User, Brigade, TrainTaskComment>(
                    sql,
                    (comment, user, brigade) =>
                    {
                        user.Brigade = brigade;
                        comment.User = user;
                        return comment;
                    }, new {task_id = id});

                return result.ToArray();
            }
        }

        public void Dispose()
        {
            _db.Connection.Close();
        }

    }
}
