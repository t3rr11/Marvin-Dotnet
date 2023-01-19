import DiscordJS, {
  Channel,
  CommandInteraction,
  MessageActionRow,
  MessageButton,
  MessageComponentInteraction,
  MessageSelectMenu,
  SelectMenuInteraction,
} from 'discord.js';
import {
  BroadcastSettingModes,
  CuratedBroadcastSettingModes,
  IGuild,
  IGuildCallback,
} from '../interfaces/guild.interface';
import { databaseErrorEmbed, errorEmbed, primaryEmbed } from '../handlers/embed.handler';
import { ICommand as WokCommand } from 'wokcommands';
import { ErrorHandler } from '../handlers/error.handler';
import * as DatabaseFunctions from '../handlers/database.functions';
import * as LogHandler from '../handlers/log.handler';

const WOKCommand: WokCommand = {
  category: 'broadcasts',
  description: 'Manage the server broadcasts.',

  slash: true,
  testOnly: false,

  options: [
    {
      name: 'settings',
      description: 'Manage the types of broadcasts that can be sent, or enable/disable them.',
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.SUB_COMMAND,
    },
    {
      name: 'channel',
      description: 'Set a channel where broadcasts will be sent (also enables them).',
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.SUB_COMMAND,
      options: [
        {
          name: 'channel',
          description: 'Pick a channel, you wish the broadcasts to be sent to.',
          required: true,
          type: DiscordJS.Constants.ApplicationCommandOptionTypes.CHANNEL,
          channelTypes: ['GUILD_TEXT'],
        },
      ],
    },
    {
      name: 'disable',
      description: 'Disable broadcasts for this server.',
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.SUB_COMMAND,
    },
  ],

  callback: async ({ interaction: msgInt, channel }) => {
    LogHandler.SaveInteractionLog(msgInt);
    if (!msgInt.isCommand()) return;
    await msgInt.deferReply({ ephemeral: true });

    const guild = await getGuild(msgInt);
    if (!guild) return;

    switch (msgInt.options.getSubcommand()) {
      case 'settings': {
        await settingsEmbed(msgInt, guild);
        break;
      }
      case 'channel': {
        const embed = primaryEmbed().setTitle('Broadcasts - Enabled');
        const channel = msgInt.options.getChannel('channel') as Channel;
        guild.broadcasts_config.is_broadcasting = true;
        guild.broadcasts_config.channel_id = channel.id;
        DatabaseFunctions.updateGuildBroadcasts(
          guild,
          msgInt.guild.id,
          function updateBroadcastsChannel(isError, severity, err) {
            if (isError) {
              ErrorHandler(severity, err);
              embed.setDescription(`There was an error trying to enable broadcasts. Please try again.`);
            } else {
              embed.setDescription(`Broadcasts will now be sent to <#${channel.id}>.`);
              LogHandler.SaveLog('Frontend', 'Info', `${msgInt.guild.id} has set the broadcasts channel.`);
            }
            msgInt
              .editReply({
                embeds: [embed],
              })
              .catch((err) =>
                LogHandler.SaveLog('Frontend', 'Error', 'Failed to edit reply for the broadcasts channel interaction')
              );
          }
        );
        break;
      }
      case 'disable': {
        const embed = primaryEmbed().setTitle('Broadcasts - Disabled');
        guild.broadcasts_config.is_broadcasting = false;
        DatabaseFunctions.updateGuildBroadcasts(
          guild,
          msgInt.guild.id,
          function disableBroadcasts(isError, severity, err) {
            if (isError) {
              ErrorHandler(severity, err);
              embed.setDescription(`There was an error trying to disable broadcasts. Please try again.`);
            } else {
              embed.setDescription(`Successfully disabled broadcasts, you will no longer recieve broadcasts.`);
              LogHandler.SaveLog('Frontend', 'Info', `${msgInt.guild.id} has disabled broadcasts.`);
            }
            msgInt
              .editReply({
                embeds: [embed],
              })
              .catch((err) =>
                LogHandler.SaveLog('Frontend', 'Error', 'Failed to edit reply for the broadcasts disable interaction')
              );
          }
        );
        break;
      }
      default: {
        await msgInt
          .editReply({
            embeds: [errorEmbed().setDescription('Command not yet implemented.')],
          })
          .catch((err) =>
            LogHandler.SaveLog(
              'Frontend',
              'Error',
              'Failed to edit reply for the broadcasts command not implemented interaction'
            )
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
            case 'enable_broadcasts_btn': {
              await enableBroadcasts(msgInt, channel);
              await int.update({}).catch();
              break;
            }
            case 'disable_broadcasts_btn': {
              await disableBroadcasts(msgInt, channel);
              break;
            }
            case 'save_btn': {
              // Yeah it does nothing.
              await int.update({}).catch();
              break;
            }
            case 'items_menu_select': {
              await updateBroadcasts(msgInt, 'item_track_mode', (int as SelectMenuInteraction).values[0], guild);
              await int.update({}).catch();
              break;
            }
            case 'titles_menu_select': {
              await updateBroadcasts(msgInt, 'title_track_mode', (int as SelectMenuInteraction).values[0], guild);
              await int.update({}).catch();
              break;
            }
            case 'clan_menu_select': {
              await updateBroadcasts(msgInt, 'clan_track_mode', (int as SelectMenuInteraction).values[0], guild);
              await int.update({}).catch();
              break;
            }
            case 'triumph_menu_select': {
              await updateBroadcasts(msgInt, 'triumph_track_mode', (int as SelectMenuInteraction).values[0], guild);
              await int.update({}).catch();
              break;
            }
          }
        } catch (err) {
          console.error('Broadcasts message collector', err);
        }
      }
    });
  },
};

