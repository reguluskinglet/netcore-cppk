UPDATE CheckListEquipments
SET FaultType=@fault_type, NameTask=@name_task, Value=@value, ValueType=@value_type,
UpdateDate = CURRENT_TIMESTAMP
WHERE CheckListType=@checklist_type AND EquipmentModelId=@equipment_model_id
IF @@ROWCOUNT = 0
   INSERT INTO CheckListEquipments (CheckListType, EquipmentModelId, FaultType, NameTask, Value, ValueType)
	VALUES (@checklist_type, @equipment_model_id, @fault_type, @name_task, @value, @value_type)
