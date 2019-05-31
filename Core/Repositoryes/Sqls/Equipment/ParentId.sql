SELECT e.* from EquipmentModels as em
left join Equipments as e ON em.EquipmentId = e.Id
where em.id = @id