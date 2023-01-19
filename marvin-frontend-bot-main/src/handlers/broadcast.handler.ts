import { Client, Interaction } from 'discord.js';
import { IClan } from '../interfaces/clan.interface';
import { IDestinyUserLite } from '../interfaces/destiny_user.interface';
import { IGuild, IGuildBroadcasts } from '../interfaces/guild.interface';
import { alertEmbed, primaryEmbed } from './embed.handler';
import { ErrorHandler } from './error.handler';
import * as DatabaseFunctions from './database.functions';
import * as MiscHandler from './misc.handler';
import * as ManifestHandler from './manifest.handler';
import * as LogHandler from './log.handler';
import { IDestinyUserBroadcast, BroadcastType } from '../interfaces/broadcast.interface';

interface IBroadcastsGroupedByHash {
  [key: string]: IBroadcastsGroupedByClan;
}
interface IBroadcastsGroupedByClan {
  [key: string]: IDestinyUserBroadcast[];
}

export async function handle(client: Client) {
  const broadcasts = await fetchBroadcasts();
  if (Object.keys(broadcasts).length > 0) {
    Object.keys(broadcasts).forEach((guild_id) => {
      Object.keys(broadcasts[guild_id]).forEach((type) => {
        buildEmbed(client, broadcasts[guild_id][type], type);
      });
    });
  }
}

const buildEmbed = (client: Client, broadcastsGroupedByHash: IBroadcastsGroupedByHash[], type: string) => {
  switch (Number(type)) {
    case BroadcastType.Title: {
      try {
        buildTitleBroadcasts(client, broadcastsGroupedByHash);
      } catch (err) {
        console.log(err);
      }
      break;
    }
    case BroadcastType.GildedTitle: {
      try {
        buildGildedTitleBroadcasts(client, broadcastsGroupedByHash);
      } catch (err) {
        console.log(err);
      }
      break;
    }
    case BroadcastType.Triumph: {
      try {
        buildTriumphBroadcasts(client, broadcastsGroupedByHash);
      } catch (err) {
        console.log(err);
      }
      break;
    }
    case BroadcastType.Collectible: {
      try {
        buildCollectibleBroadcasts(client, broadcastsGroupedByHash);
      } catch (err) {
        console.log(err);
      }
      break;
    }
  }
};

