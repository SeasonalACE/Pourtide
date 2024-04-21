use ace_shard;

UPDATE ace_shard.biota biota
JOIN biota_properties_string name ON biota.id = name.object_Id AND name.type = 1
JOIN biota_properties_int damage ON biota.Id = damage.object_Id AND damage.type = 44
SET damage.value = 30
WHERE name.value = 'Chorizite Arrow';