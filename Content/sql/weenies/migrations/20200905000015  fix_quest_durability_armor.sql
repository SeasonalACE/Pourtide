use ace_shard;

UPDATE ace_shard.biota biota
JOIN biota_properties_string name ON biota.id = name.object_Id AND name.type = 1
JOIN biota_properties_int armor ON biota.Id = armor.object_Id AND armor.type = 28 AND armor.value > 0
JOIN biota_properties_int original ON biota.Id = original.object_Id AND original.type = 60000
LEFT JOIN biota_properties_int work ON biota.Id = work.object_Id AND work.type = 105
SET armor.value = original.value
WHERE work.object_id IS NULL AND original.value IS NOT NULL;