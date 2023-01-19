create table tracked_metrics
(
    hash         bigint not null
        constraint tracked_metrics_pk
            primary key
        constraint tracked_metrics_un
            unique,
    display_name varchar,
    is_tracking  boolean default true
);

alter table tracked_metrics
    owner to postgres;

