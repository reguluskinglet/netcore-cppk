select d.*,c.* from InspectionData d
left join Carriages c on c.Id=d.CarriageId
WHERE d.InspectionId=@inspectionId
Order by c.Number asc, d.UpdateDate asc