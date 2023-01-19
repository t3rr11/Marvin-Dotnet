import DiscordJS, { ApplicationCommandOptionChoice, CommandInteraction } from 'discord.js';
import { ICommand as WokCommand } from 'wokcommands';
import { IDestinyUser, IDestinyUserLite, IDestinyUserLiteCallback } from '../interfaces/destiny_user.interface';
import { databaseErrorEmbed, errorEmbed, noMembersFoundEmbed, primaryEmbed } from '../handlers/embed.handler';
import { Metrics } from '../references';
import * as DatabaseFunctions from '../handlers/database.functions';
import * as LogHandler from '../handlers/log.handler';
import { byString } from '../handlers/misc.handler';
import { buildRows, mapDefaultFieldNames } from '../handlers/interaction.helper';
import { IGuildCallback, IGuild } from '../interfaces/guild.interface';
import { ICommand } from '../interfaces/command.interface';
import { IRegisteredUserCallback } from '../interfaces/registered_user.interface';

const sizeOption = {
  name: 'size',
  description: 'How big of a leaderboard do you want? Top 10? Top 25? Top 5? (max 25)',
  type: 10, // NUMBER
  min_value: 0,
  max_value: 25,
};

const Choices: ApplicationCommandOptionChoice[] = [
  { name: 'Leviathan', value: 'Leviathan' },
  { name: 'Eater of Worlds', value: 'Eater of Worlds' },
  { name: 'Spire of Stars', value: 'Spire of Stars' },
  { name: 'Last Wish', value: 'Last Wish' },
  { name: 'Scourge of the Past', value: 'Scourge of the Past' },
  { name: 'Crown of Sorrows', value: 'Crown of Sorrows' },
  { name: 'Garden of Salvation', value: 'Garden of Salvation' },
  { name: 'Deep Stone Crypt', value: 'Deep Stone Crypt' },
  { name: 'Vault of Glass', value: 'Vault of Glass' },
  { name: 'Vow of the Disciple', value: 'Vow of the Disciple' },
  { name: "King's Fall", value: "King's Fall" },
  { name: 'Total', value: 'All raids combined' },
];

const WOKCommand: WokCommand = {
  category: 'raid',
  description: 'Create a leaderboard for a specific raid',

  slash: true,
  testOnly: false,

  minArgs: 1,
  expectedArgs: '<choice>',

  options: [
    {
      name: 'choice',
      description: 'Choose a raid.',
      required: true,
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.STRING,
      choices: Choices,
    },
    sizeOption,
  ],

  callback: async ({ interaction: msgInt, args }) => {
    LogHandler.SaveInteractionLog(msgInt);
    await msgInt.deferReply({ ephemeral: false });
    const guildPlayers = await getGuildMembers(msgInt);
    if (!guildPlayers) return;
    const registeredPlayerInfo = await getRegisteredUser(msgInt);

    // Add registered user if details were found.
    if (registeredPlayerInfo) {
      if (!guildPlayers.find((e) => e.membership_id === registeredPlayerInfo.membership_id)) {
        guildPlayers.push(registeredPlayerInfo);
      }
    }

    await BuildLeaderboard(msgInt, guildPlayers, registeredPlayerInfo);
  },
};

const BuildLeaderboard = async (
  msgInt: CommandInteraction,
  guildPlayers: IDestinyUserLite[],
  registeredPlayerInfo: IDestinyUserLite
) => {
  const embed = primaryEmbed();
  const command = commands.find((command) => command.name === msgInt.options.getString('choice'));
  const size = msgInt.options.getNumber('size') || 10;
  let sortedPlayers: IDestinyUserLite[] = [];

  try {
    if (!Array.isArray(command.sorting)) {
      sortedPlayers = guildPlayers.sort((a, b) => {
        let a_data_one = byString(a, command.sorting) || 0;
        let b_data_two = byString(b, command.sorting) || 0;

        return b_data_two - a_data_one;
      });
    } else {
      sortedPlayers = guildPlayers.sort((a, b) => {
        let a_data_one = byString(a, command.sorting[0]) || 0;
        let a_data_two = byString(a, command.sorting[1]) || 0;
        let b_data_one = byString(b, command.sorting[0]) || 0;
        let b_data_two = byString(b, command.sorting[1]) || 0;

        return b_data_one + b_data_two - (a_data_one + a_data_two);
      });
    }

    embed.setTitle(command.title.replace('{size}', size.toString()));
    embed.setDescription(
      `[Click to see full leaderboard](https://marvin.gg/leaderboards/${msgInt.guild.id}/${command.leaderboardURL}/)\n` +
        '```' +
        `Rank: ${mapDefaultFieldNames(command.fields)} - Name` +
        `\n\n` +
        `${buildRows(command.fields, sortedPlayers as IDestinyUser[], size || 10).join('')}` +
        '```'
    );

    msgInt.editReply({
      embeds: [embed],
    });
  } catch (err) {
    LogHandler.SaveLog('Frontend', 'Error', err);
    msgInt.editReply({
      embeds: [errorEmbed()],
    });
  }
};

