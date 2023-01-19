import DiscordJS from 'discord.js';
import { ICommand } from 'wokcommands';
import * as LogHandler from '../handlers/log.handler';

export default {
  category: 'test', // Required for slash commands
  description: 'Adds two numbers together', // Required for slash commands

  slash: 'both',
  testOnly: true, // Ensure you have test servers setup

  minArgs: 2,
  expectedArgs: '<num1> <num2>',

  options: [
    {
      name: 'num1',
      description: 'The first number.',
      required: true,
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.NUMBER,
    },
    {
      name: 'num2',
      description: 'The second number',
      required: true,
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.NUMBER,
    },
    {
      name: 'size',
      description: 'How big of a leaderboard do you want? Top 10? Top 25? Top 5? (max 25)',
      type: 10, // NUMBER
      min_value: 0,
      max_value: 25,
    },
  ],

  callback: ({ interaction: msgInt, args }) => {
    LogHandler.SaveInteractionLog(msgInt);
    const num1 = parseInt(args[0]);
    const num2 = parseInt(args[1]);

    return `The sum is ${num1 + num2}`;
  },
} as ICommand;
