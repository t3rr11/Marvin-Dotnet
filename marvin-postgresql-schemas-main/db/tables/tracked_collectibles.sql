create table tracked_collectibles
(
    hash               bigint               not null
        primary key,
    is_broadcasting    boolean default true not null,
    custom_description varchar,
    type               varchar,
    display_name       varchar,
    custom_name        varchar
);

alter table tracked_collectibles
    owner to postgres;

