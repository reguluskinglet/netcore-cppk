using System;
using System.Collections.Generic;
using System.Text;

namespace Rzdppk.Core.Repositoryes.Sqls.Tasks
{
    class ReportCommon
    {
        public static string Sql1Common = @"from TrainTasks ts
OUTER APPLY (select top 1 * from TrainTaskStatuses tss where tss.TrainTaskId=ts.Id order by tss.Date desc) as Status
left join Carriages c on c.Id=ts.CarriageId
left join Models m on m.Id=c.ModelId
left join Trains t on t.Id=c.TrainId
left join EquipmentModels em on em.Id=ts.EquipmentModelId
left join Equipments e on e.Id=em.EquipmentId
left join auth_users u on u.Id=ts.UserId";
        public static string SqlSelect = @"select ts.*,(select count(*) from TrainTaskStatuses tss where tss.TrainTaskId=ts.Id and tss.Status=@repeat_task_status) as Repeats,
Status.*,
c.*,m.*,t.*,em.*,e.*,u.*";
        public static string SqlCount = @"select count(*)";
        public static string Sql1PagingEnd = @"ORDER BY Repeats DESC, ts.CreateDate DESC OFFSET @skip ROWS FETCH NEXT @limit ROWS ONLY;";

        public static string Sql2Common = @"from TrainTasks ts
left join Carriages c on c.Id=ts.CarriageId
left join Trains t on t.Id=c.TrainId
left join EquipmentModels em on em.Id=ts.EquipmentModelId
left join Equipments e on e.Id=em.EquipmentId";

        public static string Sql2Count = @"select count(*)";
        public static string Sql2Select = @"select ts.*,c.*,t.*,em.*,e.*";

        public static string Sql2End = @"OFFSET @skip ROWS FETCH NEXT @limit ROWS ONLY;";
    }
}
