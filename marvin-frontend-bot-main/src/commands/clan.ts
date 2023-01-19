import DiscordJS, {
  CommandInteraction,
  GuildMember,
  MessageActionRow,
  MessageComponentInteraction,
  MessageEmbed,
  MessageSelectMenu,
  Permissions,
  SelectMenuInteraction,
} from 'discord.js';
import { ICommand as WokCommand } from 'wokcommands';
import { databaseErrorEmbed, errorEmbed, noPermissionEmbed, primaryEmbed } from '../handlers/embed.handler';
import { IGuild, IGuildCallback } from '../interfaces/guild.interface';
import { IClan } from '../interfaces/clan.interface';
import { IRegisteredUserCallback } from '../interfaces/registered_user.interface';
import { GetGroupsForMemberResponse, GroupResponse, ServerResponse } from 'bungie-api-ts/groupv2';
import { ErrorHandler } from '../handlers/error.handler';
import * as DatabaseFunctions from '../handlers/database.functions';
import * as APIHandler from '../handlers/api.handler';
import * as LogHandler from '../handlers/log.handler';

const WOKCommand: WokCommand = {
  category: 'clan',
  description: 'Setup or manage the clan(s) associated with this server',

  slash: true,
  testOnly: false,

  options: [
    {
      name: 'help',
      description: 'Helpful information to get you started.',
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.SUB_COMMAND,
    },
    {
      name: 'list',
      description: 'Show a list of clans linked to this server.',
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.SUB_COMMAND,
    },
    {
      name: 'setup',
      description: 'Link your clan to this server.',
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.SUB_COMMAND,
    },
    {
      name: 'info',
      description: 'See information about the clans on this server.',
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.SUB_COMMAND,
    },
    {
      name: 'activity',
      description: 'See what clannies are up to.',
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.SUB_COMMAND,
    },
    {
      name: 'add',
      description: 'Link another clan to this discord.',
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.SUB_COMMAND,
      options: [
        {
          name: 'id',
          description: 'Use `/clan help` to learn how to get this ID value for this command',
          required: true,
          type: DiscordJS.Constants.ApplicationCommandOptionTypes.NUMBER,
        },
      ],
    },
    {
      name: 'remove',
      description: 'Remove a linked clan from this discord server.',
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.SUB_COMMAND,
    },
  ],

  callback: async ({ interaction: msgInt, channel }) => {
    LogHandler.SaveInteractionLog(msgInt);
    if (!msgInt.isCommand()) return;

    const guildCallback: IGuildCallback = await new Promise((resolve) =>
      DatabaseFunctions.getGuild(msgInt.guild.id, (isError, isFound, data) => {
        resolve({ isError, isFound, data });
      })
    );
    const guild = guildCallback.data?.[0];

    if (guildCallback.isError) {
      return databaseErrorEmbed();
    }

    switch (msgInt.options.getSubcommand()) {
      case 'help': {
        await msgInt
          .reply({
            embeds: [helpEmbed(primaryEmbed())],
            ephemeral: false,
          })
          .catch((err) => LogHandler.SaveLog('Frontend', 'Error', 'Failed to send clan help interaction'));
        break;
      }
      case 'list': {
        await msgInt
          .reply({
            embeds: [await listEmbed(msgInt, guild)],
            ephemeral: false,
          })
          .catch((err) => LogHandler.SaveLog('Frontend', 'Error', 'Failed to send clan list interaction'));
        break;
      }
      case 'setup': {
        await msgInt
          .reply({
            embeds: [await setupEmbed(msgInt, guild)],
            ephemeral: false,
          })
          .catch((err) => LogHandler.SaveLog('Frontend', 'Error', 'Failed to send clan setup interaction'));
        break;
      }
      case 'add': {
        await msgInt
          .reply({
            embeds: [await addEmbed(msgInt, guild)],
            ephemeral: false,
          })
          .catch((err) => {
            console.log(err);
            LogHandler.SaveLog('Frontend', 'Error', 'Failed to send clan add interaction');
          });
        break;
      }
      case 'remove': {
        await removeEmbed(msgInt, guild);
        break;
      }
      default: {
        await msgInt
          .reply({
            embeds: [errorEmbed().setDescription('Command not yet implemented.')],
            ephemeral: true,
          })
          .catch((err) =>
            LogHandler.SaveLog('Frontend', 'Error', 'Failed to send clan command not implemented interaction')
          );
        break;
      }
    }

    const filter = (btnInt: MessageComponentInteraction) => {
      return msgInt.user.id === btnInt.user.id;
    };
    const collector = (channel as any).createMessageComponentCollector({
      filter,
      time: 1000 * 60 * 5,
    });

    collector.on('collect', async (int: MessageComponentInteraction) => {
      if (int) {
        try {
          switch (int.customId) {
            case 'clan_menu_select': {
              await updateClanList(msgInt, guild, (int as SelectMenuInteraction).values[0]);
              await int.update({}).catch();
              break;
            }
          }
        } catch (err) {
          console.error('Announcements message collector', err);
        }
      }
    });
  },
};

