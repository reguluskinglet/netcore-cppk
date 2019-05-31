select l.*, tp.TaskPrintId, tp.Id as TaskPrintItemsId
from labels as l
left join TaskPrintItems as tp ON tp.LabelId = l.Id
where 
CarriageId = @carriageId and
EquipmentModelId = @equipmentModelId
