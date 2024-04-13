USE ace_auth;

-- Create the xp_cap table if it does not exist
CREATE TABLE IF NOT EXISTS `xp_cap` (
  `Id` INT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT 'Xp cap for players',
  `DailyTimestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Daily timestamp for XP cap',
  `WeeklyTimestamp` DATETIME NOT NULL DEFAULT (CURRENT_DATE() + INTERVAL 6 DAY) COMMENT 'Weekly timestamp for XP cap',
  `Week` INT UNSIGNED NOT NULL DEFAULT 1 COMMENT 'Week number for XP cap',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='xp_cap for players';