SELECT *
FROM Meterage m
LEFT JOIN Labels l on l.Id=m.LabelId
LEFT JOIN Carriages c on c.Id=l.CarriageId
LEFT JOIN Trains t on t.Id=c.TrainId
LEFT JOIN EquipmentModels em on em.Id=l.EquipmentModelId
LEFT JOIN Equipments e on e.Id=em.EquipmentId
WHERE m.InspectionId=@inspection_id AND m.LabelId IS NOT NULL
ORDER BY m.Date ASC