import Canvas from 'canvas';
import { MessageAttachment, MessageEmbed } from 'discord.js';
import { IItem, IVendor } from '../interfaces/vendor.interface';

export const buildModCanvasBuffer = async (vendor: string, data: IVendor) => {
  //Canvasing the mod images
  const canvasWidth = 500;
  const canvasHeight = 210;
  const modWidth = 32;
  const modHeight = 32;
  const canvas = Canvas.createCanvas(canvasWidth, canvasHeight);
  const ctx = canvas.getContext('2d');

  const background = await Canvas.loadImage(`./src/images/${vendor}.png`);
  const mod1Image = await Canvas.loadImage(`https://bungie.net${data.additional_data[0]?.icon}`);
  const mod2Image = await Canvas.loadImage(`https://bungie.net${data.additional_data[1]?.icon}`);
  const mod3Image = await Canvas.loadImage(`https://bungie.net${data.additional_data[2]?.icon}`);
  const mod4Image = await Canvas.loadImage(`https://bungie.net${data.additional_data[3]?.icon}`);

  //Add Images (The spacing here is the previous mod height + the next mod height + spacing);
  ctx.drawImage(background, 0, 0, canvasWidth, canvasHeight);
  ctx.drawImage(mod1Image, 260, 32, modWidth, modHeight);
  ctx.drawImage(mod2Image, 260, 74, modWidth, modHeight);
  ctx.drawImage(mod3Image, 260, 116, modWidth, modHeight);
  ctx.drawImage(mod4Image, 260, 158, modWidth, modHeight);

  //Add Text Backgrounds
  ctx.beginPath();
  ctx.globalAlpha = 0.2;
  ctx.rect(250, 20, 230, 176);
  ctx.fill('evenodd');
  ctx.stroke();

  //Add Text
  ctx.globalAlpha = 1;
  ctx.font = '13px sans-serif';
  ctx.fillStyle = '#ffffff';
  ctx.fillText(FormatText(data.additional_data[0]?.name), 270 + 30, 52);
  ctx.fillText(FormatText(data.additional_data[1]?.name), 270 + 30, 94);
  ctx.fillText(FormatText(data.additional_data[2]?.name), 270 + 30, 136);
  ctx.fillText(FormatText(data.additional_data[3]?.name), 270 + 30, 178);

  return canvas.toBuffer();
};

export const buildXurCanvasBuffer = async (embed: MessageEmbed, data: IVendor, vendorLocation: number) => {
  //Canvasing the mod images
  const canvas = Canvas.createCanvas(500, 210);
  const ctx = canvas.getContext('2d');
  let friendlyLocation = 'Hidden';
  let locationText = "Xûr's location is hidden";

  //Add Background Image
  switch (vendorLocation) {
    case 0: {
      friendlyLocation = 'Tower';
      locationText = 'Xûr can be found in the **Tower**, near **Dead Orbit**.';
      ctx.drawImage(await Canvas.loadImage(`./src/images/xur_tower.png`), 0, 0, 500, 210);
      break;
    }
    case 1: {
      friendlyLocation = 'EDZ';
      locationText = 'Xûr can be found on **EDZ** in the **Winding Cove**.';
      ctx.drawImage(await Canvas.loadImage(`./src/images/xur_edz.png`), 0, 0, 500, 210);
      break;
    }
    case 2: {
      friendlyLocation = 'Nessus';
      locationText = "Xûr can be found in **Nessus** on a branch over in **Watcher's Grave**.";
      ctx.drawImage(await Canvas.loadImage(`./src/images/xur_nessus.png`), 0, 0, 500, 210);
      break;
    }
    default: {
      friendlyLocation = 'Hidden';
      locationText = "Xûr's location is hidden.";
      break;
    }
  }

  //Build Item
  const buildItemDesc = (item: IItem) => {
    if (item.itemType === 2) {
      const intellect = item.stats['144602215']?.value;
      const resilience = item.stats['392767087']?.value;
      const discipline = item.stats['1735777505']?.value;
      const recovery = item.stats['1943323491']?.value;
      const mobility = item.stats['2996146975']?.value;
      const strength = item.stats['4244567218']?.value;
      const total = intellect + resilience + discipline + recovery + mobility + strength;

      return `**${item.name}** - ${total}\n${mobility ? 'Mob: ' + mobility : ''}, ${
        resilience ? 'Res: ' + resilience : ''
      }, ${recovery ? 'Rec: ' + recovery : ''}\n${discipline ? 'Dis: ' + discipline : ''}, ${
        intellect ? 'Int: ' + intellect : ''
      }, ${strength ? 'Str: ' + strength : ''}\n\n`;
    } else if (item.itemType === 3) {
      const stability = item.stats['155624089']?.value;
      const handling = item.stats['943549884']?.value;
      const range = item.stats['1240592695']?.value;
      const magazine = item.stats['3871231066']?.value;
      const impact = item.stats['4043523819']?.value;
      const reload = item.stats['4188031367']?.value;
      const rpm = item.stats['4284893193']?.value;

      return `**${item.name}**\n${impact ? 'Imp: ' + impact : ''}, ${range ? 'Ran: ' + range : ''}, ${
        stability ? 'Sta: ' + stability : ''
      }, ${handling ? 'Han: ' + handling : ''}\n${reload ? 'Rel: ' + reload : ''}, ${
        magazine ? 'Mag: ' + magazine : ''
      }, ${rpm ? 'Rpm: ' + rpm : ''}\n\n`;
    } else {
      return `**${item.name}**\n\n`;
    }
  };

  //Add Image to Embed
  const attachment = new MessageAttachment(canvas.toBuffer(), 'xurLocation.png');
  embed.setImage('attachment://xurLocation.png');
  embed.setTitle(`Xûr - ${friendlyLocation}`);

  let description = [`${locationText}\n\n**Items for sale**\n\n`];

  (data.additional_data as IItem[]).map((item) => {
    description.push(buildItemDesc(item));
  });

  embed.setDescription(description.join(''));

  return { attachment, xurEmbed: embed };
};

function FormatText(string) {
  let name = string;
  if (string.split(' ').length > 3) {
    name =
      string.split(' ')[0] +
      ' ' +
      string.split(' ')[1] +
      ' ' +
      string.split(' ')[2] +
      '\n' +
      string.substr(
        (string.split(' ')[0] + ' ' + string.split(' ')[1] + ' ' + string.split(' ')[2]).length,
        string.length
      );
  }
  return name;
}
