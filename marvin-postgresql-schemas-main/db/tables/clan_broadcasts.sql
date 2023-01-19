create table clan_broadcasts
(
    guild_id      varchar               not null,
    clan_id       bigint                not null,
    type          integer               not null,
    was_announced boolean default false not null,
    old_value     varchar,
    new_value     varchar,
    date          timestamp
);

alter table clan_broadcasts
    owner to postgres;