const enableBroadcasts = async (msgInt: CommandInteraction, channel) => {
  const embed = primaryEmbed().setTitle('Broadcasts');
  const guild = await getGuild(msgInt);
  if (!guild) return;

  if (!guild.broadcasts_config.channel_id) {
    embed.setDescription(
      'Please select a channel to broadcast to before enabling broadcasts.\nUse `/broadcast channel`'
    );
    msgInt
      .editReply({
        embeds: [embed],
        components: [],
      })
      .catch((err) =>
        LogHandler.SaveLog(
          'Frontend',
          'Error',
          'Failed to edit reply for the enable broadcasts interaction when no channel set'
        )
      );
    return;
  }

  guild.broadcasts_config.is_broadcasting = true;

  DatabaseFunctions.updateGuildBroadcasts(guild, msgInt.guild.id, function enableBroadcasts(isError, severity, err) {
    if (isError) {
      ErrorHandler(severity, err);
      embed.setDescription(`There was an error trying to enable broadcasts. Please try again.`);
      (channel as any).send({
        embeds: [embed],
        ephemeral: true,
      });
    } else {
      LogHandler.SaveLog('Frontend', 'Info', `${msgInt.guild.id} has re-enabled broadcasts.`);
    }

    // Reload settings embed.
    settingsEmbed(msgInt, guild);
  });
};

const disableBroadcasts = async (msgInt: CommandInteraction, channel) => {
  const embed = primaryEmbed().setTitle('Broadcasts');
  const guild = await getGuild(msgInt);
  if (!guild) return;

  guild.broadcasts_config.is_broadcasting = false;
  guild.broadcasts_config.channel_id = null;

  DatabaseFunctions.updateGuildBroadcasts(
    guild,
    msgInt.guild.id,
    async function disableBroadcasts(isError, severity, err) {
      if (isError) {
        ErrorHandler(severity, err);
        embed.setDescription(`There was an error trying to disable broadcasts. Please try again.`);
        (channel as any).send({
          embeds: [embed],
          ephemeral: true,
        });
      } else {
        LogHandler.SaveLog('Frontend', 'Info', `${msgInt.guild.id} has disabled broadcasts.`);
        await msgInt
          .editReply({
            embeds: [primaryEmbed().setTitle('Broadcasts').setDescription('Broadcasts are now disabled.')],
            components: [],
          })
          .catch((err) =>
            LogHandler.SaveLog(
              'Frontend',
              'Error',
              'Failed to edit reply for the disable broadcasts interaction after disabling them'
            )
          );
      }
    }
  );
};

