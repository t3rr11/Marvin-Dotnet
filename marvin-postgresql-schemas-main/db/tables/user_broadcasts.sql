create table user_broadcasts
(
    guild_id        varchar               not null,
    clan_id         bigint                not null,
    type            integer               not null,
    was_announced   boolean default false not null,
    date            timestamp             not null,
    membership_id   bigint                not null,
    hash            bigint                not null,
    additional_data jsonb,
    primary key (guild_id, clan_id, type, membership_id, hash)
);

alter table user_broadcasts
    owner to postgres;

