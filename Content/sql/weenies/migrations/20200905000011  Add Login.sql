use ace_auth;

CREATE TABLE IF NOT EXISTS `character_login` (
  `characterLoginLogId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `accountId` INT UNSIGNED NOT NULL,
  `accountName` VARCHAR(50) NOT NULL, 
  `characterId` INT UNSIGNED NOT NULL,
  `characterName` VARCHAR(50) NOT NULL,   
  `sessionIP` VARCHAR(45),
  `loginDateTime` DATETIME,  
  `logoutDateTime` DATETIME,
  PRIMARY KEY (`characterLoginLogId`)  
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;
