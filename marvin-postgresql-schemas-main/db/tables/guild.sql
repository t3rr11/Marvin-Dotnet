create table guild
(
    guild_id             varchar(128) not null
        constraint guild_pk
            primary key,
    guild_name           varchar(1024),
    owner_id             varchar(128),
    owner_avatar         varchar(128),
    is_tracking          boolean   default true,
    clans                jsonb,
    joined_on            timestamp default now(),
    broadcasts_config    jsonb     default '{"channel_id": null, "tracked_items": [], "tracked_titles": [], "clan_track_mode": 1, "is_broadcasting": false, "item_track_mode": 2, "title_track_mode": 1}'::jsonb,
    announcements_config jsonb     default '{"xur": 1, "adas": 1, "gunsmiths": 1, "channel_id": null, "wellspring": 1, "lost_sectors": 1, "is_announcing": false}'::jsonb
);

alter table guild
    owner to postgres;

create unique index guild_guild_id_idx
    on guild (guild_id);

