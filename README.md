# Source code for Marvin

I have exluded the websites, express server and admin tools to avoid clones.

The frontend is written in Typescript and can be found in this repo: `marvin-fontend-bot-main`.
The backend is written in .NET Core 7 and is split into many different micro packages so hosting it will be quite a complex job and I wish you the best of luck.

Few things to note,
- The database was PostgreSQL, You can find some if not all the table structures in `marvin-postgresql-schemas-main/tables`.
- You'll find a repo `marvin.discord.bot-main` this was a rewrite we were working on to rewrite the bot in .NET Core 7 and has work in it but is not complete and if you want to replicate the bot that was in production, you'll want to use `marvin-fontend-bot-main` which is written in Typescript.

## 19 Jan 2023 - Terrii