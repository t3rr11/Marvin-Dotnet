import { ICommand as WokCommand } from 'wokcommands';
import { primaryEmbed } from '../handlers/embed.handler';
import * as LogHandler from '../handlers/log.handler';

const WOKCommand: WokCommand = {
  category: 'tools',
  description: 'A list of other cool projects!',

  slash: true,
  testOnly: false,

  callback: async ({ interaction: msgInt }) => {
    LogHandler.SaveInteractionLog(msgInt);
    let embed = primaryEmbed();

    embed.setTitle('Want more? Here is a list of other cool things!');
    embed.setDescription(`Go check out these other cool things and tools.`);
    embed.addField('DIM', 'https://destinyitemmanager.com');
    embed.addField('Charlemagne', 'https://warmind.io');
    embed.addField('Braytech', 'https://braytech.org');
    embed.addField('Power Bars', 'https://destiny-power-bars.corke.dev/');
    embed.addField('Light.gg', 'https://light.gg');
    embed.addField(
      'Reports',
      'https://raid.report\nhttps://trials.report\nhttps://crucible.report\nhttps://grandmaster.report\nhttps://dungeon.report'
    );
    embed.addField('Collection of other tools', 'https://cosmodrome.page/');

    await msgInt
      .reply({
        embeds: [embed],
      })
      .catch((err) => LogHandler.SaveLog('Frontend', 'Error', 'Failed to send tools interaction'));
  },
};

export default WOKCommand;
