import { CommandInteraction, Guild } from 'discord.js';
import { DB } from './database.handler';
import { IManifest } from '../interfaces/manifest.interface';
import { ICallbackFn } from '../interfaces/callbacks.interface';
import { IRegisteredUser } from '../interfaces/registered_user.interface';
import { IGuild } from '../interfaces/guild.interface';
import * as QueryBuilder from './query.builder';
import * as Misc from './misc.handler';
import { IDestinyUserBroadcast } from '../interfaces/broadcast.interface';
import { IClan } from '../interfaces/clan.interface';
import { GroupV2 } from 'bungie-api-ts/groupv2';
import { DestinyRecordDefinition } from 'bungie-api-ts/destiny2';

export const addLog = (data: any, callback: ICallbackFn) => {
  const queryString = QueryBuilder.insertQuery(
    {
      location: data.location,
      type: data.type,
      log: data.log,
    },
    'logs'
  );
  return query(queryString, callback);
};

export const query = async (query: string, callback: ICallbackFn, data?: any, log?: boolean) => {
  try {
    if (log) {
      console.log(query, data ? data : '');
    }
    await DB.query(query, data ? data : undefined).then((result) => {
      if (result?.rows.length > 0) {
        callback(false, true, result.rows);
      } else {
        callback(false, false, null);
      }
    });
  } catch (err) {
    console.log(query, data ? data : '');
    console.error(err);
    callback(true, false, err);
  }
};

// Query Columns
const DestinyUserLite = [
  'clan_id',
  'display_name',
  'membership_id',
  'first_scan',
  'forced_scan',
  'private',
  'time_played',
  'clan_join_date',
  'last_played',
  'last_updated',
  'current_activity',
  'date_activity_started',
  'metrics',
  'progressions',
  'computed_data',
];

// Gets
export const getManifestVersion = (callback: ICallbackFn) => query(`SELECT * FROM manifest`, callback);
export const getSeason = (callback: ICallbackFn) =>
  query(`SELECT * FROM season WHERE NOW() > season.start and season.end > NOW()`, callback);
export const getGuild = (guild_id: string, callback: ICallbackFn) =>
  query(`SELECT * FROM guild WHERE guild_id = $1`, callback, [guild_id]);
export const getAllGuilds = (callback: ICallbackFn) => query(`SELECT * FROM guild`, callback);
export const getClan = (clan_id: string, callback: ICallbackFn) =>
  query(`SELECT * FROM clan WHERE clan_id = $1`, callback, [clan_id]);
export const getListOfGuildIds = (callback: ICallbackFn) =>
  query(`SELECT guild_id, is_tracking, guild_name FROM guild`, callback);
export const getGuildMembers = (clan_ids: string[] | number[], callback: ICallbackFn) =>
  query(`SELECT ${DestinyUserLite.join(',')} FROM destiny_user WHERE clan_id IN (${clan_ids})`, callback);
export const getSelectedClans = (clan_ids: string[] | number[], callback: ICallbackFn) =>
  query(`SELECT * FROM clan WHERE clan_id IN (${clan_ids})`, callback);
export const getSelectedGuilds = (guild_ids: string[] | number[], callback: ICallbackFn) =>
  query(`SELECT * FROM guild WHERE cast(guild_id as bigint) IN (${guild_ids.join(',')})`, callback);
export const getAllTrackedCollectibles = (callback: ICallbackFn) =>
  query(`SELECT * FROM tracked_collectibles`, callback);
export const getFullGuild = (guild_id: string, callback: ICallbackFn) => {
  query(
    `SELECT * FROM guild WHERE guild_id = $1`,
    async (isError, isFound, guilds) => {
      if (!isError && isFound) {
        callback(false, true, guilds);
      } else {
        callback(isError, isFound, guilds);
      }
    },
    [guild_id]
  );
};
export const getAllGuildsForClanId = (clan_id: number, callback: ICallbackFn) => {
  query(
    `SELECT * FROM guild WHERE "clans" IN ($1)`,
    async (isError, isFound, guilds) => {
      if (!isError && isFound) {
        callback(false, true, guilds);
      } else {
        callback(isError, isFound, guilds);
      }
    },
    [clan_id]
  );
};
export const getAllBroadcasts = (callback: ICallbackFn) =>
  query(`SELECT * FROM user_broadcasts WHERE was_announced = false`, callback);
export const getDestinyNamesFromIds = (player_ids: string[], callback: ICallbackFn) =>
  query(
    `SELECT membership_id, display_name FROM destiny_user WHERE membership_id IN (${player_ids.join(',')})`,
    callback
  );
export const getClanCount = (callback: ICallbackFn) =>
  query(`SELECT clan_id, clan_name, patreon, is_tracking FROM clan`, callback);
export const getRegisteredUsersCount = (callback: ICallbackFn) =>
  query(`SELECT count(*) FROM registered_users`, callback);
