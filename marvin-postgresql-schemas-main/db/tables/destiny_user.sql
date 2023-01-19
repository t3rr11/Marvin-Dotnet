create table destiny_user
(
    clan_id               bigint,
    display_name          varchar(256),
    time_played           integer default 0,
    clan_join_date        timestamp(0),
    last_played           timestamp(0),
    last_updated          timestamp(0),
    private               boolean default false,
    first_scan            boolean default true,
    current_activity      varchar(128),
    date_activity_started timestamp(0),
    forced_scan           boolean default false,
    metrics               jsonb,
    records               jsonb,
    progressions          jsonb,
    membership_id         bigint not null
        primary key,
    items                 jsonb,
    recent_items          jsonb,
    computed_data         jsonb
);

alter table destiny_user
    owner to postgres;

create index clan_id
    on destiny_user (clan_id);

create index destiny_user_records_art_hash
    on destiny_user using hash (((((records -> '292307915'::text) -> 'Objectives'::text) -> '2096413328'::text) ->
                                 'Progress'::text));

create index destiny_user_records_power_hash
    on destiny_user using hash (((((records -> '3241995275'::text) -> 'Objectives'::text) -> '2678711821'::text) ->
                                 'Progress'::text));