const updateBroadcasts = async (msgInt: CommandInteraction, key: string, selection: string, guild: IGuild) => {
  guild.broadcasts_config[key] = parseInt(selection);
  await new Promise((resolve) =>
    DatabaseFunctions.updateGuildBroadcasts(guild, msgInt.guild.id, async (isError, isFound, data) => {
      if (isError) {
        msgInt
          .editReply({
            embeds: [databaseErrorEmbed()],
          })
          .catch((err) =>
            LogHandler.SaveLog('Frontend', 'Error', 'Failed to edit reply for the broadcasts update interaction')
          );
      } else {
        await settingsEmbed(msgInt, guild);
        resolve(true);
      }
    })
  );
};

const disableBroadcastBtn = new MessageButton()
  .setCustomId('disable_broadcasts_btn')
  .setLabel('Disable broadcasts')
  .setStyle('DANGER');
const enableBroadcastsBtn = new MessageButton()
  .setCustomId('enable_broadcasts_btn')
  .setLabel('Enable broadcasts')
  .setStyle('PRIMARY');
const saveBtn = new MessageButton().setCustomId('save_btn').setLabel('Save').setStyle('SUCCESS');

const itemsMenuSelect = (guild: IGuild): MessageActionRow =>
  new MessageActionRow().addComponents(
    new MessageSelectMenu().setCustomId('items_menu_select').setOptions([
      {
        label: 'Items - Disabled',
        description: "Item broadcasts won't be sent",
        value: CuratedBroadcastSettingModes.Disabled.toString(),
        default: guild.broadcasts_config.item_track_mode === CuratedBroadcastSettingModes.Disabled,
      },
      {
        label: 'Items - Manual',
        description: 'Only items added via /track will be broadcasted',
        value: CuratedBroadcastSettingModes.Manual.toString(),
        default: guild.broadcasts_config.item_track_mode === CuratedBroadcastSettingModes.Manual,
      },
      {
        label: 'Items - Manual + Curated',
        description: "Items you've added + items from a curated dev list.",
        value: CuratedBroadcastSettingModes.Curated.toString(),
        default: guild.broadcasts_config.item_track_mode === CuratedBroadcastSettingModes.Curated,
      },
    ])
  );

const titlesMenuSelect = (guild: IGuild): MessageActionRow =>
  new MessageActionRow().addComponents(
    new MessageSelectMenu().setCustomId('titles_menu_select').setOptions([
      {
        label: 'Titles - Disabled',
        description: 'Title broadcasts will not be sent',
        value: BroadcastSettingModes.Disabled.toString(),
        default: guild.broadcasts_config.title_track_mode === BroadcastSettingModes.Disabled,
      },
      {
        label: 'Titles - Enabled',
        description: 'Title broadcasts will be sent to the selected channel.',
        value: BroadcastSettingModes.Enabled.toString(),
        default: guild.broadcasts_config.title_track_mode === BroadcastSettingModes.Enabled,
      },
    ])
  );

const clanMenuSelect = (guild: IGuild): MessageActionRow =>
  new MessageActionRow().addComponents(
    new MessageSelectMenu().setCustomId('clan_menu_select').setOptions([
      {
        label: 'Clan - Disabled',
        description: 'Clan broadcasts will not be sent.',
        value: BroadcastSettingModes.Disabled.toString(),
        default: guild.broadcasts_config.clan_track_mode === BroadcastSettingModes.Disabled,
      },
      {
        label: 'Clan - Enabled',
        description: 'Clan broadcasts will be sent to the selected channel.',
        value: BroadcastSettingModes.Enabled.toString(),
        default: guild.broadcasts_config.clan_track_mode === BroadcastSettingModes.Enabled,
      },
    ])
  );

