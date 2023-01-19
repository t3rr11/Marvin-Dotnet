create table tracked_records
(
    hash             bigint                not null
        primary key
        constraint tracked_records_un
            unique,
    display_name     varchar,
    is_tracking      boolean default true,
    character_scoped boolean default false,
    is_reported      boolean default false not null
);

alter table tracked_records
    owner to postgres;

