import DiscordJS, { ApplicationCommandOptionChoice, CommandInteraction } from 'discord.js';
import { ICommand as WokCommand } from 'wokcommands';
import { databaseErrorEmbed, errorEmbed, primaryEmbed } from '../handlers/embed.handler';
import { IMod, IVendorCallback } from '../interfaces/vendor.interface';
import * as DatabaseFunctions from '../handlers/database.functions';
import * as CanvasHandler from '../handlers/canvas.handler';
import * as LogHandler from '../handlers/log.handler';

const Choices: ApplicationCommandOptionChoice[] = [
  { name: "Gunsmith's Mods", value: 'Gunsmith' },
  { name: "Ada-1's Mods", value: 'Ada-1' },
  { name: 'Xur', value: 'Xur' },
];

const WOKCommand: WokCommand = {
  category: 'vendors',
  description: 'Vendors',

  slash: true,
  testOnly: false,

  minArgs: 1,
  expectedArgs: '<choice>',

  options: [
    {
      name: 'vendor',
      description: 'Choose a vendor.',
      required: true,
      type: DiscordJS.Constants.ApplicationCommandOptionTypes.STRING,
      choices: Choices,
    },
  ],

  callback: async ({ interaction: msgInt, args }) => {
    LogHandler.SaveInteractionLog(msgInt);
    await msgInt.deferReply({ ephemeral: false });
    switch (args[0]) {
      case 'Gunsmith': {
        await Gunsmith(msgInt);
        break;
      }
      case 'Ada-1': {
        await Ada1(msgInt);
        break;
      }
      case 'Xur': {
        await Xur(msgInt);
        break;
      }
      default: {
        await msgInt.editReply({
          embeds: [errorEmbed().setDescription('Command not yet implemented.')],
        });
      }
    }
  },
};

const Gunsmith = async (msgInt: CommandInteraction) => {
  const embed = primaryEmbed();
  const description = [];
  embed.setTitle(`Vendor - Gunsmith - Daily Mods`);
  description.push(
    'To see who is missing these mods you can use the item command with the `obtained` flag set to `false`\n'
  );

  const vendorData = await getVendor(msgInt, 'Gunsmith');
  if (!vendorData) return;

  (vendorData.additional_data as IMod[]).map((mod) => {
    description.push(`\`/item ${mod.name}\` \`{obtained : false}\``);
  });

  const canvas = await CanvasHandler.buildModCanvasBuffer(vendorData.vendor, vendorData);
  const attachment = new DiscordJS.MessageAttachment(canvas, 'mods.png');
  embed.setImage('attachment://mods.png');

  await msgInt.editReply({
    embeds: [embed.setDescription(description.join('\n'))],
    files: [attachment],
  });
};

const Ada1 = async (msgInt: CommandInteraction) => {
  const embed = primaryEmbed();
  const description = [];
  embed.setTitle(`Vendor - Ada-1 - Daily Mods`);
  description.push(
    'To see who is missing these mods you can use the item command with the `obtained` flag set to `false`\n'
  );

  const vendorData = await getVendor(msgInt, 'Ada-1');
  if (!vendorData) return;

  (vendorData.additional_data as IMod[]).map((mod) => {
    description.push(`\`/item ${mod.name}\` \`{obtained : false}\``);
  });

  const canvas = await CanvasHandler.buildModCanvasBuffer(vendorData.vendor, vendorData);
  const attachment = new DiscordJS.MessageAttachment(canvas, 'mods.png');
  embed.setImage('attachment://mods.png');

  await msgInt.editReply({
    embeds: [embed.setDescription(description.join('\n'))],
    files: [attachment],
  });
};

const Xur = async (msgInt: CommandInteraction) => {
  const embed = primaryEmbed();

  const vendorData = await getVendor(msgInt, 'Xûr');
  if (!vendorData) return;

  const { attachment, xurEmbed } = await CanvasHandler.buildXurCanvasBuffer(embed, vendorData, vendorData.location);

  await msgInt.editReply({
    embeds: [xurEmbed],
    files: [attachment],
  });
};

const getVendor = async (msgInt: CommandInteraction, vendor: string) => {
  const vendorCallback: IVendorCallback = await new Promise((resolve) =>
    DatabaseFunctions.getVendor(vendor, (isError, isFound, data) => {
      resolve({ isError, isFound, data });
    })
  );

  if (vendorCallback.isError) {
    await msgInt.editReply({
      embeds: [databaseErrorEmbed()],
    });
    return undefined;
  } else {
    return vendorCallback.data?.[0];
  }
};

export default WOKCommand;
