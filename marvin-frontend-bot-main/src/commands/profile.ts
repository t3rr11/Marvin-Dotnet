import DiscordJS, { ApplicationCommandOptionChoice, CommandInteraction } from 'discord.js';
import { ICommand as WokCommand } from 'wokcommands';
import {
  databaseErrorEmbed,
  destinyUserNotFoundEmbed,
  noMembersFoundEmbed,
  primaryEmbed,
  userNotRegisteredEmbed,
} from '../handlers/embed.handler';
import { IDestinyUserLite, IDestinyUserLiteCallback } from '../interfaces/destiny_user.interface';
import { IGuildCallback } from '../interfaces/guild.interface';
import { IRegisteredUserCallback } from '../interfaces/registered_user.interface';
import { Metrics, PresentationNodes, Progressions } from '../references';
import * as DatabaseFunctions from '../handlers/database.functions';
import * as MiscHandler from '../handlers/misc.handler';
import * as ManifestHandler from '../handlers/manifest.handler';
import * as LogHandler from '../handlers/log.handler';
import { BroadcastType, IDestinyUserBroadcastCallback } from '../interfaces/broadcast.interface';
import { DestinyRecordDefinition } from 'bungie-api-ts/destiny2';
import { ICallback } from '../interfaces/callbacks.interface';

const Choices: ApplicationCommandOptionChoice[] = [
  { name: 'Stats', value: 'stats' },
  { name: 'Raids', value: 'raids' },
  { name: 'Broadcasts', value: 'broadcasts' },
  { name: 'Grandmasters', value: 'grandmasters' },
];

const Raids: { name?: string; hash: string }[] = [
  { name: 'Leviathan', hash: '2486745106' },
  { name: 'Eater of Worlds', hash: '2659534585' },
  { name: 'Spire of Stars', hash: '700051716' },
  { name: 'Leviathan (PRESTIGE)', hash: '1130423918' },
  { name: 'Eater of Worlds (PRESTIGE)', hash: '3284024615' },
  { name: 'Spire of Stars (PRESTIGE)', hash: '3070318724' },
  { name: 'Last Wish', hash: '905240985' },
  { name: 'Scourge of the Past', hash: '1201631538' },
  { name: 'Crown of Sorrow', hash: '1815425870' },
  { name: 'Garden of Salvation', hash: '1168279855' },
  { name: 'Deep Stone Crypt', hash: '954805812' },
  { name: 'Vault of Glass', hash: '2506886274' },
  { name: 'Vow of the Disciple', hash: '3585185883' },
  { name: "King's Fall", hash: '1624029217' },
];

const WOKCommand: WokCommand = {
  category: 'profile',
  description: 'Returns a profile overview.',

  slash: true,
  testOnly: false,

  options: [
    {
      name: 'type',
      description: 'Choose a specific type of leaderboard',
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.STRING,
      choices: Choices,
      required: true,
    },
    {
      name: 'user',
      description: 'Select someone to view their profile.',
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.MENTIONABLE,
    },
  ],

  callback: async ({ interaction: msgInt, args }) => {
    LogHandler.SaveInteractionLog(msgInt);
    await msgInt.deferReply({ ephemeral: false });

    const type = args[0] || 'stats';
    const guildPlayers = await getGuildMembers(msgInt);
    if (!guildPlayers) return;
    const registeredPlayerInfo = await getRegisteredUser(msgInt, args[1]);
    if (!registeredPlayerInfo) return;

    // Add registered user if details were found.
    if (registeredPlayerInfo) {
      if (!guildPlayers.find((e) => e.membership_id === registeredPlayerInfo.membership_id)) {
        guildPlayers.push(registeredPlayerInfo);
      }
    }

    switch (type) {
      case 'stats':
        return generateProfile(msgInt, guildPlayers, registeredPlayerInfo);
      case 'raids':
        return generateRaidsProfile(msgInt, guildPlayers, registeredPlayerInfo);
      case 'broadcasts':
        return generateBroadcastsProfile(msgInt, registeredPlayerInfo);
      case 'grandmasters':
        return generateGrandmastersProfile(msgInt, registeredPlayerInfo);
    }
  },
};

