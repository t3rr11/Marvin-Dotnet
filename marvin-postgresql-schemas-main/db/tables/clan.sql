create table if not exists clan
(
    clan_id        integer not null
        constraint clan_pk
            primary key
        constraint clan_un
            unique,
    clan_name      varchar(512),
    clan_callsign  varchar(32),
    clan_level     integer      default 1,
    member_count   integer      default 0,
    members_online integer      default 0,
    forced_scan    boolean      default true,
    is_tracking    boolean      default true,
    joined_on      timestamp(0) default now(),
    last_scan      timestamp(0) default now(),
    patreon        boolean      default false
);

alter table clan
    owner to postgres;

create unique index if not exists clan_clan_id_idx
    on clan (clan_id);