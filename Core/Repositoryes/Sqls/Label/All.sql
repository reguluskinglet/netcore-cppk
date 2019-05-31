select 
taskPrints.id, taskPrints.UserId, taskPrints.LabelType, 
TemplateLabels.Name as TemplateLabelsName,
taskPrints.CreateDate,
auth_users.Name


from TaskPrints as taskPrints
left join TemplateLabels as templateLabels on taskPrints.TemplateLabelId = templateLabels.Id
left join auth_users as auth_users on auth_users.Id = taskPrints.UserId
ORDER BY taskPrints.id ASC 
OFFSET @skip ROWS
FETCH NEXT @limit ROWS ONLY;