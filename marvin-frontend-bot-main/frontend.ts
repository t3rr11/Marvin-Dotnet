import DiscordJS, { AutocompleteInteraction, Intents } from 'discord.js';
import { StartConnection, IConnectionStatus } from './src/handlers/database.handler';
import * as ManifestHandler from './src/handlers/manifest.handler';
import * as DatabaseFunctions from './src/handlers/database.functions';
import * as BroadcastHandler from './src/handlers/broadcast.handler';
import * as AnnouncementsHandler from './src/handlers/announcements.handler';
import * as APIHandler from './src/handlers/api.handler';
import * as MiscHandler from './src/handlers/misc.handler';
import { SaveLog } from './src/handlers/log.handler';
import { ErrorHandler } from './src/handlers/error.handler';
import { errorEmbed, primaryEmbed } from './src/handlers/embed.handler';
import WOKCommands from 'wokcommands';
import discordModals from 'discord-modals';
import path from 'path';
import dotenv from 'dotenv';
import { sleep } from './src/handlers/misc.handler';
dotenv.config();
process.env['NODE_TLS_REJECT_UNAUTHORIZED'] = '0';

// Create a new Discord client
const client = new DiscordJS.Client({
  intents: [Intents.FLAGS.GUILDS, Intents.FLAGS.GUILD_MESSAGES],
});

// Global variables
let DiscordReady: boolean = false;
let DatabaseReady: boolean = false;
let IsBackendDown: boolean = false;
let IsPatreonBackendDown: boolean = false;
let CommandsInput: number = 0;
let BotStarted: boolean = false;

// Startup - Connect to the database
StartConnection().then((ConnectionStatus: IConnectionStatus) => {
  DatabaseReady = true;
});

//Make sure before doing anything that we are connected to the database. Run a simple interval check that ends once it's connected.
ManifestHandler.checkManifestUpdate('');
let startupCheck = setInterval(async function Startup() {
  if (DiscordReady && DatabaseReady && ManifestHandler.checkManifestMounted()) {
    console.log('Manifest has been mounted.');
    clearInterval(startupCheck);

    Ready();
  }
}, 1000);

const UpdateActivityList = () => {
  var ActivityList = [];
  // ActivityList.push(`Serving ${client.guilds.cache.reduce((a, g) => a + g.memberCount, 0)} users`);
  // ActivityList.push(`Use /help to see commands`);
  // ActivityList.push(`Consider supporting? /donate`);
  ActivityList.push(
    `Marvin is coming to an end March 2023. More info here: https://twitter.com/Marvindotgg/status/1608801608829394946`
  );
  var activity = ActivityList[Math.floor(Math.random() * ActivityList.length)];
  client.user.setActivity(activity);
};

const Ready = async () => {
  if (BotStarted) return;
  console.log('Frontend is connected and ready.');

  if (!JSON.parse(process.env.TESTING) && !BotStarted) {
    // Get every 10 minute interval
    var now = new Date();
    var min = now.getMinutes();
    var startIn = 10 - (min % 10);

    // Check guilds and add/disable guilds that don't match
    await CheckGuilds(client);

    // Track and report status changes for backend.
    // setInterval(() => CheckBackends(), 1000 * 10);

    // On every 10th minute
    const run10MinuteIntervalTasks = () => {
      // Run once, then start interval
      LogStatus();
      setInterval(() => LogStatus(), 1000 * 60 * 10);
    };

    // Loops
    setTimeout(run10MinuteIntervalTasks, startIn * 60 * 1000);
    setInterval(() => BroadcastHandler.handle(client), 1000 * 120);
    setInterval(() => UpdateActivityList(), 1000 * 20); // Every 20 seconds
  }

  ResetHandler();
  UpdateActivityList();
};

const LogStatus = async () => {
  const scanTime = await new Promise((resolve) =>
    DatabaseFunctions.getScanTime((isError, isFound, scanTime: { time_passed: number }[]) => {
      if (isError || !isFound) resolve([{ time_passed: 0 }, { time_passed: 0 }]);
      resolve(scanTime);
    })
  );

  const playerCount = await new Promise((resolve) =>
    DatabaseFunctions.getPlayerCount((isError, isFound, playerCount: { sum: number }[]) => {
      if (isError || !isFound) resolve([{ sum: 0 }]);
      resolve(playerCount);
    })
  );

  DatabaseFunctions.getClanCount(async (isError, isFound, clans: any[]) => {
    if (isError || !isFound || clans.length === 0) return;

    const payload = {
      MemberCount: client.guilds.cache.reduce((a, g) => a + g.memberCount, 0),
      GuildCount: client.guilds.cache.size,
      CommandsInput: CommandsInput,
      GeneralClanCount: clans.filter((clan) => !clan.patreon && clan.is_tracking).length,
      PatreonClanCount: clans.filter((clan) => clan.patreon && clan.is_tracking).length,
      NotTrackedClanCount: clans.filter((clan) => !clan.is_tracking).length,
      OnlinePlayers: playerCount[0].sum || 0,
      GeneralScanTime: scanTime[0].time_passed || 0,
      PatreonScanTime: scanTime[1].time_passed || 0,
    };

    await DatabaseFunctions.addSystemLog(
      {
        payload: JSON.stringify(payload),
        log_type: 1,
        source: 'Marvin.Discord.Bot',
      },
      () => {}
    );

    // Reset values
    CommandsInput = 0;
  });
};

