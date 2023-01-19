import { CommandInteraction } from 'discord.js';
import { Progressions } from '../references';
import { IDestinyUser, IDestinyUserLite } from '../interfaces/destiny_user.interface';
import { primaryEmbed, errorEmbed } from '../handlers/embed.handler';
import { buildPersonalRow, mapFieldNames } from '../handlers/interaction.helper';
import * as LogHandler from '../handlers/log.handler';

const config = {
  fields: [
    {
      type: 'RankLeaderboard',
      data: [
        `progressions[${Progressions.Rankings.VALOR}].currentProgress`,
        `progressions[${Progressions.Rankings.VALOR}].currentResetCount`,
      ],
      resetInterval: 10000,
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
      const a_current_progress = a.progressions[Progressions.Rankings.VALOR]?.currentProgress || 0;
      const b_current_progress = b.progressions[Progressions.Rankings.VALOR]?.currentProgress || 0;
      const a_resets = (a.progressions[Progressions.Rankings.VALOR]?.currentResetCount || 0) * 10000;
      const b_resets = (b.progressions[Progressions.Rankings.VALOR]?.currentResetCount || 0) * 10000;
      return Number(b_current_progress + b_resets) - Number(a_current_progress + a_resets);
    });

    embed.setTitle(`Top ${size} Seasonal Valor Rankings`);
    embed.setDescription(
      `[Click to see full leaderboard](https://marvin.gg/leaderboards/${msgInt.guild.id}/valor/)\n` +
        '```' +
        `Rank: Valor (Resets) - Name` +
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
