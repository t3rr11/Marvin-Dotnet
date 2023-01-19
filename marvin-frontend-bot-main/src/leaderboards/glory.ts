import { CommandInteraction } from 'discord.js';
import { Progressions } from '../references';
import { IDestinyUser, IDestinyUserLite } from '../interfaces/destiny_user.interface';
import { primaryEmbed, errorEmbed } from '../handlers/embed.handler';
import { buildPersonalRow, mapFieldNames } from '../handlers/interaction.helper';
import * as LogHandler from '../handlers/log.handler';

const config = {
  fields: [
    {
      type: 'Leaderboard',
      data: `progressions[${Progressions.Rankings.GLORY}].currentProgress`,
    },
  ],
};

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
      const aTotal = a.progressions[Progressions.Rankings.GLORY]?.currentProgress || 0;
      const bTotal = b.progressions[Progressions.Rankings.GLORY]?.currentProgress || 0;
      return bTotal - aTotal;
    });

    embed.setTitle(`Top ${size} Seasonal Glory Rankings`);
    embed.setDescription(
      `[Click to see full leaderboard](https://marvin.gg/leaderboards/${msgInt.guild.id}/glory/)\n` +
        '```' +
        `Rank: Glory - Name` +
        `\n\n` +
        `${buildRows(config.fields, sortedPlayers as IDestinyUser[], size).join('')}` +
        `${buildPersonalRow(config.fields, registeredPlayerInfo as IDestinyUser, sortedPlayers as IDestinyUser[])}` +
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

const buildRows = (fields, sortedPlayers: IDestinyUser[], size: number) => {
  return sortedPlayers
    .map((player: IDestinyUser, index) => {
      const rank = index + 1;
      const serialisedName = player.display_name.replace(/\*|\^|\~|\_|\`/g, function (x) {
        return '\\' + x;
      });
      const leaderboard_fields = mapFieldNames(fields, player);

      return `${rank}: ${leaderboard_fields} - ${serialisedName}\n`;
    })
    .slice(0, size);
};
