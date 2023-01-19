import { VoluspaVendorResponse } from '../interfaces/braytech.interface';
import { ErrorHandler } from './error.handler';
import { primaryEmbed } from './embed.handler';
import { MessageAttachment } from 'discord.js';
import { IMod, IVendor } from '../interfaces/vendor.interface';
import * as APIHandler from './api.handler';
import * as CanvasHandler from './canvas.handler';
import * as ManifestHandler from './manifest.handler';
import * as DatabaseFunctions from './database.functions';
import { IGuild } from '../interfaces/guild.interface';
import { SaveLog } from './log.handler';

export function updateDailyVendors(client, ResetTime) {
  // Loop through vendors
  const vendors = [
    { name: 'Gunsmith', hash: '672118013' },
    { name: 'Ada-1', hash: '350061650' },
  ];
  for (let vendor of vendors) {
    const getVendor = (vendor) =>
      APIHandler.GetVendor(vendor.hash, async function (isError, ModData: VoluspaVendorResponse) {
        if (!isError && ModData?.Response?.sales?.data) {
          // Get mods and new refresh date.
          let refreshDate = ModData.Response.vendor.data.nextRefreshDate;
          const vendorLocation = ModData.Response.vendor.data?.vendorLocationIndex;
          const dailySales = ModData.Response.sales.data;
          const modsRaw = Object.values(dailySales).filter(
            (e) => ManifestHandler.getManifestItemByHash(e.itemHash)?.itemType === 19
          );
          const mods = Object.values(modsRaw).map((e) => {
            let mod = ManifestHandler.getManifestItemByHash(e.itemHash);
            if (e.overrideNextRefreshDate) {
              refreshDate = e.overrideNextRefreshDate;
            }
            return {
              name: mod.displayProperties.name,
              icon: mod.displayProperties.icon,
              description: mod.displayProperties.description,
              hash: mod.hash,
              collectibleHash: mod.collectibleHash,
            };
          });

          const vendorData = {
            vendor: vendor.name,
            additional_data: mods,
            location: vendorLocation,
            next_refresh_date: refreshDate,
          } as IVendor;

          // Only proceed if the reset times are different otherwise you're re-entering the duplicte data
          if (ResetTime !== refreshDate) {
            // Add new database entry.
            DatabaseFunctions.addDailyMods({ ...vendorData }, function addDailyMods(isError, isFound, data) {});

            // Send mod announcements
            sendDailyVendors(client, vendor, vendorData);
          } else {
            ErrorHandler('Med', `Tried to enter duplicate mod data for ${vendor.name}. Ignored.`);
          }
        } else {
          //If failed for some reason, set a timeout to retry and log error.
          ErrorHandler('Med', `Failed to update mods for ${vendor.name}, retrying in 60 seconds.`);
          setTimeout(() => {
            getVendor(vendor);
          }, 60000);
        }
      });

    if (!JSON.parse(process.env.TESTING)) {
      getVendor(vendor);
    }
  }
}

export function updateXurVendor(client, ResetTime) {
  const getXur = () =>
    DatabaseFunctions.getVendor('Xûr', async function (isError, isFound, lastVendorEntry) {
      const vendor = ManifestHandler.getManifest().DestinyVendorDefinition[2190858386];
      if (!isError && isFound) {
        // Check to make sure it's past the reset date, otherwise we don't want to store a new entry
        if ((new Date() > new Date(lastVendorEntry.nextRefreshDate) && new Date().getDay() === 5) || !ResetTime) {
          APIHandler.GetVendor(vendor.hash, async function (isError, ItemData: VoluspaVendorResponse) {
            if (!isError && ItemData?.Response?.sales?.data) {
              //Get items and new refresh date.
              let refreshDate = ItemData.Response.vendor.data.nextRefreshDate;
              const vendorLocation = ItemData.Response.vendor.data.vendorLocationIndex;
              const dailySales = ItemData.Response.sales.data;
              const itemRaw = Object.values(dailySales).filter(
                (e) =>
                  ManifestHandler.getManifestItemByHash(e.itemHash)?.inventory?.tierType === 6 &&
                  ManifestHandler.getManifestItemByHash(e.itemHash)?.collectibleHash
              );
              const items = Object.values(itemRaw).map((e) => {
                let item = ManifestHandler.getManifestItemByHash(e.itemHash);
                return {
                  name: item.displayProperties.name,
                  icon: item.displayProperties.icon,
                  description: item.displayProperties.description,
                  hash: item.hash,
                  collectibleHash: item.collectibleHash,
                  stats: ItemData.Response?.itemComponents?.stats?.data[e?.vendorItemIndex]?.stats,
                  itemType: item.itemType,
                };
              });

              const vendorData = {
                vendor: 'Xûr',
                additional_data: items,
                location: vendorLocation,
                next_refresh_date: refreshDate,
              } as IVendor;

              // Only proceed if the reset times are different otherwise you're re-entering the duplicte data
              if (ResetTime !== 0 && ResetTime !== refreshDate) {
                // Add new database entry.
                DatabaseFunctions.addDailyMods({ ...vendorData }, function addDailyMods(isError, isFound, data) {});

                // Send xur announcements
                sendXurAnnouncements(client, vendor, vendorData, vendorLocation);
              } else {
                // Check if this was a test or not, if it was a test, don't log error.
                if (ResetTime !== 0) {
                  ErrorHandler(
                    'Med',
                    `Tried to enter duplicate mod data for ${vendor.displayProperties.name}. Ignored.`
                  );
                }
              }
            } else {
              //If failed for some reason, set a timeout to retry and log error.
              ErrorHandler(
                'Med',
                `Failed to update mods for ${vendor.displayProperties.name}, retrying in 60 seconds.`
              );
              setTimeout(() => {
                getXur();
              }, 60000);
            }
          });
        }
      }
    });

  if (!JSON.parse(process.env.TESTING)) {
    getXur();
  }
}

