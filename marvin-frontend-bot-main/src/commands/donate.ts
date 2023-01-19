import { ICommand as WokCommand } from 'wokcommands';
import { primaryEmbed } from '../handlers/embed.handler';
import * as LogHandler from '../handlers/log.handler';

const WOKCommand: WokCommand = {
  category: 'donate',
  description: 'Have a few extra dollars and you like what we do, please consider supporting.',

  slash: true,
  testOnly: false,

  callback: async ({ interaction: msgInt }) => {
    LogHandler.SaveInteractionLog(msgInt);

    let embed = primaryEmbed();
    embed.setTitle('Want to help support future updates?');
    embed.setDescription(
      `By becoming a Patreon for $2.50 USD/month, Your clan will be scanned by a more powerful version of Marvin.\n\nThis means leaderboards and broadcasts will update anywhere from instant to ~60 seconds rather than the usual scan times between 5-10 minutes.`
    );
    embed.addField('<:patreon:779549421851377665> Patreon ', 'https://www.patreon.com/Terrii');
    embed.addField('<:kofi:779548939975131157> Ko-fi ', 'https://ko-fi.com/terrii_dev');
    embed.addField('<:paypal:779549835522080768> Paypal ', 'https://paypal.me/guardianstats');

    await msgInt
      .reply({
        embeds: [embed],
      })
      .catch((err) => LogHandler.SaveLog('Frontend', 'Error', 'Failed to send donate interaction'));
  },
};

export default WOKCommand;
