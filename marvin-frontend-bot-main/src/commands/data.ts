import DiscordJS from 'discord.js';
import { ICommand as WokCommand } from 'wokcommands';
import { primaryEmbed } from '../handlers/embed.handler';
import * as ManifestHandler from '../handlers/manifest.handler';
import * as LogHandler from '../handlers/log.handler';

const WOKCommand: WokCommand = {
  category: 'data',
  description: 'Grabs information about a given collectible. (Tracking Purposes)',

  slash: true,
  testOnly: false,

  options: [
    {
      name: 'name',
      description: 'Search for an collectible you wish to view data on.',
      required: true,
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.STRING,
      autocomplete: true,
    },
  ],

  callback: async ({ interaction: msgInt, args, channel }) => {
    LogHandler.SaveInteractionLog(msgInt);
    const embed = primaryEmbed();
    const collectible = ManifestHandler.getCollectible(args[0]);
    const item = ManifestHandler.getManifestItemByHash(collectible.itemHash);

    embed.setTitle(collectible.displayProperties.name);

    if (item.flavorText) {
      embed.setDescription(
        `${item.flavorText}\n\nTo enable server broadcasts for this item use: \`/track ${collectible.displayProperties.name}\``
      );
    } else {
      embed.setDescription(
        `There is no description for this item.\n\nTo enable server broadcasts for this item use: \`/track ${collectible.displayProperties.name}\``
      );
    }

    embed.setThumbnail(`https://bungie.net${collectible.displayProperties.icon}`);
    if (item.hash) {
      embed.setURL(`https://www.light.gg/db/items/${item.hash}`);
    }
    if (item.screenshot) {
      embed.setImage(`https://bungie.net${item.screenshot}`);
    }

    await msgInt
      .reply({
        embeds: [embed],
      })
      .catch((err) => LogHandler.SaveLog('Frontend', 'Error', 'Failed to send data interaction'));
  },
};

export default WOKCommand;
