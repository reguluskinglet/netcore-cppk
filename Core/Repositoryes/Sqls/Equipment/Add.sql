INSERT into
Equipments
(name, description, categoryid)
VALUES
(@name, @description, @category_id)
SELECT SCOPE_IDENTITY()