async function sendDailyVendors(client, vendor, vendorData: IVendor) {
  const embed = primaryEmbed()
    .setTitle(`Vendor - ${vendor.name} - Daily Mods`)
    .setFooter({ text: 'Data provided by Braytech', iconURL: 'https://bray.tech/static/images/icons/icon-96.png' })
    .setTimestamp();

  const canvas = await CanvasHandler.buildModCanvasBuffer(vendor.name, vendorData);
  const attachment = new MessageAttachment(canvas, 'mods.png');
  embed.setImage('attachment://mods.png');

  var description = [];
  description.push(`To see who needs these mods use:`);

  (vendorData.additional_data as IMod[]).map((mod) => {
    description.push(`\`/item ${mod.name}\` \`{obtained : false}\``);
  });

  embed.setDescription(description.join('\n'));

  if (vendor.name === 'Ada-1') {
    try {
      client.guilds.cache
        .get('886500502060302357')
        .channels.cache.get('1019748411286761482')
        .send({
          embeds: [embed],
          files: [attachment],
        });
    } catch (err) {
      console.error(`Failed to send ada-1 mods broadcast to 886500502060302357 because of ${err}`);
    }
  }
  if (vendor.name === 'Gunsmith') {
    try {
      client.guilds.cache
        .get('886500502060302357')
        .channels.cache.get('1019748411286761482')
        .send({
          embeds: [embed],
          files: [attachment],
        });
    } catch (err) {
      console.error(`Failed to send gunsmith mods broadcast to 886500502060302357 because of ${err}`);
    }
  }

  // DatabaseFunctions.getAllGuilds((isError: boolean, isFound: boolean, guilds: IGuild[]) => {
  //   if (!isError && isFound) {
  //     for (let i in guilds) {
  //       let guild = guilds[i];

  //       var description = [];
  //       description.push(`To see who needs these mods use:`);

  //       (vendorData.additional_data as IMod[]).map((mod) => {
  //         description.push(`\`/item ${mod.name}\` \`{obtained : false}\``);
  //       });

  //       embed.setDescription(description.join('\n'));

  //       if (vendor.name === 'Ada-1') {
  //         if (guild.announcements_config.adas && guild.announcements_config.channel_id !== null) {
  //           try {
  //             client.guilds.cache
  //               .get(guild.guild_id)
  //               .channels.cache.get(guild.announcements_config.channel_id)
  //               .send({
  //                 embeds: [embed],
  //                 files: [attachment],
  //               });
  //           } catch (err) {
  //             console.log(`Failed to send ada-1 mods broadcast to ${guild.guild_id} because of ${err}`);
  //           }
  //         }
  //       }

  //       if (vendor.name === 'Gunsmith') {
  //         if (guild.announcements_config.gunsmiths && guild.announcements_config.channel_id !== null) {
  //           try {
  //             client.guilds.cache
  //               .get(guild.guild_id)
  //               .channels.cache.get(guild.announcements_config.channel_id)
  //               .send({
  //                 embeds: [embed],
  //                 files: [attachment],
  //               });
  //           } catch (err) {
  //             console.log(`Failed to send gunsmith mods broadcast to ${guild.guild_id} because of ${err}`);
  //           }
  //         }
  //       }
  //     }
  //   } else {
  //     SaveLog('Frontend', 'Error', 'Failed to get all guilds to send mods announcements');
  //   }
  // });
}

async function sendXurAnnouncements(client, vendor, vendorData: IVendor, vendorLocation) {
  const embed = primaryEmbed()
    .setFooter({ text: 'Data provided by Braytech', iconURL: 'https://bray.tech/static/images/icons/icon-96.png' })
    .setTimestamp();

  const { attachment, xurEmbed } = await CanvasHandler.buildXurCanvasBuffer(embed, vendorData, vendorLocation);

  try {
    client.guilds.cache
      .get('886500502060302357')
      .channels.cache.get('1019748411286761482')
      .send({
        embeds: [xurEmbed],
        files: [attachment],
      });
  } catch (err) {
    console.error('error posting xur announcement', err);
  }

  // DatabaseFunctions.getAllGuilds((isError: boolean, isFound: boolean, guilds: IGuild[]) => {
  //   if (!isError && isFound) {
  //     for (let guild of guilds) {
  //       if (guild.announcements_config.xur && guild.announcements_config.channel_id !== null) {
  //         try {
  //           client.guilds.cache
  //             .get(guild.guild_id)
  //             .channels.cache.get(guild.announcements_config.channel_id)
  //             .send({
  //               embeds: [xurEmbed],
  //               files: [attachment],
  //             });
  //         } catch (err) {
  //           console.log(`Failed to send xur announcement to ${guild.guild_id} because of ${err}`);
  //         }
  //       }
  //     }
  //   } else {
  //     SaveLog('Frontend', 'Error', 'Failed to get all guilds to send xur announcements');
  //   }
  // });
}
