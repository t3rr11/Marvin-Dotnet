import DiscordJS, { MessageActionRow, MessageSelectMenu, SelectMenuInteraction } from 'discord.js';
import { ICommand as WokCommand } from 'wokcommands';
import { ServerResponse, UserInfoCard, UserSearchResponse } from 'bungie-api-ts/user';
import { membershipDiscordEmoji } from '../handlers/interaction.helper';
import { errorEmbed, primaryEmbed } from '../handlers/embed.handler';
import * as DatabaseFunctions from '../handlers/database.functions';
import * as RequestHandler from '../handlers/api.handler';
import * as LogHandler from '../handlers/log.handler';
import { IRegisteredUser } from '../interfaces/registered_user.interface';

const WOKCommand: WokCommand = {
  category: 'register',
  description: 'Link your Destiny 2 account to Marvin',

  slash: true,
  testOnly: false,

  options: [
    {
      name: 'name',
      description: 'Search for your bungie name, usually something along the lines of Marvin#1234',
      required: true,
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.STRING,
    },
  ],

  callback: async ({ interaction: msgInt, args, channel }) => {
    LogHandler.SaveInteractionLog(msgInt);
    let lookupResults: UserInfoCard[] = [];

    const registerEmbed = primaryEmbed()
      .setTitle('Register')
      .setDescription('Looking for any accounts that match. Gimme a sec...');

    await msgInt
      .reply({
        embeds: [registerEmbed],
        ephemeral: true,
      })
      .catch((err) => LogHandler.SaveLog('Frontend', 'Error', 'Failed to send register interaction'));

    if (args[0].includes('#')) {
      // If the user made a bungie name and hash lookup use this method of search.
      await RequestHandler.SearchDestinyPlayer(
        encodeURIComponent(args[0]),
        async (isError, data: ServerResponse<UserInfoCard[]>) => {
          if (!isError) {
            lookupResults = data.Response;
            if (lookupResults.length === 0) {
              NoResults(msgInt, args[0]);
            } else if (lookupResults.length === 1) {
              FoundAMatch(msgInt, lookupResults[0]);
            } else {
              FoundAFew(msgInt, lookupResults);
            }
          } else {
            if (data.ErrorCode === 217) {
              NoResults(msgInt, args[0]);
            } else {
              console.log(isError, data);
              ReturnError(msgInt, data.Message);
            }
          }
        }
      );
    } else {
      // If they are not using their bungie name and hash, then do a prefix lookup instead.
      await RequestHandler.SearchPrefixDestinyPlayer(
        encodeURIComponent(args[0]),
        async (isError, data: ServerResponse<UserSearchResponse>) => {
          if (!isError) {
            lookupResults = data.Response.searchResults.flatMap((user) => user.destinyMemberships);
            if (lookupResults.length === 0) {
              NoResults(msgInt, args[0]);
            } else if (lookupResults.length === 1) {
              FoundAMatch(msgInt, lookupResults[0]);
            } else {
              FoundAFew(msgInt, lookupResults);
            }
          } else {
            if (data.ErrorCode === 217) {
              NoResults(msgInt, args[0]);
            } else {
              console.log(isError, data);
              ReturnError(msgInt, data.Message);
            }
          }
        }
      );
    }

    const filter = (btnInt: SelectMenuInteraction) => {
      return msgInt.user.id === btnInt.user.id;
    };
    const collector = (channel as any).createMessageComponentCollector({
      filter,
      time: 1000 * 60 * 5,
      max: 1,
    });

    collector.on('collect', async (int: SelectMenuInteraction) => {
      if (!int.isSelectMenu) return;
      if (int) {
        try {
          const selectedMembershipId = int.values[0];
          const userResult = lookupResults.find((e) => e.membershipId === selectedMembershipId);

          if (!userResult) ReturnError(msgInt, `Could not find the selected membership_id: ${selectedMembershipId}`);

          switch (int.customId) {
            case 'select_a_platform_menu': {
              FoundAMatch(msgInt, userResult);
              break;
            }
            case 'select_a_user_menu': {
              FoundAMatch(msgInt, userResult);
              break;
            }
          }

          int.update({}).catch();
        } catch (err) {
          console.error('Register select menu collector', err);
        }
      }
    });
  },
};

async function FoundAMatch(msgInt: DiscordJS.CommandInteraction<DiscordJS.CacheType>, user: UserInfoCard) {
  // Check for user card
  if (!user.bungieGlobalDisplayName) {
    ReturnError(msgInt);
    return;
  }

  // Found a match
  const foundAMatchEmbed = primaryEmbed()
    .setTitle('Successfully registered.')
    .setDescription(
      `Your username has been set to: ${user.bungieGlobalDisplayName}#${user.bungieGlobalDisplayNameCode}.`
    );

  const userEntry: IRegisteredUser = {
    membership_id: user.membershipId,
    platform: user.membershipType,
    user_id: msgInt.user.id,
    username: `${user.bungieGlobalDisplayName}#${user.bungieGlobalDisplayNameCode}`,
  };

  DatabaseFunctions.addRegisteredUser(userEntry, async (isError, isFound, data) => {
    if (!isError) {
      await msgInt.editReply({
        embeds: [foundAMatchEmbed],
        components: [],
      });
    } else {
      LogHandler.SaveLog('Frontend', 'Error', `Error registering the user: ${JSON.stringify(userEntry)}`);
      ReturnError(msgInt);
    }
  });
}

async function FoundAFew(msgInt: DiscordJS.CommandInteraction<DiscordJS.CacheType>, users: UserInfoCard[]) {
  const foundAFewEmbed = primaryEmbed()
    .setTitle('Please pick a user')
    .setDescription('Looks like your name is quite popular, here are the ones we found.');

  const selectRow = new MessageActionRow().addComponents(
    new MessageSelectMenu()
      .setCustomId('select_a_user_menu')
      .setPlaceholder('Select a user')
      .setOptions(
        ...users.map((user) => {
          return {
            label: `${user.bungieGlobalDisplayName}#${user.bungieGlobalDisplayNameCode}`,
            value: user.membershipId.toString(),
            emoji: membershipDiscordEmoji(user.membershipType),
          };
        })
      )
  );

  await msgInt.editReply({
    embeds: [foundAFewEmbed],
    components: [selectRow],
  });
}

async function NoResults(msgInt: DiscordJS.CommandInteraction<DiscordJS.CacheType>, name: string) {
  // If found no results, return no results embed.
  const noResultsEmbed = errorEmbed()
    .setTitle('No results...')
    .setDescription(
      `Could not find a Destiny 2 account that matches: ${name}. Please double check try again.\n\n**Hint**\nBungie given names which look a little something like this. Marvin#1234 usually have more success.`
    );

  await msgInt.editReply({
    embeds: [noResultsEmbed],
    components: [],
  });
}

async function ReturnError(msgInt: DiscordJS.CommandInteraction<DiscordJS.CacheType>, message?: string) {
  // Error happened. Tell em to try again.
  console.error(message);
  await msgInt.editReply({
    embeds: [message ? errorEmbed().setDescription(message) : errorEmbed()],
    components: [],
  });
}

export default WOKCommand;
