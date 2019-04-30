CREATE DATABASE hato
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'English_United States.1252'
    LC_CTYPE = 'English_United States.1252'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;
	
CREATE TABLE apikeys
(
    id integer NOT NULL,
    apikey character varying(100) COLLATE pg_catalog."default" NOT NULL,
    enabled integer NOT NULL,
    CONSTRAINT apikeys_pkey PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)

CREATE TABLE titleids
(
    id bigint NOT NULL DEFAULT nextval('titleids_id_seq'::regclass) ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 ),
    anidb_id integer,
    anilist_id integer,
    kitsu_id integer,
    malid integer,
    animeplanet_id character varying(50) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    notify_id character varying(30) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    mediatype integer NOT NULL,
    CONSTRAINT titleids_pkey PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)

GRANT CREATE, CONNECT ON DATABASE hato TO <hato db username>;
GRANT INSERT, SELECT, UPDATE, DELETE ON TABLE apikeys TO <hato db username>;
GRANT INSERT, SELECT, UPDATE ON TABLE titleids TO <hato db username>;
Grant USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO <hato db username>;
commit;