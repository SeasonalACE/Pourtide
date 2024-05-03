use ace_shard;

UPDATE ace_shard.biota b
JOIN biota_properties_int attuned ON b.id = attuned.object_Id and attuned.`type`= 114
JOIN biota_properties_int bonded ON b.id = bonded.object_Id and bonded.`type`= 33
SET attuned.value = 0,
    bonded.value = 0
WHERE b.weenie_Class_Id = 604001;