const getGuildMembers = async (msgInt: CommandInteraction) => {
  const guildCallback: IGuildCallback = await new Promise((resolve) =>
    DatabaseFunctions.getGuild(msgInt.guild.id, (isError, isFound, data) => {
      resolve({ isError, isFound, data });
    })
  );

  if (guildCallback.isError) {
    msgInt.editReply({ embeds: [databaseErrorEmbed()] });
    return undefined;
  }

  const guild = guildCallback.data?.[0];

  const guildMembersCallback: IDestinyUserLiteCallback = await new Promise((resolve) =>
    DatabaseFunctions.getGuildMembers(guild.clans, (isError, isFound, data) => {
      resolve({ isError, isFound, data });
    })
  );

  if (guildMembersCallback.isError) {
    msgInt.editReply({ embeds: [databaseErrorEmbed()] });
    return undefined;
  }

  if (!guildMembersCallback.isFound) {
    LogHandler.SaveLog(
      'Frontend',
      'Error',
      `${msgInt.guild.id} tried to use /raid but there were no guild members found. Perhaps no clan setup?`
    );
    msgInt.editReply({ embeds: [noMembersFoundEmbed()] });
    return undefined;
  }

  const guildMembers = guildMembersCallback.data;
  return guildMembers;
};

const getRegisteredUser = async (msgInt: CommandInteraction) => {
  const registeredUserCallback: IRegisteredUserCallback = await new Promise((resolve) =>
    DatabaseFunctions.getRegisteredUserById(msgInt.user.id, (isError, isFound, data) => {
      resolve({ isError, isFound, data });
    })
  );

  if (registeredUserCallback.isError) return undefined;
  if (!registeredUserCallback.isFound) return undefined;

  const destinyUserCallback: IDestinyUserLiteCallback = await new Promise((resolve) =>
    DatabaseFunctions.getLiteUserById(registeredUserCallback.data?.[0].membership_id, (isError, isFound, data) => {
      resolve({ isError, isFound, data });
    })
  );

  if (destinyUserCallback.isError) return undefined;
  if (!destinyUserCallback.isFound) return undefined;

  return destinyUserCallback.data?.[0];
};

