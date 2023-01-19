import { EmbedFooterData, MessageEmbed } from 'discord.js';

enum colors {
  PRIMARY = 0x0099ff, // Dodger Blue
  GOLD = 0xffe000, // Gold
  ERROR = 0xff3348, // Red
}

const defaultFooter: EmbedFooterData = {
  text: process.env.DEFAULT_FOOTER,
  iconURL: process.env.DEFAULT_LOGO_URL,
};

export const primaryEmbed = () => new MessageEmbed().setColor(colors.PRIMARY).setFooter(defaultFooter).setTimestamp();

export const alertEmbed = () => new MessageEmbed().setColor(colors.GOLD).setFooter(defaultFooter).setTimestamp();

export const errorEmbed = () =>
  new MessageEmbed()
    .setColor(colors.ERROR)
    .setFooter(defaultFooter)
    .setTimestamp()
    .setTitle('Oh no... Something went wrong')
    .setDescription('Please try again!');

export const databaseErrorEmbed = () =>
  new MessageEmbed()
    .setColor(colors.ERROR)
    .setFooter(defaultFooter)
    .setTimestamp()
    .setTitle('Oh no... Something went wrong')
    .setDescription('Hmm looks like a hitch in the database. Please try again.');

export const userNotRegisteredEmbed = () =>
  new MessageEmbed()
    .setColor(colors.ERROR)
    .setFooter(defaultFooter)
    .setTimestamp()
    .setTitle('User has not registered')
    .setDescription(
      'In order to view this information the user requested needs to have linked the Destiny account to their discord account. Without this, I cannot grab the information for that account.'
    );

export const destinyUserNotFoundEmbed = () =>
  new MessageEmbed()
    .setColor(colors.ERROR)
    .setFooter(defaultFooter)
    .setTimestamp()
    .setTitle('Destiny user not found')
    .setDescription(
      "I could not find this user, perhaps they've linked the wrong account, could be private, or just hasn't been scanned yet. Maybe give it a few and try again."
    );

export const noMembersFoundEmbed = () =>
  new MessageEmbed()
    .setColor(colors.ERROR)
    .setFooter(defaultFooter)
    .setTimestamp()
    .setTitle("Couldn't find any data")
    .setDescription(
      "I couldn't find any data, have you added a clan to this server yet? `/clan setup` if so, maybe wait a few minutes for the first scan to go through and try again."
    );

export const noCommandEmbed = () =>
  new MessageEmbed()
    .setColor(colors.ERROR)
    .setFooter(defaultFooter)
    .setTimestamp()
    .setTitle('Oh no... Something went wrong')
    .setDescription('This command does not exist.');

export const noPermissionEmbed = () =>
  new MessageEmbed()
    .setColor(colors.ERROR)
    .setFooter(defaultFooter)
    .setTimestamp()
    .setTitle('Oh no... Something went wrong')
    .setDescription(
      'You do not have permission to use this command, only the person who setup Marvin or any server Administrator can make changes.'
    );
