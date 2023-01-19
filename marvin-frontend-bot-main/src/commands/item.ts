import DiscordJS, { ApplicationCommandOptionChoice, CommandInteraction } from 'discord.js';
import { ICommand as WokCommand } from 'wokcommands';
import { databaseErrorEmbed, errorEmbed, noMembersFoundEmbed, primaryEmbed } from '../handlers/embed.handler';
import * as ManifestHandler from '../handlers/manifest.handler';
import * as MiscHandler from '../handlers/misc.handler';
import * as DatabaseFunctions from '../handlers/database.functions';
import { IGuildCallback } from '../interfaces/guild.interface';
import { IDestinyUserLite, IDestinyUserLiteCallback } from '../interfaces/destiny_user.interface';
import { DestinyScope } from 'bungie-api-ts/destiny2';
import * as LogHandler from '../handlers/log.handler';

const Choices: ApplicationCommandOptionChoice[] = [
  { name: 'Has the item', value: 'has' },
  { name: 'Missing the item', value: 'missing' },
];

const WOKCommand: WokCommand = {
  category: 'item',
  description: 'Returns a list of users who have or do not have a particular item.',

  slash: true,
  testOnly: false,

  options: [
    {
      name: 'name',
      description: 'Search for an collectible.',
      required: true,
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.STRING,
      autocomplete: true,
    },
    {
      name: 'obtained',
      description: 'Select whether the user has or does not have the item.',
      required: true,
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.STRING,
      choices: Choices,
    },
  ],

  callback: async ({ interaction: msgInt, args, channel }) => {
    LogHandler.SaveInteractionLog(msgInt);
    await msgInt.deferReply({ ephemeral: false });
    const embed = primaryEmbed();
    const collectible = ManifestHandler.getCollectible(args[0]);
    const item = ManifestHandler.getManifestItemByHash(collectible.itemHash);
    const obtained = args[1];
    const maxPlayers = 100;

    // if(collectible.scope === DestinyScope.Character) {
    //   msgInt.editReply({
    //     embeds: [
    //       errorEmbed()
    //       .setTitle('Ahh darn! Item is character scoped.')
    //       .setDescription('Due to there being too many dang collectibles in this game, I have opted to not store character collectibles to save on storage space and to keep commands snappy quick. So for that reason I cannot return a list of who has this item as I also don\'t know. Sorry!')
    //     ]
    //   });
    //   return;
    // }

    const guildMembers = await getGuildMembers(msgInt);
    if (!guildMembers) return;

    const guildMembersIds = guildMembers.map((p) => p.membership_id);

    DatabaseFunctions.getGuildMembersItem(
      guildMembersIds,
      collectible.hash.toString(),
      obtained,
      (isError: boolean, isFound: boolean, players?: IDestinyUserLite[]) => {
        if (isError) {
          msgInt.editReply({
            embeds: [databaseErrorEmbed()],
          });
        } else if (!isFound) {
          embed.setTitle(`No results - ${collectible.displayProperties.name}`);
          embed.setDescription(
            obtained === 'has'
              ? 'We found that nobody has this item yet. Go be the first!'
              : 'No-one is missing this item. No need to show a list.'
          );
          embed.setThumbnail(`https://bungie.net${collectible.displayProperties.icon}`);

          msgInt.editReply({
            embeds: [embed],
          });
        } else {
          embed.setTitle(
            `People who ${obtained === 'has' ? 'have' : 'do not have'} ${collectible.displayProperties.name}!`
          );
          embed.setThumbnail(`https://bungie.net${collectible.displayProperties.icon}`);

          const chunks = MiscHandler.MakeItChunky(players.slice(0, maxPlayers), 2);

          for (var chunk of chunks) {
            embed.addField(
              `${obtained === 'has' ? 'Obtained' : 'Missing'}`,
              chunk.map((e) => e.display_name).join('\n'),
              true
            );
          }

          const playerCount = `${
            players.length > maxPlayers ? `${maxPlayers} / ${players.length}` : ` ${players.length} / ${maxPlayers}`
          }`;
          embed.setDescription(
            `This list can only show ${maxPlayers} players. There may be more not on this list depending on how many clans are tracked. ${playerCount}`
          );

          msgInt.editReply({
            embeds: [embed],
          });
        }
      }
    );
  },
};

const getGuildMembers = async (msgInt: CommandInteraction) => {
  const guildCallback: IGuildCallback = await new Promise((resolve) =>
    DatabaseFunctions.getGuild(msgInt.guild.id, (isError, isFound, data) => {
      resolve({ isError, isFound, data });
    })
  );

  if (guildCallback.isError) {
    msgInt.editReply({ embeds: [databaseErrorEmbed()] });
    return undefined;
  }

  const guild = guildCallback.data?.[0];

  const guildMembersCallback: IDestinyUserLiteCallback = await new Promise((resolve) =>
    DatabaseFunctions.getGuildMembers(guild.clans, (isError, isFound, data) => {
      resolve({ isError, isFound, data });
    })
  );

  if (guildMembersCallback.isError) {
    msgInt.editReply({ embeds: [databaseErrorEmbed()] });
    return undefined;
  }

  if (!guildMembersCallback.isFound) {
    LogHandler.SaveLog(
      'Frontend',
      'Error',
      `${msgInt.guild.id} tried to use /item but there were no guild members found. Perhaps no clan setup?`
    );
    msgInt.editReply({ embeds: [noMembersFoundEmbed()] });
    return undefined;
  }

  const guildMembers = guildMembersCallback.data;
  return guildMembers;
};

export default WOKCommand;
