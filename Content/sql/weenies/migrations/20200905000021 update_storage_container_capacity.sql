use ace_shard;

UPDATE ace_shard.biota b 
JOIN biota_properties_int container ON b.id = container.object_Id AND container.`type` = 7
SET container.value = 10
WHERE b.weenie_Type = 57;