select * from EquipmentActs as ea
left join ActCategories as ac ON ac.Id = ea.ActCategoryId
where ea.EquipmentId = @equipmentId