const helpEmbed = (embed: MessageEmbed) => {
  embed.setTitle('Clan - Help');

  const description = [];
  description.push("**Setup** - It's easy!");
  description.push('- First register by using the `/register` command.\n- Then once registered `/clan setup`\n');

  description.push('**Want more than one clan?**');
  description.push('Todo\n');

  description.push('**Manage**');
  description.push('Todo\n');

  description.push('**Remove**');
  description.push('Todo\n');

  embed.setDescription(description.join('\n'));
  return embed;
};

const listEmbed = async (interaction: CommandInteraction, guild: IGuild): Promise<MessageEmbed> => {
  let descriptions = [
    'To add another clan use: `/add clan`\n\nTo remove a tracked clan, use the ID associated with the clan.\nExample: `/clan remove 123456`\n',
  ];

  if (!guild.clans) {
    let embed = errorEmbed();
    embed.setTitle('This server has no linked clans');
    embed.setDescription('To link a clan use `/clan setup`');
    return primaryEmbed().setTitle('Linked Clans').setDescription(descriptions.join('\n'));
  }

  return await new Promise((resolve) =>
    DatabaseFunctions.getSelectedClans(guild.clans.toString().split(','), (isError, isFound, clans: IClan[]) => {
      if (!isError) {
        if (isFound) {
          guild.clans.forEach((clan_id) => {
            let clan_data = clans.find((e) => e?.clan_id === clan_id);
            descriptions.push(
              `ID: \`${clan_id}\` - Name: \`${clan_data ? clan_data.clan_name : 'Still scanning...'}\``
            );
          });
        } else {
          resolve(primaryEmbed().setDescription('Still scanning, please wait...'));
        }
      } else {
        resolve(databaseErrorEmbed());
      }
      resolve(primaryEmbed().setTitle('Linked Clans').setDescription(descriptions.join('\n')));
    })
  );
};

const setupEmbed = async (interaction: CommandInteraction, guild: IGuild): Promise<MessageEmbed> => {
  const userCallback: IRegisteredUserCallback = await new Promise((resolve) =>
    DatabaseFunctions.getRegisteredUserById(interaction.user.id, (isError, isFound, data) => {
      resolve({ isError, isFound, data });
    })
  );

  const user = userCallback.data?.[0];

  if (userCallback.isError) {
    return databaseErrorEmbed();
  }

  if (!userCallback.isFound) {
    let embed = errorEmbed();
    embed.setTitle('User not registered');
    embed.setDescription(
      'Please register first so that i know who you are in order to add your clan.\nUse: `/register`'
    );
    return embed;
  }

  if (guild.clans) {
    let embed = errorEmbed();
    embed.setTitle('This server already has a registered clan');
    embed.setDescription(
      'If you wish to add another to the server use `/clan add`\nIf you have changed clan use `/clan remove` first.'
    );
    return embed;
  }

  return await new Promise((resolve) =>
    APIHandler.GetClanFromMbmID(
      user.platform,
      user.membership_id,
      (isError: boolean, data: ServerResponse<GetGroupsForMemberResponse>) => {
        if (!isError) {
          if (data.Response.results.length > 0) {
            let clan = data.Response.results[0].group;
            delete guild.joined_on;
            guild.clans = [parseInt(clan.groupId)];
            guild.owner_id = interaction.user.id;
            guild.owner_avatar = interaction.user.avatar;
            DatabaseFunctions.updateGuildByID(guild, (isError, severity, err) => {
              if (isError) {
                ErrorHandler(severity, err);
                resolve(databaseErrorEmbed());
              } else {
                DatabaseFunctions.addClan(interaction, clan, (isError, severity, err) => {
                  if (isError) {
                    ErrorHandler(severity, err);
                    resolve(databaseErrorEmbed());
                  } else {
                    LogHandler.SaveLog('Frontend', 'Clans', `Clan Added: ${clan.name} (${clan.groupId})`);
                    resolve(
                      primaryEmbed()
                        .setTitle('Success')
                        .setDescription(
                          `${clan.name} has been successfully registered to this server! If this is the first time registering it may take a few minutes to grab your clans data for the first time.`
                        )
                    );
                  }
                });
              }
            });
          } else {
            resolve(
              errorEmbed().setDescription(
                'So you are apparently not in a clan? Was there a mistake in registering your username?'
              )
            );
          }
        } else {
          ErrorHandler('Low', data);
          resolve(errorEmbed().setDescription('There was an error when trying to get clan data. Try again?'));
        }
      }
    )
  );
};

