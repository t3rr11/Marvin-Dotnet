import { MessageActionRow, MessageButton, MessageComponentInteraction } from 'discord.js';
import { ICommand as WokCommand } from 'wokcommands';
import { primaryEmbed } from '../handlers/embed.handler';
import * as LogHandler from '../handlers/log.handler';

const WOKCommand: WokCommand = {
  category: 'help',
  description: 'Need help? We gotcha!',

  slash: true,
  testOnly: false,

  callback: async ({ interaction: msgInt, channel }) => {
    LogHandler.SaveInteractionLog(msgInt);

    const embed = primaryEmbed()
      .setTitle('Help - Setup')
      .setDescription(
        "To setup, follow these steps; \n\n- Use `/Register` (this will link your destiny account to your discord account).\n- Then use `/clan setup` (this will link your clan to discord)\n- **wait 15 minutes or so** whilst I scan your clan.\n- Then that's it you'll be ready to go! \n\nTo set up broadcasts use `/broadcasts channel` and then use `/broadcasts manage` to manage the types of broadcasts that can come through."
      );

    await msgInt
      .reply({
        embeds: [embed],
        components: [helpButtonsRow1, helpButtonsRow2, linkButton],
        ephemeral: false,
      })
      .catch((err) => LogHandler.SaveLog('Frontend', 'Error', 'Failed to send help interaction'));

    const filter = (btnInt: MessageComponentInteraction) => {
      return msgInt.user.id === btnInt.user.id;
    };
    const collector = (channel as any).createMessageComponentCollector({
      filter,
      time: 1000 * 60 * 5,
    });

    collector.on('collect', async (int: MessageComponentInteraction) => {
      // Remove previous fields before editing message
      embed.setFields([]);

      if (int) {
        try {
          switch (int.customId) {
            case 'help_button_setup': {
              embed.setTitle('Help - Setup');
              embed.setDescription(
                "To setup, follow these steps; \n\n- Use `/Register` (this will link your destiny account to your discord account).\n- Then use `/clan setup` (this will link your clan to discord)\n- **wait 15 minutes or so** whilst I scan your clan.\n- Then that's it you'll be ready to go! \n\nTo set up broadcasts use `/broadcast channel` and then use `/broadcast manage` to manage the types of broadcasts that can come through."
              );

              await msgInt
                .editReply({
                  embeds: [embed],
                })
                .catch((err) =>
                  LogHandler.SaveLog(
                    'Frontend',
                    'Error',
                    'Failed to edit reply for the help collector setup button interaction'
                  )
                );
              break;
            }
            case 'help_button_commands': {
              embed.setTitle('Help - Commands');
              embed.setDescription('Here is a list of commands! Example: `/leaderboard valor`');
              embed.addField(
                'Commands',
                [
                  '`/leaderboard valor`',
                  '`/leaderboard glory`',
                  '`/leaderboard infamy`',
                  '`/leaderboard saint14`',
                  '`/raid {choice}`',
                  '`/dungeon {choice}`',
                  '`/profile {choice}`',
                ].join('\n')
              );

              await msgInt
                .editReply({
                  embeds: [embed],
                })
                .catch((err) =>
                  LogHandler.SaveLog(
                    'Frontend',
                    'Error',
                    'Failed to edit reply for the help collector commands button interaction'
                  )
                );
              break;
            }
            case 'help_button_broadcasts': {
              embed.setTitle('Help - Broadcasts');
              embed.setDescription(
                "Broadcasts are clan achievements that are announced to the selected channel, things like titles obtained by clannies, or rare weapons like raid exotics, and even things like clan level ups.\n\nTo enable broadcasts use `/broadcast channel` this will give you a dropdown where you can select a channel you'd like Marvin to announce them to.\n\nTo manage which broadcasts get send use `/broadcast settings`\n\nIf there is an item that isn't being broadcast that you want to broadcast, you can add them by using `/track`."
              );

              await msgInt
                .editReply({
                  embeds: [embed],
                })
                .catch((err) =>
                  LogHandler.SaveLog(
                    'Frontend',
                    'Error',
                    'Failed to edit reply for the help collector broadcasts button interaction'
                  )
                );
              break;
            }
            case 'help_button_announcements': {
              embed.setTitle('Help - Announcements');
              embed.setDescription(
                "**Announcements are disabled globally, sadly this feature is not yet ready**\n\nAnnouncments are are daily messages that are sent to the selected channel every reset to show things like mod rotations, lost sector rotation or even Xur when he appears.\n\nTo enable announcements use `/announcement channel` this will give you a dropdown where you can select a channel you'd like Marvin to announce them to.\n\nTo manage which announcements get send use `/announcement manage`"
              );

              await msgInt
                .editReply({
                  embeds: [embed],
                })
                .catch((err) =>
                  LogHandler.SaveLog(
                    'Frontend',
                    'Error',
                    'Failed to edit reply for the help collector announcements button interaction'
                  )
                );
              break;
            }
            case 'help_button_items': {
              embed.setTitle('Help - Items');
              embed.setDescription(
                'This command is used to show who has any given item. You can use the item command on any profile collectible that is found in the Destiny 2 collections tab. `/item Anarchy` will return a list of people who own the anarchy item.'
              );

              await msgInt
                .editReply({
                  embeds: [embed],
                })
                .catch((err) =>
                  LogHandler.SaveLog(
                    'Frontend',
                    'Error',
                    'Failed to edit reply for the help collector items button interaction'
                  )
                );
              break;
            }
            case 'help_button_titles': {
              embed.setTitle('Help - Titles');
              embed.setDescription(
                'This command is used to show who has any given title. e.g `/title Rivensbane` will return a list of people who have obtained the Rivensbane title.'
              );

              await msgInt
                .editReply({
                  embeds: [embed],
                })
                .catch((err) =>
                  LogHandler.SaveLog(
                    'Frontend',
                    'Error',
                    'Failed to edit reply for the help collector titles button interaction'
                  )
                );
              break;
            }
            case 'help_button_others': {
              embed.setTitle('Help - Others');
              embed.setDescription(
                'Here is a list of other commands!\n\n `/donate` - To donate to the bot developer, helps keep things running.\n`/request` - To request a new feature, or to report a bug.\n`/tools` - A bunch of other tools from other developers.'
              );

              await msgInt
                .editReply({
                  embeds: [embed],
                })
                .catch((err) =>
                  LogHandler.SaveLog(
                    'Frontend',
                    'Error',
                    'Failed to edit reply for the help collector others button interaction'
                  )
                );
              break;
            }
          }

          await int.update({}).catch();
        } catch (err) {
          console.error('Help message collector', err);
        }
      }
    });
  },
};

const helpButtonsRow1 = new MessageActionRow()
  .addComponents(new MessageButton().setCustomId('help_button_setup').setLabel('Setup').setStyle('PRIMARY'))
  .addComponents(new MessageButton().setCustomId('help_button_commands').setLabel('Commands').setStyle('PRIMARY'))
  .addComponents(new MessageButton().setCustomId('help_button_broadcasts').setLabel('Broadcasts').setStyle('PRIMARY'))
  .addComponents(
    new MessageButton().setCustomId('help_button_announcements').setLabel('Announcements').setStyle('PRIMARY')
  );

const helpButtonsRow2 = new MessageActionRow()
  .addComponents(new MessageButton().setCustomId('help_button_items').setLabel('Items').setStyle('PRIMARY'))
  .addComponents(new MessageButton().setCustomId('help_button_titles').setLabel('Titles').setStyle('PRIMARY'))
  .addComponents(new MessageButton().setCustomId('help_button_others').setLabel('Others').setStyle('PRIMARY'));

const linkButton = new MessageActionRow().addComponents(
  new MessageButton().setURL('https://marvin.gg/commands').setLabel('View all commands').setStyle('LINK')
);

export default WOKCommand;
