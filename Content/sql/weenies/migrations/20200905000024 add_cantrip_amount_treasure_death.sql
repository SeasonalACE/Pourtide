use ace_world;

ALTER TABLE treasure_death
ADD COLUMN cantrip_amount INT;

UPDATE treasure_death
SET cantrip_amount = 0
WHERE cantrip_amount IS NULL;