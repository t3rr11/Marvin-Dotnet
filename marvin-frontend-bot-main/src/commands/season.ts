import { ICommand as WokCommand } from 'wokcommands';
import { primaryEmbed } from '../handlers/embed.handler';
import { ISeasonResponse } from '../interfaces/season.interface';
import * as LogHandler from '../handlers/log.handler';
import * as MiscHandler from '../handlers/misc.handler';
import * as DatabaseFunctions from '../handlers/database.functions';

const WOKCommand: WokCommand = {
  category: 'season',
  description: 'Tells you how long until the next season.',

  slash: true,
  testOnly: false,

  callback: async ({ interaction: msgInt }) => {
    LogHandler.SaveInteractionLog(msgInt);
    const embed = primaryEmbed().setTitle('Season');

    DatabaseFunctions.getSeason(async (isError, isFound, data: ISeasonResponse[]) => {
      if (!isError && isFound) {
        embed.setDescription(
          `Destiny 2 is currently in season ${data[0].id}. Season ${data[0].id + 1} starts in: ${MiscHandler.formatTime(
            'big',
            (new Date(data[0].end).getTime() - new Date().getTime()) / 1000
          )}`
        );

        await msgInt
          .reply({
            embeds: [embed],
            ephemeral: false,
          })
          .catch((err) => LogHandler.SaveLog('Frontend', 'Error', 'Failed to send season interaction'));
      }
    });
  },
};

export default WOKCommand;
