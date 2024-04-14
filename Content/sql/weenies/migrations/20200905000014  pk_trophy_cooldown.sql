use ace_shard;

CREATE TABLE IF NOT EXISTS `pk_trophy_cooldown` (
  `trophy_cooldown_id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `killer_id` INT UNSIGNED NOT NULL,
  `victim_id` INT UNSIGNED NOT NULL,
  `cooldown_end_time` DATETIME,
  PRIMARY KEY (`trophy_cooldown_id`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;