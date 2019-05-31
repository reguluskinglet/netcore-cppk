UPDATE
TemplateLabels
set 
Template = @template,
Name = @name,
UpdateDate = CURRENT_TIMESTAMP
WHERE
id = @id