const generateProfile = async (
  msgInt: CommandInteraction,
  guildPlayers: IDestinyUserLite[],
  registeredPlayerInfo: IDestinyUserLite
) => {
  const embed = primaryEmbed();
  const season_rank =
    registeredPlayerInfo.progressions[Progressions.Seasonal.CURRENT_SEASON_RANK].level +
    registeredPlayerInfo.progressions[Progressions.Seasonal.CURRENT_SEASON_RANK_OVERFLOW].level;
  const valor = registeredPlayerInfo.progressions[Progressions.Rankings.VALOR];
  const glory = registeredPlayerInfo.progressions[Progressions.Rankings.GLORY];
  const infamy = registeredPlayerInfo.progressions[Progressions.Rankings.INFAMY];
  const activeTriumphScore = registeredPlayerInfo.metrics[Metrics.Triumphs.TRIUMPH_SCORE_ACTIVE];
  const legacyTriumphScore = registeredPlayerInfo.metrics[Metrics.Triumphs.TRIUMPH_SCORE_LEGACY];
  const lifetimeTriumpScore = registeredPlayerInfo.metrics[Metrics.Triumphs.TRIUMPH_SCORE_LIFETIME];

  embed.setTitle(`Viewing Profile for ${registeredPlayerInfo.display_name}`);
  embed.setDescription(
    `Ranks are based on all tracked clans for this server. (Rank / ${guildPlayers.length}) players!`
  );
  embed.setFields([
    {
      name: 'Season Rank',
      value: `${season_rank} ${getRank(guildPlayers, registeredPlayerInfo, 'season_rank')}`,
      inline: true,
    },
    {
      name: 'Time Played',
      value: `${MiscHandler.AddCommas(Math.floor(registeredPlayerInfo.time_played / 60))} Hrs ${getRank(
        guildPlayers,
        registeredPlayerInfo,
        'time_played'
      )}`,
      inline: true,
    },
    {
      name: 'Last Played',
      value: `<t:${new Date(registeredPlayerInfo.last_played).getTime() / 1000}:D> (<t:${
        new Date(registeredPlayerInfo.last_played).getTime() / 1000
      }:R>)`,
      inline: true,
    },
    {
      name: 'Valor',
      value: `${MiscHandler.AddCommas(
        (valor?.currentProgress || 0) + (valor?.currentResetCount || 0) * 10000
      )} ${getRank(guildPlayers, registeredPlayerInfo, 'valor')}`,
      inline: true,
    },
    {
      name: 'Glory',
      value: `${MiscHandler.AddCommas(glory?.currentProgress) || 0} ${getRank(
        guildPlayers,
        registeredPlayerInfo,
        'glory'
      )}`,
      inline: true,
    },
    {
      name: 'Infamy',
      value: `${MiscHandler.AddCommas(
        (infamy?.currentProgress || 0) + (infamy?.currentResetCount || 0) * 10000
      )} ${getRank(guildPlayers, registeredPlayerInfo, 'infamy')}`,
      inline: true,
    },
    {
      name: 'Active Triumph Score',
      value: `${MiscHandler.AddCommas(activeTriumphScore?.progress || 0)} ${getRank(
        guildPlayers,
        registeredPlayerInfo,
        'active_triumph_score'
      )}`,
      inline: true,
    },
    {
      name: 'Legacy Triumph Score',
      value: `${MiscHandler.AddCommas(legacyTriumphScore?.progress || 0)} ${getRank(
        guildPlayers,
        registeredPlayerInfo,
        'legacy_triumph_score'
      )}`,
      inline: true,
    },
    {
      name: 'Lifetime Triumph Score',
      value: `${MiscHandler.AddCommas(lifetimeTriumpScore?.progress || 0)} ${getRank(
        guildPlayers,
        registeredPlayerInfo,
        'lifetime_triumph_score'
      )}`,
      inline: true,
    },
    {
      name: 'Total Raids',
      value: `${MiscHandler.AddCommas(registeredPlayerInfo.computed_data.totalRaids || 0)} ${getRank(
        guildPlayers,
        registeredPlayerInfo,
        'total_raids'
      )}`,
      inline: true,
    },
    {
      name: 'Total Titles',
      value: `${registeredPlayerInfo.computed_data.totalTitles || 0} ${getRank(
        guildPlayers,
        registeredPlayerInfo,
        'total_titles'
      )}`,
      inline: true,
    },
    {
      name: 'Highest Power',
      value: `${MiscHandler.AddCommas(registeredPlayerInfo.computed_data.totalLightLevel || 0)} ${getRank(
        guildPlayers,
        registeredPlayerInfo,
        'highest_power'
      )}`,
      inline: true,
    },
  ]);

  await msgInt.editReply({
    embeds: [embed],
  });
};

