import { CommandInteraction } from 'discord.js';
import { IDestinyUser, IDestinyUserLite } from '../interfaces/destiny_user.interface';
import { primaryEmbed, errorEmbed } from '../handlers/embed.handler';
import * as LogHandler from '../handlers/log.handler';

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
      const aTotal = a?.time_played || 0;
      const bTotal = b?.time_played || 0;
      return bTotal - aTotal;
    });

    embed.setTitle(`Top ${size} Overall Most Time Played Rankings`);
    embed.setDescription(
      `[Click to see full leaderboard](https://marvin.gg/leaderboards/${msgInt.guild.id}/time_played/)\n` +
        '```' +
        `Rank: Hours - Name` +
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

      return `${rank}: ${Math.floor(player?.time_played / 60 || 0).toFixed(0)} Hrs - ${serialisedName}\n`;
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

    return `\n${rank}: ${Math.floor(registeredPlayerInfo?.time_played / 60 || 0).toFixed(0)} Hrs - ${serialisedName}`;
  } else {
    return '';
  }
};
