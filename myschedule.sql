DROP DATABASE IF EXISTS myschedule;

create database myschedule;

\c myschedule;

drop table if exists user_info;
create table user_info(
	id serial not null,
	user_id character varying(100) not null,
	password character varying(100) not null,
	registration_date timestamp not null,
	update_date timestamp not null,
	status integer,
	unique(user_id)
);

drop table if exists schedule_info;
create table schedule_info(
	schedule_id serial not null, 
	user_id character varying(100) not null references user_info(user_id), 
	start_time timestamp not null, 
	ending_time timestamp not null, 
	subject character varying(255) not null, 
	detail text
);

drop table if exists update_history;
create table update_history( 
	history_id serial not null, 
	user_id character varying(100) not null references user_info(user_id), 
	update_type character varying(100) not null, 
	schedule_id integer not null, 
	update_start_time timestamp not null, 
	update_ending_time timestamp not null, 
	subject character varying(255) not null, 
	detail text, 
	update_time timestamp not null, 
	nonce integer, 
	previous_key text, 
	key text
);
