create user test with encrypted password 'test';
create database dblight owner test Encoding = 'UTF8';
grant all privileges on database dblight to test;
-- change database to dblight
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO test;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO test;

create table role
(
	role_id integer not null
		constraint role_pk
			primary key,
	role_name varchar(64)
);

-- alter table role owner to test;

create unique index role_role_name_uindex on role (role_name);

create table sex
(
	sex_id integer not null
		constraint sex_pk
			primary key,
	sex_name varchar(64)
);

-- alter table sex owner to test;

create unique index sex_sex_name_uindex on sex (sex_name);

create table role_user
(
	role_id integer not null,
	user_id integer not null,
	constraint role_user_pk
		primary key (role_id, user_id)
);

-- alter table role_user owner to test;

create table "user" (
	user_id integer not null
		constraint user_pk
			primary key,
	user_name varchar(64),
	we_chat_code varchar(64),
	phone varchar(16),
	birthday timestamp,
	income money,
	height numeric,
	sex_id integer,
    verify_id bigint,
	married boolean,
	remark text,
	photo bytea,
	register_time timestamp
);

-- alter table "user" owner to test;

create unique index user_phone_uindex on "user" (phone asc);

create unique index user_we_chat_code_uindex on "user" (we_chat_code asc);