const buildTitleBroadcasts = (client: Client, broadcastsGroupedByHash: IBroadcastsGroupedByHash[]) => {
  Object.keys(broadcastsGroupedByHash).forEach((hash) => {
    const embed = alertEmbed().setTitle(`Clan Broadcast`);
    const titleData = ManifestHandler.getRecord(hash);
    const parentData = ManifestHandler.searchPresentationNodeThatIsLinkedToRecord(titleData.hash);
    const clan_ids = Object.keys(broadcastsGroupedByHash[hash]);

    if (parentData?.[0]?.displayProperties?.icon) {
      embed.setThumbnail(`https://bungie.net${parentData[0].displayProperties.icon}`);
    } else if (titleData.displayProperties?.icon) {
      embed.setThumbnail(`https://bungie.net${titleData.displayProperties.icon}`);
    }

    if (clan_ids.length === 1) {
      const clan_broadcasts: IDestinyUserBroadcast[] = broadcastsGroupedByHash[hash][clan_ids[0]];
      if (clan_broadcasts.length === 1) {
        embed.setTitle(`Clan Broadcast - ${clan_broadcasts[0].additional_data.clan_name}`);
        embed.setDescription(
          `${clan_broadcasts[0].additional_data.display_name} has obtained title: **${titleData.titleInfo.titlesByGender['Male']}**`
        );
      } else {
        embed.setDescription(
          `${clan_broadcasts.length} people have obtained title: **${titleData.titleInfo.titlesByGender['Male']}**`
        );
        embed.addField(
          clan_broadcasts[0].additional_data.clan_name,
          clan_broadcasts.map((e) => e.additional_data.display_name).join('\n')
        );
      }
    } else {
      let all_clan_broadcasts: IDestinyUserBroadcast[] = [];
      Object.keys(broadcastsGroupedByHash[hash]).forEach((clan_id) =>
        broadcastsGroupedByHash[hash][clan_id].forEach((broadcast) => all_clan_broadcasts.push(broadcast))
      );
      embed.setDescription(
        `${all_clan_broadcasts.length} people have obtained title: **${titleData.titleInfo.titlesByGender['Male']}**`
      );

      for (let clan_id of clan_ids) {
        const clan_broadcasts: IDestinyUserBroadcast[] = broadcastsGroupedByHash[hash][clan_id];
        embed.addField(
          clan_broadcasts[0].additional_data.clan_name,
          clan_broadcasts.map((e) => e.additional_data.display_name).join('\n')
        );
      }
    }

    const guild = client.guilds.cache.get(broadcastsGroupedByHash[hash][clan_ids[0]][0].guild_id);
    const targetChannel = guild.channels.cache.get(
      broadcastsGroupedByHash[hash][clan_ids[0]][0].additional_data.broadcast_settings.channel_id
    );
    try {
      (targetChannel as any).send({
        embeds: [embed],
      });
    } catch (err) {
      LogHandler.SaveLog(
        'Frontend',
        'error',
        `Failed to send broadcast due to not have channel access. GuildID: ${
          broadcastsGroupedByHash[hash][clan_ids[0]][0].guild_id
        }, ChannelID: ${broadcastsGroupedByHash[hash][clan_ids[0]][0].additional_data.broadcast_settings.channel_id}`
      );
    }
  });

  // Mark as sent
  Object.keys(broadcastsGroupedByHash).forEach((hash) => {
    Object.keys(broadcastsGroupedByHash[hash]).forEach((clan) => {
      Object.keys(broadcastsGroupedByHash[hash][clan]).forEach((broadcast_index) => {
        const broadcast = broadcastsGroupedByHash[hash][clan][broadcast_index];
        DatabaseFunctions.updateUserBroadcast(broadcast, (isError, severity, err) => {
          if (isError) ErrorHandler(severity, err);
        });
      });
    });
  });
};
const buildGildedTitleBroadcasts = (client: Client, broadcastsGroupedByHash: IBroadcastsGroupedByHash[]) => {
  Object.keys(broadcastsGroupedByHash).forEach((hash) => {
    const embed = alertEmbed().setTitle(`Clan Broadcast`);
    const titleData = ManifestHandler.getRecord(hash);
    const clan_ids = Object.keys(broadcastsGroupedByHash[hash]);
    const parentHash = broadcastsGroupedByHash[hash][clan_ids[0]][0]?.additional_data?.parentTitleHash;
    const parentData = ManifestHandler.getRecord(parentHash);

    if (parentData?.displayProperties?.icon) {
      embed.setThumbnail(`https://bungie.net${parentData.displayProperties.icon}`);
    } else if (titleData.displayProperties?.icon) {
      embed.setThumbnail(`https://bungie.net${titleData.displayProperties.icon}`);
    }

    if (clan_ids.length === 1) {
      const clan_broadcasts: IDestinyUserBroadcast[] = broadcastsGroupedByHash[hash][clan_ids[0]];
      if (clan_broadcasts.length === 1) {
        embed.setTitle(`Clan Broadcast - ${clan_broadcasts[0].additional_data.clan_name}`);
        embed.setDescription(
          `${clan_broadcasts[0].additional_data.display_name} has gilded the **${titleData.titleInfo.titlesByGender['Male']}** title ${clan_broadcasts[0].additional_data.gildedCount} times!`
        );
      } else {
        embed.setDescription(
          `${clan_broadcasts.length} people have gilded the **${titleData.titleInfo.titlesByGender['Male']}** title!`
        );
        embed.addField(
          clan_broadcasts[0].additional_data.clan_name,
          clan_broadcasts
            .map((e) => `${e.additional_data.display_name} - ${e.additional_data.gildedCount} times.`)
            .join('\n')
        );
      }
    } else {
      let all_clan_broadcasts: IDestinyUserBroadcast[] = [];
      Object.keys(broadcastsGroupedByHash[hash]).forEach((clan_id) =>
        broadcastsGroupedByHash[hash][clan_id].forEach((broadcast) => all_clan_broadcasts.push(broadcast))
      );
      embed.setDescription(
        `${all_clan_broadcasts.length} people have gilded the **${titleData.titleInfo.titlesByGender['Male']}** title!`
      );

      for (let clan_id of clan_ids) {
        const clan_broadcasts: IDestinyUserBroadcast[] = broadcastsGroupedByHash[hash][clan_id];
        embed.addField(
          clan_broadcasts[0].additional_data.clan_name,
          clan_broadcasts
            .map((e) => `${e.additional_data.display_name} - ${e.additional_data.gildedCount} times.`)
            .join('\n')
        );
      }
    }

    const guild = client.guilds.cache.get(broadcastsGroupedByHash[hash][clan_ids[0]][0].guild_id);
    const targetChannel = guild.channels.cache.get(
      broadcastsGroupedByHash[hash][clan_ids[0]][0].additional_data.broadcast_settings.channel_id
    );
    try {
      (targetChannel as any).send({
        embeds: [embed],
      });
    } catch (err) {
      LogHandler.SaveLog(
        'Frontend',
        'error',
        `Failed to send broadcast due to not have channel access. GuildID: ${
          broadcastsGroupedByHash[hash][clan_ids[0]][0].guild_id
        }, ChannelID: ${broadcastsGroupedByHash[hash][clan_ids[0]][0].additional_data.broadcast_settings.channel_id}`
      );
    }
  });

  // Mark as sent
  Object.keys(broadcastsGroupedByHash).forEach((hash) => {
    Object.keys(broadcastsGroupedByHash[hash]).forEach((clan) => {
      Object.keys(broadcastsGroupedByHash[hash][clan]).forEach((broadcast_index) => {
        const broadcast = broadcastsGroupedByHash[hash][clan][broadcast_index];
        DatabaseFunctions.updateUserBroadcast(broadcast, (isError, severity, err) => {
          if (isError) ErrorHandler(severity, err);
        });
      });
    });
  });
};
const buildTriumphBroadcasts = (client: Client, broadcastsGroupedByHash: IBroadcastsGroupedByHash[]) => {
  Object.keys(broadcastsGroupedByHash).forEach((hash) => {
    const embed = alertEmbed().setTitle(`Clan Broadcast`);
    const triumphData = ManifestHandler.getRecord(hash);
    const clan_ids = Object.keys(broadcastsGroupedByHash[hash]);

    if (triumphData.displayProperties?.icon) {
      embed.setThumbnail(`https://bungie.net${triumphData.displayProperties.icon}`);
    }

    if (clan_ids.length === 1) {
      const clan_broadcasts: IDestinyUserBroadcast[] = broadcastsGroupedByHash[hash][clan_ids[0]];
      if (clan_broadcasts.length === 1) {
        embed.setTitle(`Clan Broadcast - ${clan_broadcasts[0].additional_data.clan_name}`);
        embed.setDescription(
          `${clan_broadcasts[0].additional_data.display_name} has completed the **${triumphData.displayProperties.name}** triumph!`
        );
      } else {
        embed.setDescription(
          `${clan_broadcasts.length} people have completed the **${triumphData.displayProperties.name}** triumph!`
        );
        embed.addField(
          clan_broadcasts[0].additional_data.clan_name,
          clan_broadcasts.map((e) => e.additional_data.display_name).join('\n')
        );
      }
    } else {
      let all_clan_broadcasts: IDestinyUserBroadcast[] = [];
      Object.keys(broadcastsGroupedByHash[hash]).forEach((clan_id) =>
        broadcastsGroupedByHash[hash][clan_id].forEach((broadcast) => all_clan_broadcasts.push(broadcast))
      );
      embed.setDescription(
        `${all_clan_broadcasts.length} people have completed the **${triumphData.displayProperties.name}** triumph!`
      );

      for (let clan_id of clan_ids) {
        const clan_broadcasts: IDestinyUserBroadcast[] = broadcastsGroupedByHash[hash][clan_id];
        embed.addField(
          clan_broadcasts[0].additional_data.clan_name,
          clan_broadcasts.map((e) => e.additional_data.display_name).join('\n')
        );
      }
    }

    embed.addField('How to complete:', triumphData.displayProperties.description);

    const guild = client.guilds.cache.get(broadcastsGroupedByHash[hash][clan_ids[0]][0].guild_id);
    const targetChannel = guild.channels.cache.get(
      broadcastsGroupedByHash[hash][clan_ids[0]][0].additional_data.broadcast_settings.channel_id
    );

    try {
      (targetChannel as any).send({
        embeds: [embed],
      });
    } catch (err) {
      LogHandler.SaveLog(
        'Frontend',
        'error',
        `Failed to send broadcast due to not have channel access. GuildID: ${
          broadcastsGroupedByHash[hash][clan_ids[0]][0].guild_id
        }, ChannelID: ${broadcastsGroupedByHash[hash][clan_ids[0]][0].additional_data.broadcast_settings.channel_id}`
      );
    }
  });

  // Mark as sent
  Object.keys(broadcastsGroupedByHash).forEach((hash) => {
    Object.keys(broadcastsGroupedByHash[hash]).forEach((clan) => {
      Object.keys(broadcastsGroupedByHash[hash][clan]).forEach((broadcast_index) => {
        const broadcast = broadcastsGroupedByHash[hash][clan][broadcast_index];
        DatabaseFunctions.updateUserBroadcast(broadcast, (isError, severity, err) => {
          if (isError) ErrorHandler(severity, err);
        });
      });
    });
  });
};
const buildCollectibleBroadcasts = (client: Client, broadcastsGroupedByHash: IBroadcastsGroupedByHash[]) => {
  Object.keys(broadcastsGroupedByHash).forEach((hash) => {
    const embed = alertEmbed().setTitle(`Clan Broadcast`);
    const collectionData = ManifestHandler.getCollectible(hash);
    const clan_ids = Object.keys(broadcastsGroupedByHash[hash]);
    const lightGGLink = `https://www.light.gg/db/legend/collectibles/${hash}`;

    if (collectionData.displayProperties?.icon) {
      embed.setThumbnail(`https://bungie.net${collectionData.displayProperties.icon}`);
    }

    if (clan_ids.length === 1) {
      const clan_broadcasts: IDestinyUserBroadcast[] = broadcastsGroupedByHash[hash][clan_ids[0]];
      if (clan_broadcasts.length === 1) {
        let completionText = ` on their ${MiscHandler.addOrdinal(
          clan_broadcasts[0].additional_data?.completions
        )} clear`;

        if (clan_broadcasts[0].additional_data?.completions === 1) {
          completionText = ` on their ${MiscHandler.addOrdinal(
            clan_broadcasts[0].additional_data?.completions
          )} clear, that lucky bastard.`;
        }

        embed.setTitle(`Clan Broadcast - ${clan_broadcasts[0].additional_data.clan_name}`);
        embed.setDescription(
          `${clan_broadcasts[0].additional_data.display_name} has obtained [${
            collectionData.displayProperties.name
          }](${lightGGLink})${clan_broadcasts[0].additional_data?.completions ? completionText : ''}!`
        );
      } else {
        embed.setDescription(
          `${clan_broadcasts.length} people have obtained [${collectionData.displayProperties.name}](${lightGGLink})`
        );
        embed.addField(
          clan_broadcasts[0].additional_data.clan_name,
          clan_broadcasts
            .map((e) => {
              if (e.additional_data?.completions) {
                return `${e.additional_data.display_name} - ${MiscHandler.addOrdinal(
                  e.additional_data.completions
                )} clear!`;
              } else {
                return e.additional_data.display_name;
              }
            })
            .join('\n')
        );
      }
    } else {
      let all_clan_broadcasts: IDestinyUserBroadcast[] = [];
      Object.keys(broadcastsGroupedByHash[hash]).forEach((clan_id) =>
        broadcastsGroupedByHash[hash][clan_id].forEach((broadcast) => all_clan_broadcasts.push(broadcast))
      );
      embed.setDescription(
        `${all_clan_broadcasts.length} people have obtained [${collectionData.displayProperties.name}](${lightGGLink})`
      );

      for (let clan_id of clan_ids) {
        const clan_broadcasts: IDestinyUserBroadcast[] = broadcastsGroupedByHash[hash][clan_id];
        embed.addField(
          clan_broadcasts[0].additional_data.clan_name,
          clan_broadcasts
            .map((e) => {
              if (e.additional_data?.completions) {
                return `${e.additional_data.display_name} - ${MiscHandler.addOrdinal(
                  e.additional_data.completions
                )} clear!`;
              } else {
                return e.additional_data.display_name;
              }
            })
            .join('\n')
        );
      }
    }

    const guild = client.guilds.cache.get(broadcastsGroupedByHash[hash][clan_ids[0]][0].guild_id);
    const targetChannel = guild.channels.cache.get(
      broadcastsGroupedByHash[hash][clan_ids[0]][0].additional_data.broadcast_settings.channel_id
    );
    try {
      (targetChannel as any).send({
        embeds: [embed],
      });
    } catch (err) {
      LogHandler.SaveLog(
        'Frontend',
        'error',
        `Failed to send broadcast due to not have channel access. GuildID: ${
          broadcastsGroupedByHash[hash][clan_ids[0]][0].guild_id
        }, ChannelID: ${broadcastsGroupedByHash[hash][clan_ids[0]][0].additional_data.broadcast_settings.channel_id}`
      );
    }
  });

  // Mark as sent
  Object.keys(broadcastsGroupedByHash).forEach((hash) => {
    Object.keys(broadcastsGroupedByHash[hash]).forEach((clan) => {
      Object.keys(broadcastsGroupedByHash[hash][clan]).forEach((broadcast_index) => {
        const broadcast = broadcastsGroupedByHash[hash][clan][broadcast_index];
        DatabaseFunctions.updateUserBroadcast(broadcast, (isError, severity, err) => {
          if (isError) ErrorHandler(severity, err);
        });
      });
    });
  });
};

