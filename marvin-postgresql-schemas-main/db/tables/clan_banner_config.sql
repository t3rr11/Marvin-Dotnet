create table clan_banner_config
(
    clan_id                   integer not null
        constraint clan_banner_config_pk
            primary key,
    decal_id                  bigint,
    decal_color_id            bigint,
    decal_background_color_id bigint,
    gonfalon_id               bigint,
    gonfalon_color_id         bigint,
    gonfalon_detail_id        bigint,
    gonfalon_detail_color_id  bigint
);

alter table clan_banner_config
    owner to postgres;

create unique index clan_banner_config_clan_id_idx
    on clan_banner_config (clan_id);

