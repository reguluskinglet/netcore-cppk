using System;
using System.Collections.Generic;
using System.Text;

namespace Rzdppk.Core.Repositoryes.Sqls.Label
{
    public class LabelCommon
    {
        
        public static string SqlCommon = @"

from TaskPrints as taskPrints
left join TemplateLabels as templateLabels on taskPrints.TemplateLabelId = templateLabels.Id
left join auth_users as auth_users on auth_users.Id = taskPrints.UserId
where taskPrints.id is not null ";

        public static string SqlSelectGetAll = @"
select 
taskPrints.id, taskPrints.UserId, taskPrints.LabelType, taskPrints.Name,
TemplateLabels.Name as TemplateLabelsName,
taskPrints.CreateDate,
auth_users.Name as UserName
";

        public static string SqlSelect = @"select *";
        public static string SqlCount = @"select count(*)";
        public static string SqlPagingEnd = @"ORDER BY ts.id ASC OFFSET @skip ROWS FETCH NEXT @limit ROWS ONLY;";
        public static string SqlPagingEndSortDate = @"ORDER BY taskPrints.CreateDate desc OFFSET @skip ROWS FETCH NEXT @limit ROWS ONLY;";

        public class FilterBody
        {
            public string Filter { get; set; }
            public string Value { get; set; }
        }
    }
}
