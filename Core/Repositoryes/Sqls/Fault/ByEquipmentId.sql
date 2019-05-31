select f.name, f.id, f.Description, f.Name from Equipments as e
left join FaultEquipments as fe ON e.id = fe.EquipmentId
left join Faults as f ON fe.FaultId = f.id
where e.Id = @id