const generateRaidsProfile = async (
  msgInt: CommandInteraction,
  guildPlayers: IDestinyUserLite[],
  registeredPlayerInfo: IDestinyUserLite
) => {
  const embed = primaryEmbed();

  embed.setTitle(`Viewing Profile for ${registeredPlayerInfo.display_name}`);
  embed.setDescription(
    `Ranks are based on all tracked clans for this server. (Rank / ${guildPlayers.length}) players!`
  );
  embed.setFields([
    ...Raids.map((raid) => {
      const metricData = ManifestHandler.getMetric(raid.hash);
      const valueData = registeredPlayerInfo.computed_data.raidCompletions[raid.hash];
      return {
        name: raid.name,
        value: valueData
          ? `${valueData} ${getRank(guildPlayers, registeredPlayerInfo, 'raid', raid.hash)}`
          : 'Not yet calculated',
        inline: true,
      };
    }),
  ]);

  await msgInt.editReply({
    embeds: [embed],
  });
};

const generateBroadcastsProfile = async (msgInt: CommandInteraction, registeredPlayerInfo: IDestinyUserLite) => {
  const embed = primaryEmbed();

  embed.setTitle(`Viewing Profile for ${registeredPlayerInfo.display_name}`);
  embed.setDescription(
    `This only shows broadcasts whilst Marvin was tracking your clan in this server. (Capped at 15 newest broadcasts)`
  );

  const registeredPlayerBroadcasts = await getBroadcastsForUser(msgInt, registeredPlayerInfo);
  if (!registeredPlayerBroadcasts) return;
  registeredPlayerBroadcasts.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()).slice(0, 15);

  embed.addField(
    'Broadcast',
    registeredPlayerBroadcasts
      .map((broadcast) => {
        switch (broadcast.type) {
          case BroadcastType.Title:
            return ManifestHandler.getRecord(broadcast.hash)?.displayProperties?.name || 'Unknown';
          case BroadcastType.GildedTitle:
            return ManifestHandler.getRecord(broadcast.hash)?.displayProperties?.name || 'Unknown';
          case BroadcastType.Collectible:
            return ManifestHandler.getCollectible(broadcast.hash)?.displayProperties?.name || 'Unknown';
          case BroadcastType.Triumph:
            return ManifestHandler.getRecord(broadcast.hash)?.displayProperties?.name || 'Unknown';
          case BroadcastType.RecordStepObjectiveCompleted:
            return ManifestHandler.getRecord(broadcast.hash)?.displayProperties?.name || 'Unknown';
        }
      })
      .join('\n'),
    true
  );

  embed.addField(
    'Date',
    registeredPlayerBroadcasts
      .map((e) => {
        return MiscHandler.formatDate(e.date);
      })
      .join('\n'),
    true
  );

  await msgInt.editReply({
    embeds: [embed],
  });
};