// Once bot has connected to Discord
client.once('ready', async () => {
  console.log('Connected to Discord');
  DiscordReady = true;

  // Link client to discord-modals.
  discordModals(client);

  // Link client to WOKCommands.
  new WOKCommands(client, {
    commandsDir: path.join(__dirname, 'src/commands'),
    typeScript: true,
    // testServers: [process.env.TEST_GUILD_ID],
    botOwners: ['194972321168097280', '261497385274966026'],
  });
});

// Handle reset functions
function ResetHandler() {
  // Define Reset Time and Weekly Reset as today at 17:00 UTC and 17:00 UTC on Tuesday
  var timeNow = Date.now();
  var resetTime = new Date().setUTCHours(17, 0, 0, 0);
  let resetOffset = 1000 * 60 * 15;
  let trueReset: number;

  if (timeNow > resetTime) {
    trueReset = new Date(resetTime).setDate(new Date(resetTime).getUTCDate() + 1);
  } else {
    trueReset = resetTime;
  }

  let millisUntilReset = trueReset - timeNow;

  SaveLog('Frontend', 'Info', `Next reset: ${new Date(trueReset).toUTCString()}`);
  SaveLog('Frontend', 'Info', `Time until: ${MiscHandler.formatTime('big', millisUntilReset / 1000)}`);

  if (!JSON.parse(process.env.TESTING)) {
    // Define daily reset functions
    setTimeout(() => {
      SaveLog('Frontend', 'Info', `Fired the daily reset handler: ${new Date().toUTCString()}`);

      // Send daily broadcasts for the first time.
      // AnnouncementsHandler.sendDailyLostSectorBroadcasts(client);
      // AnnouncementsHandler.sendDailyWellspringBroadcasts(client);
      AnnouncementsHandler.updateDailyVendors(client, new Date(trueReset));
      AnnouncementsHandler.updateXurVendor(client, new Date(trueReset));

      // Reset the handler for tomorrow.
      ResetHandler();
    }, millisUntilReset + resetOffset);

    // GOTCHA: Temp fix for announcements
    AnnouncementsHandler.updateDailyVendors(client, new Date(trueReset));
    AnnouncementsHandler.updateXurVendor(client, new Date(trueReset));
  }
}

async function CheckGuilds(client: DiscordJS.Client) {
  const InTheseGuilds = client.guilds.cache.toJSON().map((e) => e.id);
  const DatabaseGuilds: {
    isError: boolean;
    isFound: boolean;
    data: [{ guild_id: string; is_tracking: boolean; guild_name: string }];
  } = await new Promise((resolve) =>
    DatabaseFunctions.getListOfGuildIds((isError, isFound, data) => {
      resolve({ isError, isFound: true, data: data ? data : [] });
    })
  );

  if (DatabaseGuilds.isError) {
    SaveLog(
      'Frontend',
      'Error',
      'Failed to retrieve a list of guilds for the check guilds startup function, retrying in 30s'
    );
    await sleep(1000 * 30);
    CheckGuilds(client);

    return undefined;
  }

  if (InTheseGuilds.length > 0) {
    InTheseGuilds.forEach((guild_id) => {
      if (!DatabaseGuilds.data.find((guild) => guild.guild_id === guild_id)) {
        // Add Guild
        SaveLog('Frontend', 'Error', `Guild (${guild_id}) was not found in database, must be new. Adding entry now.`);
        AddGuild(client.guilds.cache.find((guild) => guild.id === guild_id));
      }
    });
  }

  if (DatabaseGuilds.data.length > 0) {
    DatabaseGuilds.data.forEach((guild) => {
      if (!InTheseGuilds.find((guild_id) => guild_id === guild.guild_id) && guild.is_tracking) {
        // Remove Guild
        SaveLog(
          'Frontend',
          'Error',
          `Guild (${guild.guild_id}) was not found in discord guild cache, must be gone. Untracking entry now.`
        );
        RemoveGuild({
          name: guild.guild_name,
          id: guild.guild_id,
        } as DiscordJS.Guild);
      }
      if (InTheseGuilds.find((guild_id) => guild_id === guild.guild_id) && !guild.is_tracking) {
        // Re-enable tracking
        SaveLog(
          'Frontend',
          'Error',
          `Guild (${guild.guild_id}) was found in database, but isn't being tracked, must have come back. Re-enabling tracking.`
        );
        AddGuild(client.guilds.cache.find((g) => g.id === guild.guild_id));
      }
    });
  }
}

