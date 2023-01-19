using Discord.Interactions;
using Discord.WebSocket;
using Marvin.ClanQueueServer.Models.Database;
using Marvin.DbAccess.Services;
using Marvin.DbAccess.Services.Interfaces;

namespace Marvin.ClanQueueServer.DiscordHandlers.Interactions.Modules;

[Group("db-statistics", "Collection of command related to pulling db data")]
public class DbStatisticsCommandHandler : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    private readonly GuildDbAccess _guildDbAccess;

    public DbStatisticsCommandHandler(IGuildDbAccess guildDbAccess)
    {
        _guildDbAccess = (GuildDbAccess)guildDbAccess;
    }


    private const string GetOnlineStatsQuery = @"
SELECT 
    (select sum(members_online) from clan where is_tracking = true) as currently,
    (select count(*) from destiny_user WHERE last_updated > current_date - interval '1 days') as daily,
    (select count(*) from destiny_user WHERE last_updated > current_date - interval '3 days') as halfweek,
    (select count(*) from destiny_user WHERE last_updated > current_date - interval '7 days') as weekly,
    (select count(*) from destiny_user WHERE last_updated > current_date - interval '14 days') as fortnightly,
    (select count(*) from destiny_user WHERE last_updated > current_date - interval '31 days') as monthly";

    [SlashCommand("users-online", "Gets amount of users online from different periods")]
    public async Task GetOnlineStatsFromDb()
    {
        var statsResult = await _guildDbAccess.QueryAsync<OnlineUserStats>(GetOnlineStatsQuery);
        var stats = statsResult.FirstOrDefault();

        if (stats is null)
        {
            var embed = EmbedBuilding.CreateSimpleEmbed("Stats", "Failed to get stats from db");
            await Context.Interaction.RespondAsync(embed: embed);
        }
        else
        {
            var embed = EmbedBuilding.CreateSimpleEmbed(
                "Stats",
                $"Current Marvin Playerbase\n\n```Currently:   {String.Format("{0:n0}", stats.Currently)}\nDaily:       {String.Format("{0:n0}", stats.Daily)}\nHalfweek:    {String.Format("{0:n0}", stats.Halfweek)}\nWeekly:      {String.Format("{0:n0}", stats.Weekly)}\nFortnightly: {String.Format("{0:n0}", stats.Fortnightly)}\nMonthly:     {String.Format("{0:n0}", stats.Monthly)}```");
            await Context.Interaction.RespondAsync(embed: embed);
        }
    }

}