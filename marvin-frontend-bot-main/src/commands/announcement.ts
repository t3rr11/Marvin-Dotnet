import DiscordJS, {
  Channel,
  CommandInteraction,
  MessageActionRow,
  MessageButton,
  MessageComponentInteraction,
  MessageSelectMenu,
  SelectMenuInteraction,
} from 'discord.js';
import { AnnouncementSettingModes, IGuild, IGuildCallback } from '../interfaces/guild.interface';
import { databaseErrorEmbed, errorEmbed, primaryEmbed } from '../handlers/embed.handler';
import { ICommand as WokCommand } from 'wokcommands';
import { ErrorHandler } from '../handlers/error.handler';
import * as DatabaseFunctions from '../handlers/database.functions';
import * as LogHandler from '../handlers/log.handler';

const WOKCommand: WokCommand = {
  category: 'announcements',
  description: 'Manage the server announcements. (Patreon Feature)',

  slash: true,
  testOnly: false,

  options: [
    {
      name: 'settings',
      description: 'Manage the types of announcements that can be sent, or enable/disable them.',
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.SUB_COMMAND,
    },
    {
      name: 'channel',
      description: 'Set a channel where announcements will be sent (also enables them).',
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.SUB_COMMAND,
      options: [
        {
          name: 'channel',
          description: 'Pick a channel, you wish the announcements to be sent to.',
          required: true,
          type: DiscordJS.Constants.ApplicationCommandOptionTypes.CHANNEL,
          channelTypes: ['GUILD_TEXT'],
        },
      ],
    },
    {
      name: 'disable',
      description: 'Disable announcements for this server.',
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.SUB_COMMAND,
    },
  ],

  callback: async ({ interaction: msgInt, channel }) => {
    LogHandler.SaveInteractionLog(msgInt);
    if (!msgInt.isCommand()) return;
    await msgInt.deferReply({ ephemeral: true });

    let page = 1;
    const guild = await getGuild(msgInt);
    if (!guild) return;

    switch (msgInt.options.getSubcommand()) {
      case 'settings': {
        await settingsEmbed(msgInt, guild, page);
        break;
      }
      case 'channel': {
        const embed = primaryEmbed().setTitle('Announcements - Enabled');
        const channel = msgInt.options.getChannel('channel') as Channel;
        guild.announcements_config.is_announcing = true;
        guild.announcements_config.channel_id = channel.id;
        DatabaseFunctions.updateGuildAnnouncements(
          guild,
          msgInt.guild.id,
          function setAnnouncementsChannel(isError, severity, err) {
            if (isError) {
              ErrorHandler(severity, err);
              embed.setDescription(`There was an error trying to enable announcements. Please try again.`);
            } else {
              embed.setDescription(`Announcements will now be sent to <#${channel.id}>.`);
              LogHandler.SaveLog('Frontend', 'Info', `${msgInt.guild.id} has set the announcements channel.`);
            }
            msgInt
              .editReply({
                embeds: [embed],
              })
              .catch((err) =>
                LogHandler.SaveLog(
                  'Frontend',
                  'Error',
                  'Failed to edit reply for the announcements channel interaction'
                )
              );
          }
        );
        break;
      }
      case 'disable': {
        const embed = primaryEmbed().setTitle('Announcements - Disabled');
        guild.announcements_config.is_announcing = false;
        DatabaseFunctions.updateGuildAnnouncements(
          guild,
          msgInt.guild.id,
          function disableAnnouncements(isError, severity, err) {
            if (isError) {
              ErrorHandler(severity, err);
              embed.setDescription(`There was an error trying to disable announcements. Please try again.`);
            } else {
              embed.setDescription(`Successfully disabled announcements, you will no longer recieve announcements.`);
              LogHandler.SaveLog('Frontend', 'Info', `${msgInt.guild.id} has disabled announcements.`);
            }
            msgInt
              .editReply({
                embeds: [embed],
              })
              .catch((err) =>
                LogHandler.SaveLog(
                  'Frontend',
                  'Error',
                  'Failed to edit reply for the announcements disable interaction'
                )
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
              'Failed to edit reply for the announcements command not implemented interaction'
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
            case 'enable_announcements_btn': {
              await enableAnnouncements(msgInt, channel, page);
              await int.update({}).catch();
              break;
            }
            case 'disable_announcements_btn': {
              await disableAnnouncements(msgInt, channel, page);
              break;
            }
            case 'save_btn': {
              // Yeah it does nothing.
              await int.update({}).catch();
              break;
            }
            case 'next_btn': {
              if (page >= 1) page++;
              await settingsEmbed(msgInt, guild, page);
              await int.update({}).catch();
              break;
            }
            case 'previous_btn': {
              if (page > 1) page--;
              await settingsEmbed(msgInt, guild, page);
              await int.update({}).catch();
              break;
            }
            case 'gunsmith_menu_select': {
              await updateAnnouncements(msgInt, 'gunsmiths', (int as SelectMenuInteraction).values[0], guild, page);
              await int.update({}).catch();
              break;
            }
            case 'adas_menu_select': {
              await updateAnnouncements(msgInt, 'adas', (int as SelectMenuInteraction).values[0], guild, page);
              await int.update({}).catch();
              break;
            }
            case 'lost_sector_menu_select': {
              await updateAnnouncements(msgInt, 'lost_sectors', (int as SelectMenuInteraction).values[0], guild, page);
              await int.update({}).catch();
              break;
            }
            case 'xur_menu_select': {
              await updateAnnouncements(msgInt, 'xur', (int as SelectMenuInteraction).values[0], guild, page);
              await int.update({}).catch();
              break;
            }
            case 'wellspring_menu_select': {
              await updateAnnouncements(msgInt, 'wellspring', (int as SelectMenuInteraction).values[0], guild, page);
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

const tempDisabledEmbed = (msgInt: CommandInteraction) => {
  const embed = primaryEmbed().setTitle('Announcements');
  embed.setDescription(
    'Automatic announcements are temporarily disabled, due to an on-going rework.\n\nHowever, you can still use the commands `/vendor`.\n\nJoin support server for updates if you want to (link on marvin.gg).'
  );
  msgInt
    .editReply({
      embeds: [embed],
      components: [],
    })
    .catch((err) => LogHandler.SaveLog('Frontend', 'Error', 'Failed to send temp announcements disabled embed'));
  return;
};

const enableAnnouncements = async (msgInt: CommandInteraction, channel, page: number) => {
  tempDisabledEmbed(msgInt);
  return;
  const embed = primaryEmbed().setTitle('Announcements');
  const guild = await getGuild(msgInt);
  if (!guild) return;

  if (!guild.announcements_config.channel_id) {
    embed.setDescription(
      'Please select a channel to announce to before enabling announcements.\nUse `/announcement channel`'
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
          'Failed to edit reply for the enable announcements interaction when no channel set'
        )
      );
    return;
  }

  guild.announcements_config.is_announcing = true;

  DatabaseFunctions.updateGuildAnnouncements(
    guild,
    msgInt.guild.id,
    function enableAnnouncements(isError, severity, err) {
      if (isError) {
        ErrorHandler(severity, err);
        embed.setDescription(`There was an error trying to enable announcements. Please try again.`);
        (channel as any).send({
          embeds: [embed],
          ephemeral: true,
        });
      } else {
        LogHandler.SaveLog('Frontend', 'Info', `${msgInt.guild.id} has re-enabled announcements.`);
      }

      // Reload settings embed.
      settingsEmbed(msgInt, guild, page);
    }
  );
};

const disableAnnouncements = async (msgInt: CommandInteraction, channel, page: number) => {
  tempDisabledEmbed(msgInt);
  return;
  const embed = primaryEmbed().setTitle('Announcements');
  const guild = await getGuild(msgInt);
  if (!guild) return;

  guild.announcements_config.is_announcing = false;
  guild.announcements_config.channel_id = null;

  DatabaseFunctions.updateGuildAnnouncements(
    guild,
    msgInt.guild.id,
    async function disableAnnouncements(isError, severity, err) {
      if (isError) {
        ErrorHandler(severity, err);
        embed.setDescription(`There was an error trying to disable announcements. Please try again.`);
        (channel as any).send({
          embeds: [embed],
          ephemeral: true,
        });
      } else {
        LogHandler.SaveLog('Frontend', 'Info', `${msgInt.guild.id} has disabled announcements.`);
        await msgInt
          .editReply({
            embeds: [primaryEmbed().setTitle('Announcements').setDescription('Announcements are now disabled.')],
            components: [],
          })
          .catch((err) =>
            LogHandler.SaveLog(
              'Frontend',
              'Error',
              'Failed to edit reply for the disable announcements interaction after disabling them'
            )
          );
      }
    }
  );
};

const updateAnnouncements = async (
  msgInt: CommandInteraction,
  key: string,
  selection: string,
  guild: IGuild,
  page: number
) => {
  tempDisabledEmbed(msgInt);
  return;
  guild.announcements_config[key] = parseInt(selection);
  await new Promise((resolve) =>
    DatabaseFunctions.updateGuildAnnouncements(guild, msgInt.guild.id, async (isError, isFound, data) => {
      if (isError) {
        msgInt
          .editReply({
            embeds: [databaseErrorEmbed()],
          })
          .catch((err) =>
            LogHandler.SaveLog(
              'Frontend',
              'Error',
              'Failed to edit reply for the announcements update announcements interaction'
            )
          );
      } else {
        await settingsEmbed(msgInt, guild, page);
        resolve(true);
      }
    })
  );
};

const disableAnnouncementBtn = new MessageButton()
  .setCustomId('disable_announcements_btn')
  .setLabel('Disable announcements')
  .setStyle('DANGER');
const enableAnnouncementsBtn = new MessageButton()
  .setCustomId('enable_announcements_btn')
  .setLabel('Enable announcements')
  .setStyle('PRIMARY');
const saveBtn = new MessageButton().setCustomId('save_btn').setLabel('Save').setStyle('SUCCESS');
const nextBtn = new MessageButton().setCustomId('next_btn').setLabel('More options').setStyle('PRIMARY');
const previousBtn = new MessageButton().setCustomId('previous_btn').setLabel('Previous options').setStyle('PRIMARY');

const gunsmithMenuSelect = (guild: IGuild): MessageActionRow =>
  new MessageActionRow().addComponents(
    new MessageSelectMenu().setCustomId('gunsmith_menu_select').setOptions([
      {
        label: 'Gunsmith - Disabled',
        description: 'Disable daily gunsmith announcements',
        value: AnnouncementSettingModes.Disabled.toString(),
        default: guild.announcements_config.gunsmiths === AnnouncementSettingModes.Disabled,
      },
      {
        label: 'Gunsmith - Enabled',
        description: 'Enable daily gunsmith announcements',
        value: AnnouncementSettingModes.Enabled.toString(),
        default: guild.announcements_config.gunsmiths === AnnouncementSettingModes.Enabled,
      },
    ])
  );

const adasMenuSelect = (guild: IGuild): MessageActionRow =>
  new MessageActionRow().addComponents(
    new MessageSelectMenu().setCustomId('adas_menu_select').setOptions([
      {
        label: 'Ada-1 - Disabled',
        description: "Disable daily Ada-1's announcements",
        value: AnnouncementSettingModes.Disabled.toString(),
        default: guild.announcements_config.adas === AnnouncementSettingModes.Disabled,
      },
      {
        label: 'Ada-1 - Enabled',
        description: "Enable daily Ada-1's announcements",
        value: AnnouncementSettingModes.Enabled.toString(),
        default: guild.announcements_config.adas === AnnouncementSettingModes.Enabled,
      },
    ])
  );

const xurMenuSelect = (guild: IGuild): MessageActionRow =>
  new MessageActionRow().addComponents(
    new MessageSelectMenu().setCustomId('xur_menu_select').setOptions([
      {
        label: 'Xur - Disabled',
        description: "Disable weekly Xur's announcements",
        value: AnnouncementSettingModes.Disabled.toString(),
        default: guild.announcements_config.xur === AnnouncementSettingModes.Disabled,
      },
      {
        label: 'Xur - Enabled',
        description: "Enable weekly Xur's announcements",
        value: AnnouncementSettingModes.Enabled.toString(),
        default: guild.announcements_config.xur === AnnouncementSettingModes.Enabled,
      },
    ])
  );

const lostSectorMenuSelect = (guild: IGuild): MessageActionRow =>
  new MessageActionRow().addComponents(
    new MessageSelectMenu().setCustomId('lost_sector_menu_select').setOptions([
      {
        label: 'Lost Sectors - Disabled',
        description: 'Disable daily lost sector announcements',
        value: AnnouncementSettingModes.Disabled.toString(),
        default: guild.announcements_config.lost_sectors === AnnouncementSettingModes.Disabled,
      },
      {
        label: 'Lost Sectors - Enabled',
        description: 'Enable daily lost sector announcements',
        value: AnnouncementSettingModes.Enabled.toString(),
        default: guild.announcements_config.lost_sectors === AnnouncementSettingModes.Enabled,
      },
    ])
  );

const wellspringMenuSelect = (guild: IGuild): MessageActionRow =>
  new MessageActionRow().addComponents(
    new MessageSelectMenu().setCustomId('wellspring_menu_select').setOptions([
      {
        label: 'Wellspring - Disabled',
        description: 'Disable daily wellspring announcements',
        value: AnnouncementSettingModes.Disabled.toString(),
        default: guild.announcements_config.wellspring === AnnouncementSettingModes.Disabled,
      },
      {
        label: 'Wellspring - Enabled',
        description: 'Enable daily wellspring announcements',
        value: AnnouncementSettingModes.Enabled.toString(),
        default: guild.announcements_config.wellspring === AnnouncementSettingModes.Enabled,
      },
    ])
  );

const generateBtnRow = (guild: IGuild, page: number): MessageActionRow => {
  const buttonsRow = new MessageActionRow();
  if (guild.announcements_config.is_announcing) {
    buttonsRow.addComponents(disableAnnouncementBtn);
  } else {
    buttonsRow.addComponents(enableAnnouncementsBtn);
  }
  if (page === 1) {
    buttonsRow.addComponents(nextBtn);
  }
  if (page === 2) {
    buttonsRow.addComponents(previousBtn);
  }
  buttonsRow.addComponents(saveBtn);
  return buttonsRow;
};

const settingsEmbed = async (msgInt: CommandInteraction, guild: IGuild, page: number) => {
  tempDisabledEmbed(msgInt);
  return;
  const embed = primaryEmbed().setTitle('Announcements - Settings');
  const description = [];
  const components = [];

  if (!guild.announcements_config.channel_id) {
    embed.setDescription(
      'Please select a channel to announcement to before enabling announcements.\nUse `/announcement channel`'
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
          'Failed to edit reply for the announcements settings interaction when no channel set'
        )
      );
    return;
  }

  // Make description
  description.push(`Set up your Discord server announcements settings in this menu!\n`);
  description.push(`**Status**`);
  if (guild.announcements_config.is_announcing) {
    description.push(`This server is currently sending announcements to <#${guild.announcements_config.channel_id}>\n`);
    description.push(
      `If this channel no longer exists, then use \`/announcement channel\` to setup the channel again.`
    );

    if (page === 1) {
      components.push(gunsmithMenuSelect(guild));
      components.push(adasMenuSelect(guild));
      components.push(lostSectorMenuSelect(guild));
      components.push(xurMenuSelect(guild));
    }
    if (page === 2) {
      components.push(wellspringMenuSelect(guild));
    }
  } else {
    description.push(`Announcements are disabled.`);
  }

  // Build reply
  await msgInt
    .editReply({
      embeds: [embed.setDescription(description.join('\n'))],
      components: [...components, generateBtnRow(guild, page)],
    })
    .catch((err) =>
      LogHandler.SaveLog('Frontend', 'Error', 'Failed to edit reply for the announcements settings interaction')
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
          'Failed to edit reply for the announcements get guild function interaction'
        )
      );
    return undefined;
  } else {
    return guildCallback.data?.[0];
  }
};

export default WOKCommand;
