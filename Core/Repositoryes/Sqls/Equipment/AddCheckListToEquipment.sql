INSERT into
CheckListEquipments
(CheckListType, EquipmentModelId, FaultType, NameTask, Value, ValueType)
VALUES
(@checklist_type, @equipment_model_id, @fault_type, @name_task, @value, @value_type)
SELECT SCOPE_IDENTITY()