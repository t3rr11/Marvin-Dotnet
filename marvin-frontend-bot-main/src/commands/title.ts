import DiscordJS, { ApplicationCommandOptionChoice, CommandInteraction } from 'discord.js';
import { ICommand as WokCommand } from 'wokcommands';
import { databaseErrorEmbed, errorEmbed, noMembersFoundEmbed, primaryEmbed } from '../handlers/embed.handler';
import * as ManifestHandler from '../handlers/manifest.handler';
import * as MiscHandler from '../handlers/misc.handler';
import * as DatabaseFunctions from '../handlers/database.functions';
import { IGuildCallback } from '../interfaces/guild.interface';
import { IDestinyUserLite, IDestinyUserLiteCallback } from '../interfaces/destiny_user.interface';
import * as LogHandler from '../handlers/log.handler';

interface IDestinyUserLiteWithRecordState extends IDestinyUserLite {
  record_state: number;
}

const Choices: ApplicationCommandOptionChoice[] = [
  { name: 'Has the title', value: 'has' },
  { name: 'Missing the title', value: 'missing' },
];

const WOKCommand: WokCommand = {
  category: 'title',
  description: 'Returns a list of users who have or do not have a particular title.',

  slash: true,
  testOnly: false,

  options: [
    {
      name: 'name',
      description: 'Search for a title.',
      required: true,
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.STRING,
      autocomplete: true,
    },
    {
      name: 'obtained',
      description: 'Select whether the user has or does not have the title.',
      required: true,
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.STRING,
      choices: Choices,
    },
  ],

  callback: async ({ interaction: msgInt, args, channel }) => {
    LogHandler.SaveInteractionLog(msgInt);
    await msgInt.deferReply({ ephemeral: false });
    const embed = primaryEmbed();
    const record = ManifestHandler.getRecord(args[0]);
    const obtained = args[1];
    const maxPlayers = 100;

    const guildMembers = await getGuildMembers(msgInt);
    if (!guildMembers) return;

    const guildMembersIds = guildMembers.map((p) => p.membership_id);

    DatabaseFunctions.getGuildMembersRecordState(
      guildMembersIds,
      record?.hash?.toString(),
      (isError: boolean, isFound: boolean, players?: IDestinyUserLiteWithRecordState[]) => {
        if (isError) {
          msgInt.editReply({
            embeds: [databaseErrorEmbed()],
          });
        } else if (!isFound) {
          // This is returning a error due to it should have returned all users tracked, since the filtering is done in code instead of in DB.
          msgInt.editReply({
            embeds: [errorEmbed()],
          });
        } else {
          let filteredPlayers = players.filter((player) => player.record_state);
          filteredPlayers =
            obtained === 'has'
              ? filteredPlayers.filter(
                  (player) => !MiscHandler.GetRecordState(player.record_state).objectiveNotCompleted
                )
              : filteredPlayers.filter(
                  (player) => MiscHandler.GetRecordState(player.record_state).objectiveNotCompleted
                );

          if (filteredPlayers.length === 0) {
            embed.setTitle(`No results - ${record.displayProperties.name}`);
            embed.setDescription(
              obtained === 'has'
                ? 'We found that nobody has this title yet. Go be the first!'
                : 'No-one is missing this title. Which is quite crazy tbh. I wonder if I coded this right.'
            );
            embed.setThumbnail(`https://bungie.net${record.displayProperties.icon}`);

            msgInt.editReply({
              embeds: [embed],
            });
          } else {
            embed.setTitle(
              `People who ${obtained === 'has' ? 'have' : 'do not have'} ${record.titleInfo.titlesByGender['Male']}!`
            );
            embed.setThumbnail(`https://bungie.net${record.displayProperties.icon}`);

            const chunks = MiscHandler.MakeItChunky(filteredPlayers.slice(0, maxPlayers), 2);

            for (var chunk of chunks) {
              embed.addField(
                `${obtained === 'has' ? 'Obtained' : 'Missing'}`,
                chunk.map((e) => e.display_name).join('\n'),
                true
              );
            }

            const playerCount = `${
              filteredPlayers.length > maxPlayers
                ? `${maxPlayers} / ${filteredPlayers.length}`
                : ` ${filteredPlayers.length} / ${maxPlayers}`
            }`;
            embed.setDescription(
              `This list can only show ${maxPlayers} players. There may be more not on this list depending on how many clans are tracked. ${playerCount}`
            );

            msgInt.editReply({
              embeds: [embed],
            });
          }
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
      `${msgInt.guild.id} tried to use /title but there were no guild members found. Perhaps no clan setup?`
    );
    msgInt.editReply({ embeds: [noMembersFoundEmbed()] });
    return undefined;
  }

  const guildMembers = guildMembersCallback.data;
  return guildMembers;
};

export default WOKCommand;
