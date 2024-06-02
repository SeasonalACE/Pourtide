USE ace_shard;

ALTER TABLE `pk_stats_damage`
    ADD COLUMN `is_crit` BOOLEAN NOT NULL AFTER `current_realm_id`,
    ADD COLUMN `combat_mode` INT UNSIGNED NOT NULL AFTER `is_crit`;