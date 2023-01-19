create table tracked_progressions
(
    hash         bigint
        constraint tracked_progressions_un
            unique,
    display_name varchar,
    is_tracking  boolean default true
);

alter table tracked_progressions
    owner to postgres;