const generateGrandmastersProfile = async (msgInt: CommandInteraction, registeredPlayerInfo: IDestinyUserLite) => {
  const embed = primaryEmbed();

  embed.setTitle(`Viewing Profile for ${registeredPlayerInfo.display_name}`);

  let currentGrandmasterRecords = ManifestHandler.getCurrentSeasonGrandmasters(
    PresentationNodes.Conqueror_SeasonOfPlunder
  );

  let grandmastersData = await getGrandmastersForUser(msgInt, registeredPlayerInfo, currentGrandmasterRecords);
  if (!grandmastersData) return;

  let description = 'Grandmaster Completions (Season of Plunder)\n```';
  let total = 0;

  currentGrandmasterRecords.map((record) => {
    let gmClears = grandmastersData[0][`h${record.hash}`];
    total += gmClears;
    let gmName = record.displayProperties.name.replace('Grandmaster: ', '');
    description += `${gmClears} - ${gmName}\n`;
  });

  description += `\n${total} - Total`;

  description += '```';

  embed.setDescription(description);

  await msgInt.editReply({
    embeds: [embed],
  });
};

const getRank = (guildMembers: IDestinyUserLite[], registeredPlayerInfo: IDestinyUserLite, rank: string, data?) => {
  let sortedPlayers = [];
  switch (rank) {
    case 'season_rank': {
      sortedPlayers = guildMembers.sort((a, b) => {
        let bSeasonRank =
          (b.progressions[Progressions.Seasonal.CURRENT_SEASON_RANK]?.level || 0) +
          (b.progressions[Progressions.Seasonal.CURRENT_SEASON_RANK_OVERFLOW]?.level || 0);
        let aSeasonRank =
          (a.progressions[Progressions.Seasonal.CURRENT_SEASON_RANK]?.level || 0) +
          (a.progressions[Progressions.Seasonal.CURRENT_SEASON_RANK_OVERFLOW]?.level || 0);
        return bSeasonRank - aSeasonRank;
      });
      break;
    }
    case 'time_played': {
      sortedPlayers = guildMembers.sort((a, b) => (b.time_played || 0) - (a.time_played || 0));
      break;
    }
    case 'valor': {
      sortedPlayers = guildMembers.sort(
        (a, b) =>
          (b.progressions[Progressions.Rankings.VALOR]?.currentProgress || 0) -
          (a.progressions[Progressions.Rankings.VALOR]?.currentProgress || 0)
      );
      break;
    }
    case 'glory': {
      sortedPlayers = guildMembers.sort(
        (a, b) =>
          (b.progressions[Progressions.Rankings.GLORY]?.currentProgress || 0) -
          (a.progressions[Progressions.Rankings.GLORY]?.currentProgress || 0)
      );
      break;
    }
    case 'infamy': {
      sortedPlayers = guildMembers.sort(
        (a, b) =>
          (b.progressions[Progressions.Rankings.INFAMY]?.currentProgress || 0) -
          (a.progressions[Progressions.Rankings.INFAMY]?.currentProgress || 0)
      );
      break;
    }
    case 'active_triumph_score': {
      sortedPlayers = guildMembers.sort(
        (a, b) =>
          (b.metrics[Metrics.Triumphs.TRIUMPH_SCORE_ACTIVE]?.progress || 0) -
          (a.metrics[Metrics.Triumphs.TRIUMPH_SCORE_ACTIVE]?.progress || 0)
      );
      break;
    }
    case 'legacy_triumph_score': {
      sortedPlayers = guildMembers.sort(
        (a, b) =>
          (b.metrics[Metrics.Triumphs.TRIUMPH_SCORE_LEGACY]?.progress || 0) -
          (a.metrics[Metrics.Triumphs.TRIUMPH_SCORE_LEGACY]?.progress || 0)
      );
      break;
    }
    case 'lifetime_triumph_score': {
      sortedPlayers = guildMembers.sort(
        (a, b) =>
          (b.metrics[Metrics.Triumphs.TRIUMPH_SCORE_LIFETIME]?.progress || 0) -
          (a.metrics[Metrics.Triumphs.TRIUMPH_SCORE_LIFETIME]?.progress || 0)
      );
      break;
    }
    case 'total_raids': {
      sortedPlayers = guildMembers.sort(
        (a, b) => (b?.computed_data?.totalRaids || 0) - (a?.computed_data?.totalRaids || 0)
      );
      break;
    }
    case 'total_titles': {
      sortedPlayers = guildMembers.sort(
        (a, b) => (b?.computed_data?.totalTitles || 0) - (a?.computed_data?.totalTitles || 0)
      );
      break;
    }
    case 'highest_power': {
      sortedPlayers = guildMembers.sort(
        (a, b) => (b?.computed_data?.totalLightLevel || 0) - (a?.computed_data?.totalLightLevel || 0)
      );
      break;
    }
    case 'raid': {
      sortedPlayers = guildMembers.sort(
        (a, b) => (b?.computed_data?.raidCompletions?.[data] || 0) - (a?.computed_data?.raidCompletions?.[data] || 0)
      );
      break;
    }
  }

  return `*(Rank: ${MiscHandler.addOrdinal(
    sortedPlayers.findIndex((player) => player.membership_id === registeredPlayerInfo.membership_id) + 1
  )})*`;
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
      `${msgInt.guild.id} tried to use /profile but there were no guild members found. Perhaps no clan setup?`
    );
    msgInt.editReply({ embeds: [noMembersFoundEmbed()] });
    return undefined;
  }

  const guildMembers = guildMembersCallback.data;
  return guildMembers;
};

