select 
taskPrints.id, taskPrints.LabelType, taskPrints.Name,
TemplateLabels.Name as TemplateLabelsName, TemplateLabels.Id as TemplateLabelsId,
taskPrints.CreateDate,
auth_users.Name as UserName 

from TaskPrints as taskPrints
left join TemplateLabels as templateLabels on taskPrints.TemplateLabelId = templateLabels.Id
left join auth_users as auth_users on auth_users.Id = taskPrints.UserId
where taskPrints.id = @id