const addEmbed = async (interaction: CommandInteraction, guild: IGuild): Promise<MessageEmbed> => {
  const clan_id = interaction.options.getNumber('id');
  const userCallback: IRegisteredUserCallback = await new Promise((resolve) =>
    DatabaseFunctions.getRegisteredUserById(interaction.user.id, (isError, isFound, data) => {
      resolve({ isError, isFound, data });
    })
  );

  const user = userCallback.data?.[0];

  if (userCallback.isError) {
    return databaseErrorEmbed();
  }

  if (!userCallback.isFound) {
    let embed = errorEmbed();
    embed.setTitle('User not registered');
    embed.setDescription(
      'Please register first so that i know who you are in order to add your clan.\nUse: `/register`'
    );
    return embed;
  }

  if (!guild.clans) {
    return setupEmbed(interaction, guild);
  }

  if (
    guild.owner_id === interaction.user.id ||
    (interaction.member as GuildMember)?.permissions.has([Permissions.FLAGS.ADMINISTRATOR])
  ) {
    return await new Promise((resolve) =>
      APIHandler.GetClan(clan_id, function GetClan(clan_id, isError, clan: ServerResponse<GroupResponse>) {
        if (!isError) {
          const clanData = clan.Response.detail;
          let clans = guild.clans;
          if (clanData.clanInfo) {
            if (!clans.includes(clan_id)) {
              clans.push(clan_id);
              guild.clans = clans;
              delete guild.joined_on;
              DatabaseFunctions.updateGuildByID(guild, (isError, severity, err) => {
                if (isError) {
                  ErrorHandler(severity, err);
                  resolve(databaseErrorEmbed());
                } else {
                  DatabaseFunctions.addClan(interaction, clanData, (isError, severity, err) => {
                    if (isError) {
                      ErrorHandler(severity, err);
                      resolve(databaseErrorEmbed());
                    } else {
                      LogHandler.SaveLog('Frontend', 'Clans', `Clan Added: ${clanData.name} (${clanData.groupId})`);
                      resolve(
                        primaryEmbed()
                          .setTitle('Success')
                          .setDescription(
                            `${clanData.name} has been succesfully added and will start to be tracked for this server! If this is the first time they've been scanned, it may take a few minutes to load the data for the first time. Please wait.`
                          )
                      );
                    }
                  });
                }
              });
            } else {
              resolve(
                errorEmbed()
                  .setTitle(`Whatcha doing willis?`)
                  .setDescription(`${clanData.name} (${clanData.groupId}) is already being tracked by this server.`)
              );
            }
          } else {
            ErrorHandler('Low', `A clan was found, but it is not a Destiny 2 clan. Not added sorry.`);
            resolve(errorEmbed().setDescription(`A clan was found, but it is not a Destiny 2 clan. Not added sorry.`));
          }
        } else {
          ErrorHandler('High', clan);
          resolve(errorEmbed().setDescription(`Failed to find a clan with the ID: ${clan_id}`));
        }
      })
    );
  } else {
    return noPermissionEmbed();
  }
};

const removeEmbed = async (interaction: CommandInteraction, guild: IGuild) => {
  if (!guild.clans) {
    let embed = errorEmbed();
    embed.setTitle('This server has no linked clans');
    embed.setDescription('To link a clan use `/clan setup`');

    await interaction.reply({
      embeds: [embed],
    });
  }

  const clans: { name: string; id: number }[] = await new Promise((resolve) =>
    DatabaseFunctions.getSelectedClans(guild.clans.toString().split(','), (isError, isFound, clans: IClan[]) => {
      if (!isError) {
        if (isFound) {
          resolve(
            guild.clans.map((clan_id) => {
              let clan_data = clans.find((e) => e?.clan_id === clan_id);
              return { name: `${clan_data ? clan_data.clan_name : `${clan_id} (Still scanning)`}`, id: clan_id };
            })
          );
        } else {
          resolve([]);
        }
      } else {
        resolve([]);
      }
    })
  );

  if (!clans) {
    await interaction.reply({
      embeds: [databaseErrorEmbed()],
    });
  }

  const embed = primaryEmbed()
    .setTitle('Clan - Remove')
    .setDescription('To remove a clan from this server, select from the drop down. (can only display up to 25 clans)');
  const components = [];

  const clanMenuSelect = (): MessageActionRow =>
    new MessageActionRow().addComponents(
      new MessageSelectMenu().setCustomId('clan_menu_select').setOptions([
        ...clans.slice(0, 24).map((clan) => {
          return {
            label: clan.name,
            value: clan.id.toString(),
          };
        }),
      ])
    );

  components.push(clanMenuSelect());

  await interaction
    .reply({
      embeds: [embed],
      components: components,
      ephemeral: true,
    })
    .catch((err) => {
      console.log(err);
      LogHandler.SaveLog('Frontend', 'Error', 'Failed to send clan remove interaction');
    });
};

const updateClanList = async (interaction: CommandInteraction, guild: IGuild, clan_id: string) => {
  guild.clans = guild.clans.filter((id) => id.toString() !== clan_id.toString());
  guild.clans = guild.clans.length === 0 ? null : guild.clans;
  delete guild.joined_on;
  DatabaseFunctions.updateGuildByID(guild, async (isError, severity, err) => {
    if (!isError) {
      LogHandler.SaveLog('Frontend', 'Clans', `Clan: (${clan_id}) was removed from guild ${interaction.guild.id}`);
      await interaction.editReply({
        embeds: [
          primaryEmbed()
            .setTitle('Success')
            .setDescription(`The clan has been succesfully un-linked from this server.`),
        ],
        components: [],
      });
    } else {
      ErrorHandler(severity, err);
      await interaction.editReply({
        embeds: [databaseErrorEmbed()],
        components: [],
      });
    }
  });
};

export default WOKCommand;
