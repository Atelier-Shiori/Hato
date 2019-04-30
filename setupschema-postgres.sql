CREATE TABLE "apikeys" (
  "id" int NOT NULL,
  "apikey" varchar(100) NOT NULL,
  "enabled" int NOT NULL,
  PRIMARY KEY ("id")
);
CREATE TABLE "titleids" (
  "id" BIGSERIAL,
  "anidb_id" int DEFAULT NULL,
  "anilist_id" int DEFAULT NULL,
  "kitsu_id" int DEFAULT NULL,
  "malid" int DEFAULT NULL,
  "animeplanet_id" varchar(50) DEFAULT NULL,
  "notify_id" varchar(30) DEFAULT NULL,
  "mediatype" int NOT NULL,
  PRIMARY KEY ("id")
);
