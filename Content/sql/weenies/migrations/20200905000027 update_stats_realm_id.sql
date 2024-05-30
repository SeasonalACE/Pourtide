USE ace_shard;

ALTER TABLE `character_login`
ADD COLUMN `home_realm_id` SMALLINT UNSIGNED NOT NULL DEFAULT 0,
ADD COLUMN `current_realm_id` SMALLINT UNSIGNED NOT NULL DEFAULT 0;

UPDATE `character_login`
SET `home_realm_id` = 6
WHERE `home_realm_id` = 0;

UPDATE `character_login`
SET `current_realm_id` = 6
WHERE `current_realm_id` = 0;

ALTER TABLE `pk_trophy_cooldown`
ADD COLUMN `home_realm_id` SMALLINT UNSIGNED NOT NULL DEFAULT 0;

ALTER TABLE `pk_trophy_cooldown`
ADD COLUMN `current_realm_id` SMALLINT UNSIGNED NOT NULL DEFAULT 0;

UPDATE `pk_trophy_cooldown`
SET `home_realm_id` = 6
WHERE `home_realm_id` = 0;

UPDATE `pk_trophy_cooldown`
SET `current_realm_id` = 6
WHERE `current_realm_id` = 0;

ALTER TABLE `pk_stats_damage`
ADD COLUMN `home_realm_id` SMALLINT UNSIGNED NOT NULL DEFAULT 0;

ALTER TABLE `pk_stats_damage`
ADD COLUMN `current_realm_id` SMALLINT UNSIGNED NOT NULL DEFAULT 0;

UPDATE `pk_stats_damage`
SET `home_realm_id` = 6
WHERE `home_realm_id` = 0;

UPDATE `pk_stats_damage`
SET `current_realm_id` = 6
WHERE `current_realm_id` = 0;

ALTER TABLE `pk_stats_kills`
ADD COLUMN `home_realm_id` SMALLINT UNSIGNED NOT NULL DEFAULT 0;

ALTER TABLE `pk_stats_kills`
ADD COLUMN `current_realm_id` SMALLINT UNSIGNED NOT NULL DEFAULT 0;

UPDATE `pk_stats_kills`
SET `home_realm_id` = 6
WHERE `home_realm_id` = 0;

UPDATE `pk_stats_kills`
SET `current_realm_id` = 6
WHERE `current_realm_id` = 0;
