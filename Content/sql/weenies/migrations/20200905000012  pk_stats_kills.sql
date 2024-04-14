use ace_shard;

CREATE TABLE IF NOT EXISTS `pk_stats_kills` (
  `pk_kills_id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `killer_id` INT UNSIGNED NOT NULL,
  `victim_id` INT UNSIGNED NOT NULL,
  `event_time` DATETIME,  
  PRIMARY KEY (`pk_kills_id`)  
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;
