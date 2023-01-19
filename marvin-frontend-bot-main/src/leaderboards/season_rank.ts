import { CommandInteraction } from 'discord.js';
import { IDestinyUser, IDestinyUserLite } from '../interfaces/destiny_user.interface';
import { primaryEmbed, errorEmbed } from '../handlers/embed.handler';
import * as LogHandler from '../handlers/log.handler';
import { Progressions } from '../references';

export const generateLeaderboard = async (
  msgInt: CommandInteraction,
  guildPlayers: IDestinyUserLite[],
  registeredPlayerInfo: IDestinyUserLite
) => {
  const embed = primaryEmbed();
  const size = msgInt.options.getNumber('size') || 10;
  let sortedPlayers: IDestinyUserLite[] = [];
  try {
    sortedPlayers = guildPlayers.sort((a, b) => {
      const a_current_progress = a.progressions[Progressions.Seasonal.CURRENT_SEASON_RANK]?.currentProgress || 0;
      const b_current_progress = b.progressions[Progressions.Seasonal.CURRENT_SEASON_RANK]?.currentProgress || 0;
      const a_overflow_progress =
        a.progressions[Progressions.Seasonal.CURRENT_SEASON_RANK_OVERFLOW]?.currentProgress || 0;
      const b_overflow_progress =
        b.progressions[Progressions.Seasonal.CURRENT_SEASON_RANK_OVERFLOW]?.currentProgress || 0;
      return Number(b_current_progress + b_overflow_progress) - Number(a_current_progress + a_overflow_progress);
    });

    embed.setTitle(`Top ${size} Season Rank Rankings`);
    embed.setDescription(
      `[Click to see full leaderboard](https://marvin.gg/leaderboards/${msgInt.guild.id}/season_rank/)\n` +
        '```' +
        `Rank: Level - Name` +
        `\n\n` +
        `${buildRows(sortedPlayers as IDestinyUser[], size).join('')}` +
        `${buildPersonalRow(registeredPlayerInfo as IDestinyUser, sortedPlayers as IDestinyUser[])}` +
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

const buildRows = (sortedPlayers: IDestinyUser[], size: number) => {
  return sortedPlayers
    .map((player: IDestinyUser, index) => {
      const rank = index + 1;
      const serialisedName = player.display_name.replace(/\*|\^|\~|\_|\`/g, function (x) {
        return '\\' + x;
      });
      const seasonRank =
        (player?.progressions[Progressions.Seasonal.CURRENT_SEASON_RANK]?.level || 0) +
        (player?.progressions[Progressions.Seasonal.CURRENT_SEASON_RANK_OVERFLOW]?.level || 0);

      return `${rank}: ${seasonRank} - ${serialisedName}\n`;
    })
    .slice(0, size);
};

const buildPersonalRow = (registeredPlayerInfo: IDestinyUser, sortedPlayers: IDestinyUser[]) => {
  const index = sortedPlayers.findIndex((player) => player.membership_id === registeredPlayerInfo?.membership_id);

  if (index >= 0) {
    const rank = index + 1;
    const serialisedName = registeredPlayerInfo.display_name.replace(/\*|\^|\~|\_|\`/g, function (x) {
      return '\\' + x;
    });
    const seasonRank =
      (registeredPlayerInfo?.progressions[Progressions.Seasonal.CURRENT_SEASON_RANK]?.level || 0) +
      (registeredPlayerInfo?.progressions[Progressions.Seasonal.CURRENT_SEASON_RANK_OVERFLOW]?.level || 0);

    return `\n${rank}: ${seasonRank} - ${serialisedName}`;
  } else {
    return '';
  }
};
