import DiscordJS, { CommandInteraction } from 'discord.js';
import { ICommand as WokCommand } from 'wokcommands';
import { databaseErrorEmbed, errorEmbed, primaryEmbed } from '../handlers/embed.handler';
import { IGuildCallback } from '../interfaces/guild.interface';
import * as ManifestHandler from '../handlers/manifest.handler';
import * as DatabaseFunctions from '../handlers/database.functions';
import * as LogHandler from '../handlers/log.handler';

const WOKCommand: WokCommand = {
  category: 'untrack',
  description: 'Removes the selected collectible to the server specific broadcasts list.',

  slash: true,
  testOnly: false,

  options: [
    {
      name: 'name',
      description: 'Search for an collectible you wish to remove from the broadcasts list for this server.',
      required: true,
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.STRING,
      autocomplete: true,
    },
  ],

  callback: async ({ interaction: msgInt, args, channel }) => {
    LogHandler.SaveInteractionLog(msgInt);
    await msgInt.deferReply({ ephemeral: false });
    const embed = primaryEmbed();
    const collectible = ManifestHandler.getCollectible(args[0]);
    const item = ManifestHandler.getManifestItemByHash(collectible.itemHash);

    const guild = await getGuild(msgInt);
    if (!guild) return;

    if (!guild.broadcasts_config.tracked_items.includes(collectible.hash)) {
      await msgInt.editReply({
        embeds: [
          errorEmbed()
            .setTitle('Item is not tracked.')
            .setDescription('This item is not being tracked. Nothing to remove.'),
        ],
      });
    } else {
      guild.broadcasts_config.tracked_items = guild.broadcasts_config.tracked_items.filter(
        (hash) => hash !== collectible.hash
      );
      DatabaseFunctions.updateGuildBroadcasts(
        guild,
        guild.guild_id,
        (isError: boolean, isFound: boolean, data?: any) => {
          if (isError) {
            msgInt.editReply({
              embeds: [databaseErrorEmbed()],
            });
          } else {
            embed.setTitle('Successfully removed!');
            embed.setDescription(
              `Successfully removed ${collectible.displayProperties.name} from this servers custom broadcasts!`
            );
            embed.setThumbnail(`https://bungie.net${collectible.displayProperties.icon}`);

            msgInt.editReply({
              embeds: [embed],
            });
          }
        }
      );
    }
  },
};

const getGuild = async (msgInt: CommandInteraction) => {
  const guildCallback: IGuildCallback = await new Promise((resolve) =>
    DatabaseFunctions.getFullGuild(msgInt.guild.id, (isError, isFound, data) => {
      resolve({ isError, isFound, data });
    })
  );

  if (guildCallback.isError) {
    msgInt.editReply({
      embeds: [databaseErrorEmbed()],
    });
    return undefined;
  } else {
    return guildCallback.data?.[0];
  }
};

export default WOKCommand;
