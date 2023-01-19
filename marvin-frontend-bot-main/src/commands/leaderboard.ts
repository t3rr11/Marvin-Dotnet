import { CommandInteraction } from 'discord.js';
import { ICommand as WokCommand } from 'wokcommands';
import { databaseErrorEmbed, errorEmbed, noMembersFoundEmbed } from '../handlers/embed.handler';
import { IDestinyUserLiteCallback } from '../interfaces/destiny_user.interface';
import { IGuildCallback } from '../interfaces/guild.interface';
import { IRegisteredUserCallback } from '../interfaces/registered_user.interface';
import * as DatabaseFunctions from '../handlers/database.functions';
import * as LogHandler from '../handlers/log.handler';
import * as Valor from '../leaderboards/valor';
import * as Infamy from '../leaderboards/infamy';
import * as Glory from '../leaderboards/glory';
import * as TrialsRank from '../leaderboards/trials_rank';
import * as IronBanner from '../leaderboards/iron_banner';
import * as Power from '../leaderboards/power';
import * as TriumphScore from '../leaderboards/triumph_score';
import * as TimePlayed from '../leaderboards/time_played';
import * as SeasonRank from '../leaderboards/season_rank';

const sizeOption = {
  name: 'size',
  description: 'How big of a leaderboard do you want? Top 10? Top 25? Top 5? (max 25)',
  type: 10, // NUMBER
  min_value: 0,
  max_value: 25,
};

const CommandOptions = [
  {
    name: 'valor',
    description: 'Generate a server specific valor leaderboard.',
    type: 1,
    options: [sizeOption],
  },
  {
    name: 'infamy',
    description: 'Generate a server specific infamy leaderboard.',
    type: 1,
    options: [sizeOption],
  },
  {
    name: 'glory',
    description: 'Generate a server specific glory leaderboard.',
    type: 1,
    options: [sizeOption],
  },
  {
    name: 'trials_rank',
    description: 'Generate a server specific trials rank leaderboard.',
    type: 1,
    options: [sizeOption],
  },
  {
    name: 'iron_banner',
    description: 'Generate a server specific iron banner rank leaderboard.',
    type: 1,
    options: [sizeOption],
  },
  {
    name: 'power',
    description: 'Generate a server specific highest power leaderboard.',
    type: 1,
    options: [sizeOption],
  },
  {
    name: 'triumph_score',
    description: 'Generate a server specific triumph score leaderboard.',
    type: 1,
    options: [sizeOption],
  },
  {
    name: 'triumph_score_legacy',
    description: 'Generate a server specific legacy triumph score leaderboard.',
    type: 1,
    options: [sizeOption],
  },
  {
    name: 'triumph_score_lifetime',
    description: 'Generate a server specific lifetime triumph score leaderboard.',
    type: 1,
    options: [sizeOption],
  },
  {
    name: 'time_played',
    description: 'Generate a server specific most time played leaderboard.',
    type: 1,
    options: [sizeOption],
  },
  {
    name: 'season_rank',
    description: 'Generate a server specific season rank leaderboard.',
    type: 1,
    options: [sizeOption],
  },
];

const WOKCommand: WokCommand = {
  category: 'leaderboard',
  description: 'Returns a specific leaderboard.',

  slash: true,
  testOnly: false,
  options: CommandOptions,

  callback: async ({ interaction: msgInt, args }) => {
    LogHandler.SaveInteractionLog(msgInt);
    await msgInt.deferReply({ ephemeral: false });

    const type = msgInt.options.getSubcommand();
    const guildPlayers = await getGuildMembers(msgInt);
    if (!guildPlayers) return;
    const registeredPlayerInfo = await getRegisteredUser(msgInt);

    // Add registered user if details were found.
    if (registeredPlayerInfo) {
      if (!guildPlayers.find((e) => e.membership_id === registeredPlayerInfo.membership_id)) {
        guildPlayers.push(registeredPlayerInfo);
      }
    }

    switch (type) {
      case 'valor':
        return Valor.generateLeaderboard(msgInt, guildPlayers, registeredPlayerInfo);
      case 'infamy':
        return Infamy.generateLeaderboard(msgInt, guildPlayers, registeredPlayerInfo);
      case 'glory':
        return Glory.generateLeaderboard(msgInt, guildPlayers, registeredPlayerInfo);
      case 'trials_rank':
        return TrialsRank.generateLeaderboard(msgInt, guildPlayers, registeredPlayerInfo);
      case 'iron_banner':
        return IronBanner.generateLeaderboard(msgInt, guildPlayers, registeredPlayerInfo);
      case 'power':
        return Power.generateLeaderboard(msgInt, guildPlayers, registeredPlayerInfo);
      case 'triumph_score':
        return TriumphScore.generateLeaderboard(msgInt, guildPlayers, registeredPlayerInfo, 0);
      case 'triumph_score_legacy':
        return TriumphScore.generateLeaderboard(msgInt, guildPlayers, registeredPlayerInfo, 1);
      case 'triumph_score_lifetime':
        return TriumphScore.generateLeaderboard(msgInt, guildPlayers, registeredPlayerInfo, 2);
      case 'time_played':
        return TimePlayed.generateLeaderboard(msgInt, guildPlayers, registeredPlayerInfo);
      case 'season_rank':
        return SeasonRank.generateLeaderboard(msgInt, guildPlayers, registeredPlayerInfo);
      default: {
        await msgInt.editReply({
          embeds: [errorEmbed().setDescription('Command not yet implemented.')],
        });
        break;
      }
    }
  },
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
      `${msgInt.guild.id} tried to use /leaderboard but there were no guild members found. Perhaps no clan setup?`
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

export default WOKCommand;
