import DiscordJS, { ApplicationCommandOptionChoice, CommandInteraction } from 'discord.js';
import { ICommand as WokCommand } from 'wokcommands';
import { IDestinyUser, IDestinyUserLite, IDestinyUserLiteCallback } from '../interfaces/destiny_user.interface';
import { databaseErrorEmbed, errorEmbed, noMembersFoundEmbed, primaryEmbed } from '../handlers/embed.handler';
import { Metrics } from '../references';
import * as DatabaseFunctions from '../handlers/database.functions';
import * as LogHandler from '../handlers/log.handler';
import { byString } from '../handlers/misc.handler';
import { buildPersonalRow, buildRows, mapDefaultFieldNames } from '../handlers/interaction.helper';
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
  { name: 'Shattered Throne', value: 'Shattered Throne' },
  { name: 'Pit of Heresy', value: 'Pit of Heresy' },
  { name: 'Prophecy', value: 'Prophecy' },
  { name: 'Grasp of Avarice', value: 'Grasp of Avarice' },
  { name: 'Duality', value: 'Duality' },
];

const WOKCommand: WokCommand = {
  category: 'dungeon',
  description: 'Create a leaderboard for a specific dungeon',

  slash: true,
  testOnly: false,

  minArgs: 1,
  expectedArgs: '<choice>',

  options: [
    {
      name: 'choice',
      description: 'Choose a dungeon.',
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
        let b_data_one = byString(b, command.sorting) || 0;

        return b_data_one - a_data_one;
      });
    } else {
      sortedPlayers = guildPlayers.sort((a, b) => {
        let a_data_one = byString(a, command.sorting[0]) || 0;
        let a_data_two = byString(a, command.sorting[1]) || 0;
        let a_data_three = byString(a, command.sorting[2]) || 0;
        let b_data_one = byString(b, command.sorting[0]) || 0;
        let b_data_two = byString(b, command.sorting[1]) || 0;
        let b_data_three = byString(b, command.sorting[2]) || 0;

        let a_total = a_data_one + a_data_two + a_data_three;
        let b_total = b_data_one + b_data_two + b_data_three;

        return b_total - a_total;
      });
    }

    embed.setTitle(command.title.replace('{size}', size.toString()));
    embed.setDescription(
      `[Click to see full leaderboard](https://marvin.gg/leaderboards/${msgInt.guild.id}/${command.leaderboardURL}/)\n` +
        '```' +
        `Rank: ${mapDefaultFieldNames(command.fields)} - Name` +
        `\n\n` +
        `${buildRows(command.fields, sortedPlayers as IDestinyUser[], size).join('')}` +
        `${buildPersonalRow(command.fields, registeredPlayerInfo as IDestinyUser, sortedPlayers as IDestinyUser[])}` +
        '```'
    );

    msgInt
      .editReply({
        embeds: [embed],
      })
      .catch((err) =>
        LogHandler.SaveLog('Frontend', 'Error', 'Failed to edit reply for the dungeons build leaderboard interaction')
      );
  } catch (err) {
    LogHandler.SaveLog('Frontend', 'Error', err);
    msgInt
      .editReply({
        embeds: [errorEmbed()],
      })
      .catch((err) =>
        LogHandler.SaveLog(
          'Frontend',
          'Error',
          'Failed to edit reply for the dungeons build leaderboard error interaction'
        )
      );
  }
};

