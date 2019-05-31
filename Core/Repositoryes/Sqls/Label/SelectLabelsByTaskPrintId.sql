select
tpi.id as id, 
labels.Id as LabelsId,
labels.Rfid AS Rfid,
eq.Name as EquipmentName,
tpi.TimePrinted as TimePrinted,
tl.Template as Template,
models.Name as ModelName,
pem.Name as ParentName,
c.Number as CarriageNumber, 
c.Serial as CarriageSerialNumber, 
models.ModelType as ModelType,
trains.Name as trainName

from
TaskPrintItems as tpi
left join Labels as labels on tpi.LabelId = labels.Id
left join EquipmentModels as em ON em.id = labels.EquipmentModelId
left join Models as models ON em.ModelId = models.id
left join Equipments as eq ON eq.Id = em.EquipmentId
left join TaskPrints as tp ON tp.id = tpi.TaskPrintId
left join TemplateLabels as tl ON tl.Id = tp.TemplateLabelId
left join Carriages as c on c.Id=labels.CarriageId
left join Trains as trains ON trains.id = c.TrainId
outer apply (
	select eq.name from EquipmentModels as em1
	inner join EquipmentModels as em2 ON em1.ParentId = em2.Id
	inner join Equipments as eq ON em2.EquipmentId = eq.Id
	where em1.id = em.id) as pem
where tp.id = @id