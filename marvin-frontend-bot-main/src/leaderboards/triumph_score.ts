import { CommandInteraction } from 'discord.js';
import { IDestinyUser, IDestinyUserLite } from '../interfaces/destiny_user.interface';
import { primaryEmbed, errorEmbed } from '../handlers/embed.handler';
import * as LogHandler from '../handlers/log.handler';
import { Metrics } from '../references';

enum TriumphTypes {
  'ACTIVE',
  'LEGACY',
  'LIFETIME',
}

const getType = (type: TriumphTypes): { name: string; link: string; hash: string } => {
  switch (type) {
    case 0:
      return {
        name: 'Active',
        link: 'active_triumph_score',
        hash: Metrics.Triumphs.TRIUMPH_SCORE_ACTIVE,
      };
    case 1:
      return {
        name: 'Legacy',
        link: 'legacy_triumph_score',
        hash: Metrics.Triumphs.TRIUMPH_SCORE_LEGACY,
      };
    case 2:
      return {
        name: 'Lifetime',
        link: 'lifetime_triumph_score',
        hash: Metrics.Triumphs.TRIUMPH_SCORE_LIFETIME,
      };
  }
};

export const generateLeaderboard = async (
  msgInt: CommandInteraction,
  guildPlayers: IDestinyUserLite[],
  registeredPlayerInfo: IDestinyUserLite,
  type: TriumphTypes
) => {
  const embed = primaryEmbed();
  const size = msgInt.options.getNumber('size') || 10;
  const triumphType: { name: string; link: string; hash: string } = getType(type);
  let sortedPlayers: IDestinyUserLite[] = [];
  try {
    sortedPlayers = guildPlayers.sort((a, b) => {
      const aTotal = a?.metrics?.[triumphType.hash]?.progress || 0;
      const bTotal = b?.metrics?.[triumphType.hash]?.progress || 0;
      return bTotal - aTotal;
    });

    embed.setTitle(`Top ${size} ${triumphType.name} Triumph Score Rankings`);
    embed.setDescription(
      `[Click to see full leaderboard](https://marvin.gg/leaderboards/${msgInt.guild.id}/${triumphType.link}/)\n` +
        '```' +
        `Rank: Triumph Score - Name` +
        `\n\n` +
        `${buildRows(sortedPlayers as IDestinyUser[], size, triumphType.hash).join('')}` +
        `${buildPersonalRow(registeredPlayerInfo as IDestinyUser, sortedPlayers as IDestinyUser[], triumphType.hash)}` +
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

const buildRows = (sortedPlayers: IDestinyUser[], size: number, triumphHash: string) => {
  return sortedPlayers
    .map((player: IDestinyUser, index) => {
      const rank = index + 1;
      const serialisedName = player.display_name.replace(/\*|\^|\~|\_|\`/g, function (x) {
        return '\\' + x;
      });

      return `${rank}: ${player?.metrics?.[triumphHash]?.progress || 0} - ${serialisedName}\n`;
    })
    .slice(0, size);
};

const buildPersonalRow = (registeredPlayerInfo: IDestinyUser, sortedPlayers: IDestinyUser[], triumphHash: string) => {
  const index = sortedPlayers.findIndex((player) => player.membership_id === registeredPlayerInfo?.membership_id);

  if (index >= 0) {
    const rank = index + 1;
    const serialisedName = registeredPlayerInfo.display_name.replace(/\*|\^|\~|\_|\`/g, function (x) {
      return '\\' + x;
    });

    return `\n${rank}: ${registeredPlayerInfo?.metrics?.[triumphHash]?.progress || 0} - ${serialisedName}`;
  } else {
    return '';
  }
};