const getGuildMembers = async (msgInt: CommandInteraction) => {
  const guildCallback: IGuildCallback = await new Promise((resolve) =>
    DatabaseFunctions.getGuild(msgInt.guild.id, (isError, isFound, data) => {
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
          'Failed to edit reply for the dungeons get guild members error interaction'
        )
      );
    return undefined;
  }

  const guild = guildCallback.data?.[0];

  const guildMembersCallback: IDestinyUserLiteCallback = await new Promise((resolve) =>
    DatabaseFunctions.getGuildMembers(guild.clans, (isError, isFound, data) => {
      resolve({ isError, isFound, data });
    })
  );

  if (guildMembersCallback.isError) {
    msgInt
      .editReply({
        embeds: [databaseErrorEmbed()],
      })
      .catch((err) =>
        LogHandler.SaveLog(
          'Frontend',
          'Error',
          'Failed to edit reply for the dungeons get guild members error interaction'
        )
      );
    return undefined;
  }

  if (!guildMembersCallback.isFound) {
    LogHandler.SaveLog(
      'Frontend',
      'Error',
      `${msgInt.guild.id} tried to use /dungeon but there were no guild members found. Perhaps no clan setup?`
    );
    msgInt
      .editReply({
        embeds: [noMembersFoundEmbed()],
      })
      .catch((err) =>
        LogHandler.SaveLog(
          'Frontend',
          'Error',
          'Failed to edit reply for the dungeons get guild members error interaction'
        )
      );
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
  // Shattered Throne
  {
    name: 'Shattered Throne',
    title: 'Top {size} Shattered Throne Completions',
    leaderboardURL: 'shattered_throne',
    sorting: [
      `metrics[${Metrics.Dungeons.ST_COMPLETIONS}].progress`,
      `metrics[${Metrics.Dungeons.ST_COMPLETIONS_FLAWLESS}].progress`,
      `metrics[${Metrics.Dungeons.ST_COMPLETIONS_FLAWLESS_SOLO}].progress`,
    ],
    fields: [
      {
        name: 'Total (Comp | Flaw)',
        type: 'SplitTotal3Data',
        data: [
          `metrics[${Metrics.Dungeons.ST_COMPLETIONS}].progress`,
          `metrics[${Metrics.Dungeons.ST_COMPLETIONS_FLAWLESS}].progress`,
          `metrics[${Metrics.Dungeons.ST_COMPLETIONS_FLAWLESS_SOLO}].progress`,
        ],
        inline: true,
      },
    ],
  },

  // Pit of heresy
  {
    name: 'Pit of Heresy',
    title: 'Top {size} Pit of Heresy Completions',
    leaderboardURL: 'pit_of_heresy',
    sorting: [
      `metrics[${Metrics.Dungeons.PIT_COMPLETIONS}].progress`,
      `metrics[${Metrics.Dungeons.PIT_COMPLETIONS_FLAWLESS}].progress`,
      `metrics[${Metrics.Dungeons.PIT_COMPLETIONS_FLAWLESS_SOLO}].progress`,
    ],
    fields: [
      {
        name: 'Total (Comp | Flaw)',
        type: 'SplitTotal3Data',
        data: [
          `metrics[${Metrics.Dungeons.PIT_COMPLETIONS}].progress`,
          `metrics[${Metrics.Dungeons.PIT_COMPLETIONS_FLAWLESS}].progress`,
          `metrics[${Metrics.Dungeons.PIT_COMPLETIONS_FLAWLESS_SOLO}].progress`,
        ],
        inline: true,
      },
    ],
  },

  // Prophecy
  {
    name: 'Prophecy',
    title: 'Top {size} Prophecy Completions',
    leaderboardURL: 'prophecy',
    sorting: [
      `metrics[${Metrics.Dungeons.PROPHECY_COMPLETIONS}].progress`,
      `metrics[${Metrics.Dungeons.PROPHECY_COMPLETIONS_FLAWLESS}].progress`,
    ],
    fields: [
      {
        name: 'Total (Comp | Flaw)',
        type: 'SplitTotal',
        data: [
          `metrics[${Metrics.Dungeons.PROPHECY_COMPLETIONS}].progress`,
          `metrics[${Metrics.Dungeons.PROPHECY_COMPLETIONS_FLAWLESS}].progress`,
        ],
        inline: true,
      },
    ],
  },

  // Grasp of Avarice
  {
    name: 'Grasp of Avarice',
    title: 'Top {size} Grasp of Avarice Completions',
    leaderboardURL: 'grasp_of_avarice',
    sorting: [
      `metrics[${Metrics.Dungeons.GRASP_COMPLETIONS}].progress`,
      `metrics[${Metrics.Dungeons.GRASP_COMPLETIONS_FLAWLESS}].progress`,
      `metrics[${Metrics.Dungeons.GRASP_COMPLETIONS_FLAWLESS_SOLO}].progress`,
    ],
    fields: [
      {
        name: 'Total (Comp | Flaw)',
        type: 'SplitTotal3Data',
        data: [
          `metrics[${Metrics.Dungeons.GRASP_COMPLETIONS}].progress`,
          `metrics[${Metrics.Dungeons.GRASP_COMPLETIONS_FLAWLESS}].progress`,
          `metrics[${Metrics.Dungeons.GRASP_COMPLETIONS_FLAWLESS_SOLO}].progress`,
        ],
        inline: true,
      },
    ],
  },

  // Duality
  {
    name: 'Duality',
    title: 'Top {size} Duality Completions',
    leaderboardURL: 'duality',
    sorting: [
      `metrics[${Metrics.Dungeons.DUALITY_COMPLETIONS}].progress`,
      `metrics[${Metrics.Dungeons.DUALITY_COMPLETIONS_FLAWLESS}].progress`,
      `metrics[${Metrics.Dungeons.DUALITY_COMPLETIONS_FLAWLESS_SOLO}].progress`,
    ],
    fields: [
      {
        name: 'Total (Comp | Flaw)',
        type: 'SplitTotal3Data',
        data: [
          `metrics[${Metrics.Dungeons.DUALITY_COMPLETIONS}].progress`,
          `metrics[${Metrics.Dungeons.DUALITY_COMPLETIONS_FLAWLESS}].progress`,
          `metrics[${Metrics.Dungeons.DUALITY_COMPLETIONS_FLAWLESS_SOLO}].progress`,
        ],
        inline: true,
      },
    ],
  },
];

export default WOKCommand;
