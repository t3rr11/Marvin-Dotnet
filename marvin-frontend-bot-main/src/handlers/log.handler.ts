import * as DatabaseFunctions from './database.functions';
import * as Misc from './misc.handler';
import { CommandInteraction } from 'discord.js';
import dotenv from 'dotenv';
dotenv.config();

export const SaveLog = (location, type, log) => {
  if (location !== 'ErrorHandler') {
    console.log(Misc.GetReadableDateTime('date') + ' - ' + log);
  }
  if (!process.env.TESTING) {
    DatabaseFunctions.addLog({ location, type, log }, function AddLogToDB(isError, severity, err) {});
  }
};

export const SaveInteractionLog = (msgInt: CommandInteraction) => {
  const username = msgInt?.user?.username;
  const command = msgInt?.commandName;
  const options = msgInt?.options
    ? `${msgInt?.options?.data.map((option) => `, Type: ${option?.name}, Value: ${option?.value}`)}`
    : '';
  const log = `User: ${username}, Interaction: ${command}${options}`;
  console.log(Misc.GetReadableDateTime('date') + ' - ' + log);
  if (!JSON.parse(process.env.TESTING)) {
    DatabaseFunctions.addLog(
      {
        location: 'Frontend',
        type: 'Interaction',
        log: log,
      },
      function AddLogToDB(isError, severity, err) {}
    );
  }
};
