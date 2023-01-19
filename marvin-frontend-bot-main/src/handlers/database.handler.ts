import pg from 'pg';
import dotenv from 'dotenv';
dotenv.config();

export interface IConnectionStatus {
  DBConnected: boolean | undefined;
}

export let ConnectionStatus: IConnectionStatus = {
  DBConnected: false,
};

export const DB = new pg.Client({
  host: JSON.parse(process.env.TESTING) ? process.env.EXTERNAL_DST_HOST : process.env.INTERNAL_DST_HOST,
  user: process.env.DATABASE_USER,
  port: Number(process.env.DSTPORT),
  password: process.env.DATABASE_PASS,
  database: process.env.DATABASE,
});

export const StartConnection = () =>
  new Promise<IConnectionStatus>(async (resolve, reject) => {
    await StartDBConnection()
      .then((connected) => (ConnectionStatus.DBConnected = connected))
      .catch((err) => {
        console.error(err);
      });
    resolve(ConnectionStatus);
  });

const StartDBConnection = () =>
  new Promise<boolean>((resolve, reject) => {
    const startedAt = new Date().getTime();

    DB.on('error', (err) => {
      console.log('An error occurred with postgres client => ', err);

      console.log('startedAt', startedAt);
      console.log('crashedAt', new Date().getTime());

      // Reconnect
      StartConnection();
    });

    DB.connect();

    DB.query(`SELECT * FROM manifest`, (err, res) => {
      if (!err) {
        console.log(`Connected to ${process.env.DATABASE} (PostgreSQL - ${DB.host})`);
        resolve(true);
      } else {
        console.log('DB Query Err:', err);
        StartConnection();
      }
    });
  });
