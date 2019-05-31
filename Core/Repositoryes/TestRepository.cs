using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Repositoryes.Sqls.Tasks;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Enums;
using TaskStatus = Rzdppk.Model.Enums.TaskStatus;

namespace Rzdppk.Core.Repositoryes
{
    public class TestRepository : IDisposable// : ITaskStatusRepository
    {
        private readonly IDb _db;
        
        public TestRepository()
        {
            _db = new Db();
        }



        public void Exception()
        {
            throw new Exception("eeee");
        }




        //public TrainTaskStatus GetByTrainTaskId(int id)
        //{
        //    var sql = Sql.SqlQueryCach["TaskStatus.LastByTrainTaskId"];
        //    var result = _db.Connection.Query<TrainTaskStatus>(sql, new { id = id });
        //    return result.FirstOrDefault();
        //}


        //public async Task<int> ChangeStatusByTrainTaskId(int taskId, int statusId, SqlTransaction transaction)
        //{
        //    var trainTaskStatus = new TrainTaskStatus
        //    {
        //        TrainTaskId = taskId,
        //        Status = (TaskStatus) statusId,
        //        UserId = 1,
        //        Date = DateTime.Now
        //    };
        //    //TODO переделать на получение из сесии

        //    var sql = Sql.SqlQueryCach["TaskStatus.Add"];
        //    var id = await _db.Connection.QueryAsync<int>(sql,
        //        new
        //        {
        //            date = trainTaskStatus.Date,
        //            status = trainTaskStatus.Status,
        //            trainTaskId = trainTaskStatus.TrainTaskId,
        //            userId = trainTaskStatus.Id
        //        }, transaction);

        //    return id.FirstOrDefault();
        //    //_db.Connection.Execute(Sql.SqlQueryCach["CategoryEquipment.Add"], new { name = equipmentCategory.Name, description = equipmentCategory.Description });
        //    //(@date, @status, @trainTaskId, UserId)


        //}

        public void Dispose()
        {
            _db.Connection.Close();
        }

    }
}