const getRegisteredUser = async (msgInt: CommandInteraction, user_Id: string) => {
  const registeredUserCallback: IRegisteredUserCallback = await new Promise((resolve) =>
    DatabaseFunctions.getRegisteredUserById(user_Id ? user_Id : msgInt.user.id, (isError, isFound, data) => {
      resolve({ isError, isFound, data });
    })
  );

  if (registeredUserCallback.isError) {
    msgInt.editReply({ embeds: [databaseErrorEmbed()] });
    return undefined;
  }

  if (!registeredUserCallback.isFound) {
    msgInt.editReply({ embeds: [userNotRegisteredEmbed()] });
    return undefined;
  }

  const destinyUserCallback: IDestinyUserLiteCallback = await new Promise((resolve) =>
    DatabaseFunctions.getLiteUserById(registeredUserCallback.data?.[0].membership_id, (isError, isFound, data) => {
      resolve({ isError, isFound, data });
    })
  );

  if (destinyUserCallback.isError) {
    msgInt.editReply({ embeds: [databaseErrorEmbed()] });
    return undefined;
  }

  if (!destinyUserCallback.isFound) {
    msgInt.editReply({ embeds: [destinyUserNotFoundEmbed()] });
    return undefined;
  }

  const destinyUser = destinyUserCallback.data?.[0];
  return destinyUser;
};

const getBroadcastsForUser = async (msgInt: CommandInteraction, registeredPlayerInfo: IDestinyUserLite) => {
  const playerBroadcastsCallback: IDestinyUserBroadcastCallback = await new Promise((resolve) =>
    DatabaseFunctions.getUserBroadcasts(registeredPlayerInfo.membership_id, (isError, isFound, data) => {
      resolve({ isError, isFound, data });
    })
  );

  if (playerBroadcastsCallback.isError) {
    msgInt.editReply({ embeds: [databaseErrorEmbed()] });
    return undefined;
  }

  const playerBroadcasts = playerBroadcastsCallback.data || [];
  return playerBroadcasts.filter((broadcast) => broadcast.guild_id === msgInt.guild.id);
};

const getGrandmastersForUser = async (
  msgInt: CommandInteraction,
  registeredPlayerInfo: IDestinyUserLite,
  grandmasterRecords: DestinyRecordDefinition[]
) => {
  const playerGrandmastersCallback: ICallback = await new Promise((resolve) =>
    DatabaseFunctions.getUserGrandmasters(
      registeredPlayerInfo.membership_id,
      grandmasterRecords,
      (isError, isFound, data) => {
        resolve({ isError, isFound, data });
      }
    )
  );

  if (playerGrandmastersCallback.isError) {
    msgInt.editReply({ embeds: [databaseErrorEmbed()] });
    return undefined;
  }

  return playerGrandmastersCallback.data || [];
};

export default WOKCommand;
