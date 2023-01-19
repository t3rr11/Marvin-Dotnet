create table registered_users
(
    user_id       varchar(128) not null
        constraint registered_users_pk
            primary key,
    username      varchar(128) not null,
    platform      integer,
    created_at    timestamp default now(),
    membership_id bigint
);

alter table registered_users
    owner to postgres;

