insert into Labels
(CarriageId, EquipmentModelId, LabelType, Rfid)
values
(@carriageId,@equipmentModelId,@labelType, @rfid)
SELECT SCOPE_IDENTITY()