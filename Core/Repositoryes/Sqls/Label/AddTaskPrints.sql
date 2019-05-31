insert into TaskPrints
(CreateDate, Name, UserId, LabelType, templateLabelId)
values
(CURRENT_TIMESTAMP, @name, @userId, @labelType, @templateLabelId)
SELECT SCOPE_IDENTITY()