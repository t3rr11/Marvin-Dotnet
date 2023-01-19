using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DotNetBungieAPI.Extensions;
using DotNetBungieAPI.HashReferences;
using DotNetBungieAPI.Models;
using DotNetBungieAPI.Models.Destiny.Definitions.Progressions;
using DotNetBungieAPI.Models.Destiny.Definitions.Seasons;
using DotNetBungieAPI.Service.Abstractions;
using Marvin.Bot.Commands.AutocompleteHandlers;
using Marvin.Bot.Commands.EmbedBuilders;
using Marvin.Bot.Extensions;
using Marvin.Bot.Models.Leaderboards;
using Marvin.DbAccess.EntityFramework.DbContext.Interfaces;
using Marvin.DbAccess.EntityFramework.Models.Guilds;
using Marvin.DbAccess.EntityFramework.Models.RegisteredUsers;
using Marvin.DbAccess.EntityFramework.Services.Interfaces;
using Marvin.DbAccess.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Marvin.Bot.Commands.Leaderboards;

[Group("leaderboard", "Fetches and displays respective leaderboard")]
public class LeaderboardSlashCommandHandler : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    private readonly IRawPostgresDbAccess _rawPostgresDbAccess;
    private readonly IBungieClient _bungieClient;
    private readonly IDbContextCaller _dbContextCaller;

    public LeaderboardSlashCommandHandler(
        IRawPostgresDbAccess rawPostgresDbAccess,
        IBungieClient bungieClient,
        IDbContextCaller dbContextCaller)
    {
        _rawPostgresDbAccess = rawPostgresDbAccess;
        _bungieClient = bungieClient;
        _dbContextCaller = dbContextCaller;
    }

    [SlashCommand("season_rank", "Fetches and displays season rank leaderboard")]
    public async Task GetSeasonRankLeaderboard(
        [Summary("amount")] int amount = 10,
        [Summary("season"), Autocomplete(typeof(SeasonAutocompleteHandler))]
        string seasonHash = "")
    {
        if (seasonHash == "")
        {
            seasonHash = DefinitionHashes.Seasons.SeasonoftheSeraph_2809059432.ToString();
        }

        if (!_bungieClient.Repository.TryGetDestinyDefinition<DestinySeasonDefinition>(
                uint.Parse(seasonHash),
                BungieLocales.EN,
                out var seasonDefinition))
        {
            await Context.Interaction.RespondAsync(
                embed: EmbedBuilders.Embeds.GetBasicEmbed(
                    "Error",
                    "Couldn't fetch season definition"));
        }

        if (!seasonDefinition.SeasonPass.HasValidHash)
        {
            await Context.Interaction.RespondAsync(
                embed: EmbedBuilders.Embeds.GetBasicEmbed(
                    $"{seasonDefinition.DisplayProperties.Name} Rankings",
                    "This season doesn't have any associated rankings"));
        }

        var progressions = seasonDefinition
            .SeasonPass
            .Select(x => new { x.RewardProgression, x.PrestigeProgression });

        var clanIds = await _dbContextCaller.GetFromDbContext(
            async (IDbContext<GuildDbModel> db, CancellationToken ct) =>
            {
                return await db
                    .Set1
                    .Where(x => x.GuildId == Context.Guild.Id)
                    .Select(x => x.Clans)
                    .FirstOrDefaultAsync(ct);
            });

        var linkedUserId = await _dbContextCaller.GetFromDbContext<RegisteredUserDbModel, long?>(async (db, ct) =>
        {
            return await db
                .Set1
                .Where(x => x.UserId == Context.User.Id)
                .Select(x => x.MembershipId)
                .FirstOrDefaultAsync(ct);
        });

        var result = await _rawPostgresDbAccess
            .QueryAsync<LeaderboardSeasonRankEntry>(
                $"""
                SELECT 
                    display_name as {nameof(LeaderboardSeasonRankEntry.DisplayName)},
                    membership_id as {nameof(LeaderboardSeasonRankEntry.MembershipId)},
                    progressions -> @SeasonRankProgressionHash -> 'level' as {nameof(LeaderboardSeasonRankEntry.SeasonRank)},
                    progressions -> @SeasonRankProgressionOverflowHash -> 'level' as {nameof(LeaderboardSeasonRankEntry.SeasonRankOverflow)}
                FROM destiny_user
                WHERE clan_id = ANY(@Clans)
                """,
                new
                {
                    SeasonRankProgressionHash = progressions.RewardProgression.Hash.ToString(),
                    SeasonRankProgressionOverflowHash = progressions.PrestigeProgression.Hash.ToString(),
                    Clans = clanIds!.ToArray()
                });

        var orderedResults = result
            .OrderByDescending(x => (x.SeasonRank + x.SeasonRankOverflow))
            .ToList();

        var embed = Embeds.Leaderboards.CreateRankingLeaderboard(
            amount,
            $"{seasonDefinition.DisplayProperties.Name} Rankings",
            "Rank: Level - Name",
            "No rankings found for this season :(",
            orderedResults,
            (entry) => entry.MembershipId,
            new Func<LeaderboardSeasonRankEntry, object>[]
            {
                (entry) => entry.SeasonRank + entry.SeasonRankOverflow,
                (entry) => entry.DisplayName
            },
            linkedUserId);

        await Context.Interaction.RespondAsync(embed: embed);
    }

    [SlashCommand("ranking", "Fetches and displays reputation leaderboard")]
    public async Task GetReputationLeaderboard(
        [Summary("type")]
        [Choice("Valor", "2083746873")]
        [Choice("Infamy", "3008065600")]
        [Choice("Trials", "2755675426")]
        [Choice("Iron Banner", "599071390")]
        [Choice("Competitive Division", "3696598664")]
        string progressionHash,
        [Summary("amount")] int amount = 10)
    {
        if (!_bungieClient.Repository.TryGetDestinyDefinition<DestinyProgressionDefinition>(
                uint.Parse(progressionHash),
                BungieLocales.EN,
                out var progressionDefinition))
        {
            await Context.Interaction.RespondAsync(
                embed: EmbedBuilders.Embeds.GetBasicEmbed(
                    "Error",
                    "Couldn't fetch progression definition"));
        }

        var clanIds = await _dbContextCaller.GetFromDbContext(
            async (IDbContext<GuildDbModel> db, CancellationToken ct) =>
            {
                return await db
                    .Set1
                    .Where(x => x.GuildId == Context.Guild.Id)
                    .Select(x => x.Clans)
                    .FirstOrDefaultAsync(ct);
            });

        var linkedUserId = await _dbContextCaller.GetFromDbContext<RegisteredUserDbModel, long?>(async (db, ct) =>
        {
            return await db
                .Set1
                .Where(x => x.UserId == Context.User.Id)
                .Select(x => x.MembershipId)
                .FirstOrDefaultAsync(ct);
        });

        var result = await _rawPostgresDbAccess
            .QueryAsync<ProgressionLeaderboardRankEntry>(
                $"""
                SELECT 
                    display_name as {nameof(ProgressionLeaderboardRankEntry.DisplayName)},
                    membership_id as {nameof(ProgressionLeaderboardRankEntry.MembershipId)},
                    progressions -> @ProgressionHash -> 'currentProgress' as {nameof(ProgressionLeaderboardRankEntry.Progress)},
                    COALESCE((progressions -> @ProgressionHash ->> 'currentResetCount')::int, 0) as {nameof(ProgressionLeaderboardRankEntry.CurrentResetCount)}
                FROM destiny_user
                WHERE clan_id = ANY(@Clans)
                """,
                new
                {
                    ProgressionHash = progressionDefinition.Hash.ToString(),
                    Clans = clanIds!.ToArray()
                });

        var progressionMaxValue = progressionDefinition.GetTotalProgressionValue();

        var orderedResults = result
            .OrderByDescending(x => x.Progress + x.CurrentResetCount * progressionMaxValue)
            .ToList();

        var embed = Embeds.Leaderboards.CreateRankingLeaderboard(
            amount,
            $"Top {amount} {Embeds.Leaderboards.ProgressionNameOverloads[progressionDefinition.Hash]} Rankings",
            "Rank: Progress (Resets) - Name",
            "No rankings found for this seasonal vendor :(",
            orderedResults,
            (entry) => entry.MembershipId,
            new Func<ProgressionLeaderboardRankEntry, object>[]
            {
                (entry) => entry.Progress + entry.CurrentResetCount * progressionMaxValue,
                (entry) => entry.CurrentResetCount,
                (entry) => entry.DisplayName
            },
            linkedUserId);

        await Context.Interaction.RespondAsync(embed: embed);
    }
}