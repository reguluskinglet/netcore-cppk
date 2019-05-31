insert into
TemplateLabels
(Template, Name)
VALUES
(@template, @name)
SELECT SCOPE_IDENTITY()