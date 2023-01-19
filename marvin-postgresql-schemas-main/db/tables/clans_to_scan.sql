create table clans_to_scan
(
    clan_id    bigint not null
        primary key,
    guild_id   varchar,
    channel_id varchar
);

alter table clans_to_scan
    owner to postgres;

create trigger "OnDataInsert"
    after insert
    on clans_to_scan
    for each row
execute procedure "NotifyOnDataInsert"();

