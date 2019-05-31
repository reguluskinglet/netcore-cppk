select top 1 * from TrainTaskStatuses
where TrainTaskId = @id
ORDER BY Date desc