export const getPlayerCount = (callback: ICallbackFn) =>
  query(`SELECT sum(members_online) FROM clan where is_tracking = true`, callback);
export const getScanTime = (callback: ICallbackFn) =>
  query(
    `
  (SELECT 
    TO_CHAR(age(now() at time zone 'UTC', last_scan), 'HH24:MI:SS') as time_passed
      FROM clan
      WHERE is_tracking = true
      AND patreon = false
      ORDER BY last_scan
      LIMIT 1)
  union all 
  (SELECT 
    TO_CHAR(age(now() at time zone 'UTC', last_scan), 'HH24:MI:SS') as time_passed
      FROM clan
      WHERE is_tracking = true
      AND patreon = true
      ORDER BY last_scan
      LIMIT 1)`,
    callback
  );
export const getPlayerbaseActivity = (callback: ICallbackFn) =>
  query(
    `
  select count(*)::text, 1 as ResultSet from destiny_user WHERE last_updated > current_date - interval '1 days'
  UNION ALL
  select count(*)::text, 2 as ResultSet from destiny_user WHERE last_updated > current_date - interval '3 days'
  UNION ALL
  select count(*)::text, 3 as ResultSet from destiny_user WHERE last_updated > current_date - interval '7 days'
  UNION ALL
  select count(*)::text, 4 as ResultSet from destiny_user WHERE last_updated > current_date - interval '14 days'
  UNION ALL
  select count(*)::text, 5 as ResultSet from destiny_user WHERE last_updated > current_date - interval '31 days'
  order by ResultSet;
`,
    callback
  );

// Guilds
export const getGuildMembersItem = async (
  player_ids: string[],
  hash: number | string,
  type: string,
  callback: ICallbackFn
) =>
  await query(
    `SELECT ${DestinyUserLite.join(',')} FROM destiny_user WHERE membership_id in (${player_ids.join(',')}) AND ${
      type === 'missing' ? 'NOT' : ''
    } items @> '[${hash}]';`,
    callback
  );
export const getGuildMembersRecordState = async (player_ids: string[], hash: number | string, callback: ICallbackFn) =>
  await query(
    `SELECT ${DestinyUserLite.join(
      ','
    )}, records->'${hash}'->'State' as record_state FROM destiny_user WHERE membership_id in (${player_ids.join(
      ','
    )});`,
    callback
  );

// Singles
export const getRegisteredUserById = (discord_id, callback: ICallbackFn) =>
  query(`SELECT * FROM registered_users WHERE user_id = $1`, callback, [discord_id]);
export const getLiteUserById = (membership_id, callback: ICallbackFn) =>
  query(`SELECT ${DestinyUserLite.join(',')} FROM destiny_user WHERE membership_id = $1`, callback, [membership_id]);
export const getUserBroadcasts = (membership_id, callback: ICallbackFn) =>
  query(`SELECT * FROM user_broadcasts WHERE membership_id = $1`, callback, [membership_id]);

// Add
export const addGuild = (guild: Guild, callback: ICallbackFn, is_tracking?: boolean) => {
  return query(
    QueryBuilder.createUpsertQuery(
      {
        guild_id: guild.id,
        guild_name: guild?.name?.split("'")?.join("''") || 'Unknown - No name',
        is_tracking,
      },
      'guild',
      'guild_id'
    ),
    (isError, isFound, data) => {
      callback(isError, isFound, data);
    }
  );
};
export const addClan = (interaction: CommandInteraction, clan: GroupV2, callback: ICallbackFn) => {
  getClan(clan.groupId, (isError, isFound, data) => {
    if (!isError) {
      if (!isFound) {
        const queryString = QueryBuilder.insertQuery(
          {
            clan_id: clan.groupId,
            guild_id: interaction.guildId,
            channel_id: interaction.channelId,
          },
          'clans_to_scan'
        );
        return query(queryString, callback);
      } else {
        callback(isError, isFound, data);
      }
    } else {
      callback(isError, isFound, data);
    }
  });
};
export const addRegisteredUser = (user: IRegisteredUser, callback: ICallbackFn) => {
  const queryString = QueryBuilder.createUpsertQuery(user, 'registered_users', 'user_id');
  return query(queryString, callback);
};
export const addSystemLog = (data: any, callback: ICallbackFn) => {
  const queryString = QueryBuilder.insertQuery(data, 'system_logs');
  return query(queryString, callback);
};

