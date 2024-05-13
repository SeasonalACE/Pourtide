use ace_shard;

UPDATE ace_shard.biota b
JOIN biota_properties_string name ON name.object_Id = b.id
JOIN biota_properties_int item_type ON item_type.object_Id = b.id 
JOIN biota_properties_int stack_encumbrance ON stack_encumbrance.object_Id = b.id
JOIN biota_properties_int encumbrance ON encumbrance.object_Id = b.id 
SET stack_encumbrance.value = 0,
    encumbrance.value = 0
WHERE b.weenie_Type  = 5 
    AND item_type.value = 256 
    AND encumbrance.type = 5 
    AND encumbrance.value IS NOT NULL 
    AND stack_encumbrance.type = 13 
    AND stack_encumbrance.value IS NOT NULL;