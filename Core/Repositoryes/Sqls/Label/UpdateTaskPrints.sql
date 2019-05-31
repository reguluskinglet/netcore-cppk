UPDATE
TaskPrints
SET 
Name = @name, 
UserId = @userId,
LabelType = @labelType,
TemplateLabelId = @templateLabelId,
UpdateDate = CURRENT_TIMESTAMP
WHERE
id = @id