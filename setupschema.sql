CREATE DATABASE `hato` /*!40100 DEFAULT CHARACTER SET latin1 */;
CREATE TABLE `apikeys` (
  `id` int(11) NOT NULL,
  `apikey` varchar(100) NOT NULL,
  `enabled` tinyint(4) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
CREATE TABLE `titleids` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `anidb_id` int(11) DEFAULT NULL,
  `anilist_id` int(11) DEFAULT NULL,
  `kitsu_id` int(11) DEFAULT NULL,
  `malid` int(11) DEFAULT NULL,
  `animeplanet_id` varchar(50) DEFAULT NULL,
  `notify_id` varchar(30) DEFAULT NULL,
  `mediatype` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