export const sendCustomTelstoBroadcast = (client: Client, int: Interaction) => {
    const embed = alertEmbed().setTitle(`Telesto Broadcast`);
    const hash = "1642951319";
    const collectionData = ManifestHandler.getCollectible(hash);
    const lightGGLink = `https://www.light.gg/db/legend/collectibles/${hash}`;

    if (collectionData.displayProperties?.icon) {
      embed.setThumbnail(`https://bungie.net${collectionData.displayProperties.icon}`);
    }

    embed.setTitle(`Telesto Broadcast`);
    embed.setDescription(
      `${int.user.username}#${int.user.discriminator} has obtained [${
        collectionData.displayProperties.name
      }](${lightGGLink})!`
    );

    const guild = client.guilds.cache.get(int.guildId)
    const targetChannel = guild.channels.cache.get(int.channelId);
    try {
      (targetChannel as any).send({
        embeds: [embed],
      });
    } catch (err) {
      LogHandler.SaveLog(
        'Frontend',
        'error',
        `Failed to send telesto broadcast due to not have channel access. GuildID: ${int.guildId}, ChannelID: ${int.channelId}`
      );
    }
};

const fetchBroadcasts = () =>
  new Promise((resolve, reject) =>
    DatabaseFunctions.getAllBroadcasts(async function getAllBroadcasts(
      isError,
      isFound,
      broadcasts: IDestinyUserBroadcast[]
    ) {
      if (!isError && isFound) {
        if (broadcasts.length > 0) {
          const guilds = (await getGuildData([...new Set(broadcasts.map((e) => e.guild_id))])) as IGuild[];
          const clans = (await getClanData([...new Set(broadcasts.map((e) => e.clan_id))])) as IClan[];
          const users = (await getDestinyNames([
            ...new Set(broadcasts.map((e) => e.membership_id)),
          ])) as Partial<IDestinyUserLite>[];
          let groupedBroadcasts = MiscHandler.groupByKey(broadcasts, 'guild_id');

          Object.keys(groupedBroadcasts).forEach((guild_id) => {
            const guild = guilds.find((e) => e.guild_id === guild_id);
            if (guild && guild.broadcasts_config.is_broadcasting) {
              groupedBroadcasts[guild_id] = MiscHandler.groupByKey(groupedBroadcasts[guild_id], 'type');

              Object.keys(groupedBroadcasts[guild_id]).forEach((type) => {
                groupedBroadcasts[guild_id][type] = MiscHandler.groupByKey(groupedBroadcasts[guild_id][type], 'hash');

                Object.keys(groupedBroadcasts[guild_id][type]).forEach((hash) => {
                  groupedBroadcasts[guild_id][type][hash] = MiscHandler.groupByKey(
                    groupedBroadcasts[guild_id][type][hash],
                    'clan_id'
                  );

                  Object.keys(groupedBroadcasts[guild_id][type][hash]).forEach((clan_id) => {
                    if (clans) {
                      let clan_data = clans.find((e) => e?.clan_id === Number(clan_id));

                      if (clan_data) {
                        Object.keys(groupedBroadcasts[guild_id][type][hash][clan_id]).forEach((broadcast) => {
                          let user_data = users.find(
                            (e) =>
                              e.membership_id ===
                              groupedBroadcasts[guild_id][type][hash][clan_id][broadcast].membership_id
                          );

                          // GOTCHA: Declare field if null
                          if (!groupedBroadcasts[guild_id][type][hash][clan_id][broadcast].additional_data) {
                            groupedBroadcasts[guild_id][type][hash][clan_id][broadcast].additional_data = {};
                          }

                          groupedBroadcasts[guild_id][type][hash][clan_id][broadcast].additional_data[
                            'broadcast_settings'
                          ] = guild.broadcasts_config;
                          groupedBroadcasts[guild_id][type][hash][clan_id][broadcast].additional_data['clan_name'] =
                            clan_data?.clan_name || 'Unknown';
                          groupedBroadcasts[guild_id][type][hash][clan_id][broadcast].additional_data['display_name'] =
                            user_data?.display_name || 'Unknown';
                        });
                      }
                    }
                  });
                });
              });
            }
          });

          resolve(groupedBroadcasts);
        } else {
          resolve([]);
        }
      } else {
        resolve([]);
      }
    })
  );

const getGuildData = (guild_ids) =>
  new Promise((resolve, reject) =>
    DatabaseFunctions.getSelectedGuilds(guild_ids, function getSelectedGuilds(isError, isFound, guilds: IGuild[]) {
      if (!isError && isFound) {
        resolve(guilds);
      } else {
        resolve(undefined);
      }
    })
  );

const getClanData = (clan_ids) =>
  new Promise((resolve, reject) =>
    DatabaseFunctions.getSelectedClans(clan_ids, function getSelectedClans(isError, isFound, clans: IClan[]) {
      if (!isError && isFound) {
        resolve(clans);
      } else {
        resolve(undefined);
      }
    })
  );

const getDestinyNames = (player_ids) =>
  new Promise((resolve, reject) =>
    DatabaseFunctions.getDestinyNamesFromIds(
      player_ids,
      function getDestinyNamesFromIds(isError, isFound, users: Partial<IDestinyUserLite>[]) {
        if (!isError && isFound) {
          resolve(users);
        } else {
          resolve(undefined);
        }
      }
    )
  );
