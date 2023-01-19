import discordModals, { Modal, ModalSubmitInteraction, showModal, TextInputComponent } from 'discord-modals';
import { ICommand as WokCommand } from 'wokcommands';
import { primaryEmbed } from '../handlers/embed.handler';
import * as LogHandler from '../handlers/log.handler';

const WOKCommand: WokCommand = {
  category: 'request',
  description: 'Have some feedback or have you hit a snag? Feel free to send through a request!',

  slash: true,
  testOnly: false,

  callback: async ({ interaction: msgInt, client, channel }) => {
    LogHandler.SaveInteractionLog(msgInt);
    const responseEmbed = primaryEmbed()
      .setTitle('Request')
      .setDescription(
        `Your request has been sent, Thanks for your feedback or suggestion <@${msgInt.user.id}>! If you'd like to keep up to date about the status of this request here is a link to the discord. https://marvin.gg/discord`
      );

    const guild = client.guilds.cache.get(process.env.REQUEST_GUILD_ID);
    const targetChannel = guild.channels.cache.get(process.env.REQUEST_CHANNEL_ID);

    const modal = new Modal() // We create a Modal
      .setCustomId('request_modal')
      .setTitle('Request / Feedback Form')
      .addComponents(
        new TextInputComponent()
          .setCustomId('request_body')
          .setLabel('Required')
          .setStyle('LONG')
          .setMinLength(4)
          .setMaxLength(280)
          .setPlaceholder('Type your feedback or request here!')
          .setRequired(true)
      );

    await showModal(modal, {
      client: client,
      interaction: msgInt,
    });

    client.on('modalSubmit', async (modal: ModalSubmitInteraction) => {
      if (modal) {
        try {
          const requestEmbed = primaryEmbed()
            .setTitle(`Request from ${modal.user.username}#${modal.user.discriminator} (${modal.user.id})`)
            .setDescription(modal.getTextInputValue('request_body'));

          await (targetChannel as any).send({
            embeds: [requestEmbed],
          });

          await (channel as any).send({
            embeds: [responseEmbed],
          });

          modal.update({}).catch();
        } catch (err) {
          console.error('on modalSubmit', err);
        }
      }
    });
  },
};

export default WOKCommand;
