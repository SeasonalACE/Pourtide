use ace_shard;

CREATE TABLE IF NOT EXISTS `pk_stats_damage` (
  `pk_damage_id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `attacker_id` INT UNSIGNED NOT NULL,
  `defender_id` INT UNSIGNED NOT NULL,
  `damage_amount` INT NOT NULL,
  `event_time` DATETIME,
  PRIMARY KEY (`pk_damage_id`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;