const commands: ICommand[] = [
  // Levi
  {
    name: 'Leviathan',
    title: 'Top {size} Leviathan Completions',
    leaderboardURL: 'leviathan',
    sorting: [
      `metrics[${Metrics.Raids.LEVI_COMPLETIONS}].progress`,
      `metrics[${Metrics.Raids.LEVI_PRES_COMPLETIONS}].progress`,
    ],
    fields: [
      {
        name: 'Total (Norm | Pres)',
        type: 'SplitTotal',
        data: [
          `metrics[${Metrics.Raids.LEVI_COMPLETIONS}].progress`,
          `metrics[${Metrics.Raids.LEVI_PRES_COMPLETIONS}].progress`,
        ],
        inline: true,
      },
    ],
  },

  // Eater of worlds
  {
    name: 'Eater of Worlds',
    title: 'Top {size} Eater of Worlds Completions',
    leaderboardURL: 'eater_of_worlds',
    sorting: [
      `metrics[${Metrics.Raids.EOW_COMPLETIONS}].progress`,
      `metrics[${Metrics.Raids.EOW_PRES_COMPLETIONS}].progress`,
    ],
    fields: [
      {
        name: 'Total (Norm | Pres)',
        type: 'SplitTotal',
        data: [
          `metrics[${Metrics.Raids.EOW_COMPLETIONS}].progress`,
          `metrics[${Metrics.Raids.EOW_PRES_COMPLETIONS}].progress`,
        ],
        inline: true,
      },
    ],
  },

  // Spire of stars
  {
    name: 'Spire of Stars',
    title: 'Top {size} Spire of Stars Completions',
    leaderboardURL: 'spire_of_stars',
    sorting: [
      `metrics[${Metrics.Raids.SOS_COMPLETIONS}].progress`,
      `metrics[${Metrics.Raids.SOS_PRES_COMPLETIONS}].progress`,
    ],
    fields: [
      {
        name: 'Total (Norm | Pres)',
        type: 'SplitTotal',
        data: [
          `metrics[${Metrics.Raids.SOS_COMPLETIONS}].progress`,
          `metrics[${Metrics.Raids.SOS_PRES_COMPLETIONS}].progress`,
        ],
        inline: true,
      },
    ],
  },

  // Last wish
  {
    name: 'Last Wish',
    title: 'Top {size} Last Wish Completions',
    leaderboardURL: 'last_wish',
    sorting: `metrics[${Metrics.Raids.LW_COMPLETIONS}].progress`,
    fields: [
      {
        name: 'Completions',
        type: 'Leaderboard',
        data: `metrics[${Metrics.Raids.LW_COMPLETIONS}].progress`,
        inline: true,
      },
    ],
  },

  // Scourge of the past
  {
    name: 'Scourge of the Past',
    title: 'Top {size} Scourge of the Past Completions',
    leaderboardURL: 'scourge_of_the_past',
    sorting: `metrics[${Metrics.Raids.SOTP_COMPLETIONS}].progress`,
    fields: [
      {
        name: 'Completions',
        type: 'Leaderboard',
        data: `metrics[${Metrics.Raids.SOTP_COMPLETIONS}].progress`,
        inline: true,
      },
    ],
  },

  // Crown of sorrows
  {
    name: 'Crown of Sorrows',
    title: 'Top {size} Crown of Sorrows Completions',
    leaderboardURL: 'crown_of_sorrow',
    sorting: `metrics[${Metrics.Raids.COS_COMPLETIONS}].progress`,
    fields: [
      {
        name: 'Completions',
        type: 'Leaderboard',
        data: `metrics[${Metrics.Raids.COS_COMPLETIONS}].progress`,
        inline: true,
      },
    ],
  },

  // Garden of salvation
  {
    name: 'Garden of Salvation',
    title: 'Top {size} Garden of Salvation Completions',
    leaderboardURL: 'garden_of_salvation',
    sorting: `metrics[${Metrics.Raids.GOS_COMPLETIONS}].progress`,
    fields: [
      {
        name: 'Completions',
        type: 'Leaderboard',
        data: `metrics[${Metrics.Raids.GOS_COMPLETIONS}].progress`,
        inline: true,
      },
    ],
  },

  // Deep stone crypt
  {
    name: 'Deep Stone Crypt',
    title: 'Top {size} Deep Stone Crypt Completions',
    leaderboardURL: 'deep_stone_crypt',
    sorting: `metrics[${Metrics.Raids.DSC_COMPLETIONS}].progress`,
    fields: [
      {
        name: 'Completions',
        type: 'Leaderboard',
        data: `metrics[${Metrics.Raids.DSC_COMPLETIONS}].progress`,
        inline: true,
      },
    ],
  },

  // Vault of glass
  {
    name: 'Vault of Glass',
    title: 'Top {size} Vault of Glass Completions',
    leaderboardURL: 'vault_of_glass',
    sorting: `metrics[${Metrics.Raids.VOG_COMPLETIONS}].progress`,
    fields: [
      {
        name: 'Completions',
        type: 'Leaderboard',
        data: `metrics[${Metrics.Raids.VOG_COMPLETIONS}].progress`,
        inline: true,
      },
    ],
  },

  // Vow of the Disciple
  {
    name: 'Vow of the Disciple',
    title: 'Top {size} Vow of the Disciple Completions',
    leaderboardURL: 'vow_of_the_disciple',
    sorting: `metrics[${Metrics.Raids.VOTD_COMPLETIONS}].progress`,
    fields: [
      {
        name: 'Completions',
        type: 'Leaderboard',
        data: `metrics[${Metrics.Raids.VOTD_COMPLETIONS}].progress`,
        inline: true,
      },
    ],
  },

  // King's Fall
  {
    name: "King's Fall",
    title: "Top {size} King's Fall Completions",
    leaderboardURL: 'kings_fall',
    sorting: `metrics[${Metrics.Raids.KF_COMPLETIONS}].progress`,
    fields: [
      {
        name: 'Completions',
        type: 'Leaderboard',
        data: `metrics[${Metrics.Raids.KF_COMPLETIONS}].progress`,
        inline: true,
      },
    ],
  },

  // Total
  {
    name: 'All raids combined',
    title: 'Top {size} Total Raid Completions',
    leaderboardURL: 'total_raids',
    sorting: `computed_data.totalRaids`,
    fields: [
      {
        name: 'Completions',
        type: 'Leaderboard',
        data: `computed_data.totalRaids`,
        inline: true,
      },
    ],
  },
];

export default WOKCommand;