const triumphMenuSelect = (guild: IGuild): MessageActionRow =>
  new MessageActionRow().addComponents(
    new MessageSelectMenu().setCustomId('triumph_menu_select').setOptions([
      {
        label: 'Triumphs - Disabled',
        description: 'Triumph broadcasts will not be sent.',
        value: CuratedBroadcastSettingModes.Disabled.toString(),
        default: guild.broadcasts_config.triumph_track_mode === CuratedBroadcastSettingModes.Disabled,
      },
      {
        label: 'Triumphs - Enabled',
        description: 'Triumph broadcasts will be sent to the selected channel.',
        value: CuratedBroadcastSettingModes.Curated.toString(),
        default: guild.broadcasts_config.triumph_track_mode === CuratedBroadcastSettingModes.Curated,
      },
    ])
  );

const generateBtnRow = (guild: IGuild): MessageActionRow => {
  const buttonsRow = new MessageActionRow();
  if (guild.broadcasts_config.is_broadcasting) {
    buttonsRow.addComponents(disableBroadcastBtn);
  } else {
    buttonsRow.addComponents(enableBroadcastsBtn);
  }
  buttonsRow.addComponents(saveBtn);
  return buttonsRow;
};

const settingsEmbed = async (msgInt: CommandInteraction, guild: IGuild) => {
  const embed = primaryEmbed().setTitle('Broadcasts - Settings');
  const description = [];
  const components = [];

  if (!guild.broadcasts_config.channel_id) {
    embed.setDescription(
      'Please select a channel to broadcast to before enabling broadcasts.\nUse `/broadcast channel`'
    );
    msgInt
      .editReply({
        embeds: [embed],
        components: [],
      })
      .catch((err) =>
        LogHandler.SaveLog(
          'Frontend',
          'Error',
          'Failed to edit reply for the broadcasts settings interaction when no channel set'
        )
      );
    return;
  }

  // Make description
  description.push(`Set up your Discord server broadcasts settings in this menu!\n`);
  description.push(`**Status**`);
  if (guild.broadcasts_config.is_broadcasting) {
    description.push(`This server is currently broadcasting to <#${guild.broadcasts_config.channel_id}>\n`);
    description.push(`If this channel no longer exists, then use \`/broadcast channel\` to setup the channel again.`);
    components.push(itemsMenuSelect(guild));
    components.push(titlesMenuSelect(guild));
    components.push(clanMenuSelect(guild));
    components.push(triumphMenuSelect(guild));
  } else {
    description.push(`Broadcasts are disabled.`);
  }

  // Build reply
  await msgInt
    .editReply({
      embeds: [embed.setDescription(description.join('\n'))],
      components: [...components, generateBtnRow(guild)],
    })
    .catch((err) =>
      LogHandler.SaveLog('Frontend', 'Error', 'Failed to edit reply for the broadcasts settings interaction')
    );
};

const getGuild = async (msgInt: CommandInteraction) => {
  const guildCallback: IGuildCallback = await new Promise((resolve) =>
    DatabaseFunctions.getFullGuild(msgInt.guild.id, (isError, isFound, data) => {
      resolve({ isError, isFound, data });
    })
  );

  if (guildCallback.isError) {
    msgInt
      .editReply({
        embeds: [databaseErrorEmbed()],
      })
      .catch((err) =>
        LogHandler.SaveLog(
          'Frontend',
          'Error',
          'Failed to edit reply for the broadcasts get guild function interaction'
        )
      );
    return undefined;
  } else {
    return guildCallback.data?.[0];
  }
};

export default WOKCommand;
