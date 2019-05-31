select
count(tpi.id)

from
TaskPrintItems as tpi
left join Labels as labels on tpi.LabelId = labels.Id
left join EquipmentModels as em ON em.id = labels.EquipmentModelId
left join Equipments as eq ON eq.Id = em.EquipmentId
left join TaskPrints as tp ON tp.id = tpi.TaskPrintId
left join TemplateLabels as tl ON tl.Id = tp.TemplateLabelId
where tp.id = @id
