use ace_shard;

UPDATE ace_shard.biota biota
JOIN biota_properties_string name ON biota.id = name.object_Id AND name.type = 1
JOIN biota_properties_int slayer ON biota.Id = slayer.object_Id AND slayer.type = 166
LEFT JOIN biota_properties_int work ON biota.Id = work.object_Id AND work.type = 105
SET slayer.value = 0 
WHERE slayer.value = 31 AND work.object_id IS NULL AND name.value LIKE '%hollow%';