// Joined a server
client.on('guildCreate', (guild) => AddGuild(guild));
async function AddGuild(guild: DiscordJS.Guild) {
  try {
    SaveLog('Frontend', 'Server', `Joined a new guild: ${guild.name} (${guild.id})`);
    DatabaseFunctions.updateGuildTracking(guild as any, true, function enableGuildTracking(isError, isFound, data) {
      if (!isError) {
        if (isFound) {
          SaveLog('Frontend', 'Server', `Tracking Re-Enabled: ${guild.name} (${guild.id})`);
        } else {
          const embed = new DiscordJS.MessageEmbed()
            .setColor(0x0099ff)
            .setTitle('Hey there!')
            .setDescription(
              "I am Marvin. To set me up first register with me by using the `~Register example` command. Replace example with your in-game username. \n\nOnce registration is complete use the `~Set clan` command and **then wait 5 minutes** whilst I scan your clan. That's it you'll be ready to go! \n\nTry out clan broadcasts this can be set up by typing `~Set Broadcasts #general` (does not have to be general). \n\nSee `~help` to see what I can do!"
            )
            .setFooter({
              text: process.env.DEFAULT_FOOTER,
              iconURL: process.env.DEFAULT_LOGO_URL,
            })
            .setTimestamp();
          try {
            getDefaultChannel(guild).send({ embed });
          } catch (err) {
            SaveLog('Frontend', 'Error', `Failed to give welcome message to: ${guild.name} (${guild.id})`);
          }
        }
      }
    });
  } catch (err) {
    console.log('Failed to re-enable tracking for a clan.');
  }
}

// Removed from a server
client.on('guildDelete', (guild) => RemoveGuild(guild));
async function RemoveGuild(guild: DiscordJS.Guild) {
  SaveLog('Frontend', 'Server', `Left a guild: ${guild.name} (${guild.id})`);
  DatabaseFunctions.updateGuildTracking(guild as any, false, function disableGuildTracking(isError, isFound, data) {
    if (!isError) {
      if (isFound) {
        SaveLog('Frontend', 'Server', `Tracking Disabled: ${guild.name} (${guild.id})`);
      }
    } else {
      console.log(data);
      ErrorHandler('High', `Failed to disable guild tracking for ${guild.id}`);
    }
  });
}

// Listen for autocomplete events.
client.on('interactionCreate', async (int) => {
  // Disable telesto broadcast (event over)
  // if(Math.floor(Math.random() * 6) === 5) {
  //   BroadcastHandler.sendCustomTelstoBroadcast(client, int);
  // }

  CommandsInput++;
  if (int.type !== 'APPLICATION_COMMAND_AUTOCOMPLETE') return;

  const ref = int as AutocompleteInteraction;
  const itemAutocompleteCommands = ['data', 'track', 'untrack', 'item'];
  const titleAutocompleteCommands = ['title'];

  if (itemAutocompleteCommands.includes(ref.commandName)) {
    if (!ref.options.resolved) {
      let focusedValue = ref.options.getFocused();

      ref.respond([
        ...ManifestHandler.searchForCollectible(focusedValue.toString()).map((collectible) => {
          const item = ManifestHandler.getItem(collectible.itemHash.toString());
          return {
            name: `${collectible.displayProperties.name} (${item.itemTypeAndTierDisplayName})`,
            value: collectible.hash.toString(),
          };
        }),
      ]);
    }
  } else if (titleAutocompleteCommands.includes(ref.commandName)) {
    if (!ref.options.resolved) {
      let focusedValue = ref.options.getFocused();

      let searchResults = await ManifestHandler.searchForTitle(focusedValue.toString())
        .filter((e) => e?.titleInfo?.titlesByGenderHash)
        .map((record) => {
          const titleName = record.titleInfo.titlesByGenderHash['2204441813'];
          const titleDesc = record.displayProperties.description;

          return {
            name: `${titleName} - ${titleDesc}`.slice(0, 99) || record.hash.toString(),
            value: record.hash.toString(),
          };
        });

      ref.respond(searchResults);
    }
  }
});

const getDefaultChannel = (guild) => {
  return guild.channels.cache.find(
    (channel) => channel.type === 'text' && channel.permissionsFor(guild.me).has('SEND_MESSAGES')
  );
};

client.login(process.env.TOKEN);
