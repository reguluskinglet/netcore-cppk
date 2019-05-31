using System;
using System.Collections.Generic;
using System.Text;

namespace Rzdppk.Core.Repositoryes.Sqls.Tasks
{
    public class TaskCommon
    {
        //        public static string SqlCommon = @"from TrainTasks ts
        //left join Carriages c on c.Id=ts.CarriageId
        //left join Trains t on t.Id=c.TrainId
        //left join EquipmentModels em on em.Id=ts.EquipmentModelId
        //left join Equipments e on e.Id=em.EquipmentId
        //left join Models m on m.Id=em.ModelId
        //left join Faults f on f.id=ts.FaultId
        //left join Inspections i on i.Id=ts.InspectionId
        //left join auth_users u on u.Id=ts.UserId
        //outer apply (select top 1 * from TrainTaskStatuses tss where tss.TrainTaskId=ts.Id order by tss.Date desc) as st  where st.status is not null and st.status != 0 ";
        public static string SqlCommon = @"from TrainTasks ts
left join Carriages c on c.Id=ts.CarriageId
left join Trains t on t.Id=c.TrainId
left join EquipmentModels em on em.Id=ts.EquipmentModelId
left join Equipments e on e.Id=em.EquipmentId
left join Models m on m.Id=em.ModelId
left join TrainTaskAttributes trainTaskAttributes ON trainTaskAttributes.TrainTaskId = ts.Id
left join Faults f on f.id=trainTaskAttributes.FaultId
left join Inspections i on i.Id=trainTaskAttributes.InspectionId
left join auth_users u on u.Id=ts.UserId
outer apply (select top 1 * from TrainTaskStatuses tss where tss.TrainTaskId=ts.Id order by tss.Date desc) as st  
outer apply (select top 1 * from TrainTaskExecutors tte where tte.TrainTaskId=ts.Id order by tte.Date desc) as tte  

where (st.status is null or st.status != 0)  ";




        public static string SqlSelect = @"select *";
        public static string SqlCount = @"select count(*)";
        public static string SqlPagingEnd = @"ORDER BY ts.id ASC OFFSET @skip ROWS FETCH NEXT @limit ROWS ONLY;";
        public static string SqlPagingEndSortDate = @"ORDER BY ts.CreateDate desc OFFSET @skip ROWS FETCH NEXT @limit ROWS ONLY;";

        public static string SqlPdf = @"select ts.*,c.*,t.*,em.*,em1.*,e.*,e1.*,f.*,i.*,t1.*,u.*,lab.* from TrainTasks ts
left join Carriages c on c.Id=ts.CarriageId
left join Trains t on t.Id=c.TrainId
left join EquipmentModels em on em.Id=ts.EquipmentModelId
left join EquipmentModels em1 on em1.Id=em.ParentId
left join Equipments e on e.Id=em.EquipmentId
left join Equipments e1 on e1.Id=em1.EquipmentId
left join TrainTaskAttributes ta on ta.TrainTaskId=ts.Id
left join Faults f on f.id=ta.FaultId
left join Inspections i on i.Id=ta.InspectionId
left join Trains t1 on t1.Id=i.TrainId
left join auth_users u on u.Id=i.UserId
outer apply (select top 1 * from Labels l where l.CarriageId=ts.CarriageId and l.EquipmentModelId=ts.EquipmentModelId) as lab
where ta.InspectionId=@inspection_id";

        public static string SqlByTrainPdf = @"select ts.*,c.*,t.*,em.*,em1.*,e.*,e1.*,f.*,u.*,lab.*
from TrainTasks ts
left join Carriages c on c.Id=ts.CarriageId
left join Trains t on t.Id=c.TrainId
left join EquipmentModels em on em.Id=ts.EquipmentModelId
left join EquipmentModels em1 on em1.Id=em.ParentId
left join Equipments e on e.Id=em.EquipmentId
left join Equipments e1 on e1.Id=em1.EquipmentId
left join TrainTaskAttributes ta on ta.TrainTaskId=ts.Id
left join Faults f on f.Id=ta.FaultId
left join auth_users u on u.Id=ts.UserId
outer apply (select top 1 * from Labels l where l.CarriageId=ts.CarriageId and l.EquipmentModelId=ts.EquipmentModelId) as lab
where ts.id in @taskIds";

        public class FilterBody
        {
            public string Filter { get; set; }
            public string Value { get; set; }
        }

    }
}