// Updates
export const updateManifestVersion = (manifest: IManifest, callback: ICallbackFn) => {
  const data = { version: manifest.version };
  const where = { column: 'name', data: 'Version' };
  const queryString = QueryBuilder.createUpdateQuery(data, 'manifest', where);

  return query(queryString, callback);
};
export const updateGuildTracking = (guild: Guild, enabled: boolean, callback: ICallbackFn) => {
  getGuild(guild.id, (isError, isFound, guilds: IGuild[]) => {
    if (!isError) {
      if (isFound) {
        guilds[0].is_tracking = enabled;
        guilds[0].joined_on = undefined;
        updateGuildByID(Misc.removeNulls(guilds[0]), () => {
          callback(isError, isFound, guilds);
        });

        if (!enabled) {
          // If disabling the guild, check if clans are being tracked elsewhere before disabling.
          if (guilds[0].clans) {
            for (let clan_id of guilds[0].clans) {
              getAllGuildsForClanId(clan_id, (isError, isFound, data) => {
                if (!isError) {
                  if (!isFound) {
                    updateClanTracking(clan_id, false, (isError, isFound, data) => {
                      if (isError) {
                        callback(isError, isFound, data);
                      }
                    });
                  }
                } else {
                  callback(isError, isFound, data);
                }
              });
            }
          }
        } else {
          // If re-enabling the guild, check if clans are being tracked elsewhere before enabling.
          if (guilds[0].clans) {
            for (let clan_id of guilds[0].clans) {
              query(
                `SELECT * FROM clan WHERE clan_id = $1 AND is_tracking = $2`,
                (isError, isFound, clan) => {
                  if (!isError) {
                    if (isFound) {
                      updateClanTracking(clan_id, true, (isError, isFound, data) => {
                        if (isError) {
                          callback(isError, isFound, data);
                        }
                      });
                    }
                  } else {
                    callback(isError, isFound, clan);
                  }
                },
                [clan_id, false]
              );
            }
          }
        }

        callback(false, true, guilds[0]);
      } else {
        addGuild(guild, callback, enabled);
      }
    } else {
      callback(isError, isFound, guilds);
    }
  });
};
export const updateClanTracking = (clan_id: number, is_tracking: boolean, callback: ICallbackFn) => {
  const data = {
    is_tracking,
    last_scan: new Date().toISOString(),
  };
  const where = { column: 'clan_id', data: clan_id };
  const queryString = QueryBuilder.createUpdateQuery(data, 'clan', where);

  return query(queryString, callback);
};
export const updateGuildByID = (guild: IGuild, callback: ICallbackFn) => {
  const where = { column: 'guild_id', data: guild.guild_id };
  delete guild.broadcasts_config;
  delete guild.announcements_config;
  const queryString = QueryBuilder.createUpdateQuery(guild, 'guild', where);

  return query(queryString, callback, undefined, true);
};
export const updateUserBroadcast = (broadcast: IDestinyUserBroadcast, callback: ICallbackFn) => {
  return query(
    `
    UPDATE
      user_broadcasts
    SET
      was_announced = true
    WHERE
      guild_id='${broadcast.guild_id}' AND
      membership_id='${broadcast.membership_id}' AND
      type='${broadcast.type}' AND
      clan_id='${broadcast.clan_id}' AND
      hash='${broadcast.hash}'
  `,
    callback
  );
};

// Broadcast settings
export const updateGuildBroadcasts = (guild: IGuild, guildId: string, callback: ICallbackFn) => {
  query(`UPDATE guild SET broadcasts_config = $1 WHERE guild_id='${guildId}'`, callback, [guild.broadcasts_config]);
};

// Announcement settings
export const updateGuildAnnouncements = (guild: IGuild, guildId: string, callback: ICallbackFn) => {
  query(`UPDATE guild SET announcements_config = $1 WHERE guild_id='${guildId}'`, callback, [
    guild.announcements_config,
  ]);
};

// Vendors
export const getVendor = (vendor: string, callback: ICallbackFn) =>
  query(`SELECT * FROM vendors WHERE vendor = '${vendor}' ORDER BY date_added DESC LIMIT 1`, callback);

export const addDailyMods = (
  data: { vendor: string; additional_data: object[]; location: number; next_refresh_date: string },
  callback: ICallbackFn
) => {
  query(QueryBuilder.insertQuery(data, 'vendors'), (isError, isFound, data) => {
    callback(isError, isFound, data);
  }).catch();
};

// Grandmasters
export const getUserGrandmasters = async (
  membershipId: string,
  grandmasterRecords: DestinyRecordDefinition[],
  callback: ICallbackFn
) => {
  let getRecordsQuery = 'SELECT';
  grandmasterRecords.map((record, index, values) => {
    let recordHash = record.hash;
    let objectiveHash = record.objectiveHashes[0];
    if (index === values.length - 1) {
      getRecordsQuery += ` records -> '${recordHash}' -> 'Objectives' -> '${objectiveHash}' -> 'Progress' as h${recordHash}`;
    } else {
      getRecordsQuery += ` records -> '${recordHash}' -> 'Objectives' -> '${objectiveHash}' -> 'Progress' as h${recordHash},`;
    }
  });
  getRecordsQuery += ` FROM destiny_user WHERE membership_id = ${membershipId}`;

  await query(getRecordsQuery, callback);
};
