use ace_world;

UPDATE ace_world.weenie w
JOIN weenie_properties_string name ON name.object_Id = w.class_Id
JOIN weenie_properties_int item_type ON item_type.object_Id = w.class_Id 
JOIN weenie_properties_int stack_encumbrance ON stack_encumbrance.object_Id = w.class_Id 
JOIN weenie_properties_int encumbrance ON encumbrance.object_Id = w.class_Id 
SET stack_encumbrance.value = 0,
    encumbrance.value = 0
WHERE w.type = 5 
    AND item_type.value = 256 
    AND encumbrance.type = 5 
    AND encumbrance.value IS NOT NULL 
    AND stack_encumbrance.type = 13 
    AND stack_encumbrance.value IS NOT NULL;