using Discord.WebSocket;
using DotNetBungieAPI.Service.Abstractions;
using Marvin.Bot.Commands.EmbedBuilders;
using Marvin.Bot.Extensions;
using Marvin.Bot.Services.Interfaces;
using Marvin.DbAccess.EntityFramework.Models.Broadcasts;
using Marvin.DbAccess.EntityFramework.Models.ClanBroadcasts;
using Marvin.DbAccess.EntityFramework.Models.Clans;
using Marvin.DbAccess.EntityFramework.Models.UserBroadcasts;
using Marvin.DbAccess.EntityFramework.Services.Interfaces;
using Marvin.HostedServices.Extensions;
using static Marvin.Bot.Commands.EmbedBuilders.Embeds;

namespace Marvin.Bot.Services.Hosted;

public class BackgroundBroadcastService : PeriodicBackgroundService
{
    private readonly ILogger<BackgroundBroadcastService> _logger;
    private readonly ISystemsStatusService _systemsStatusService;
    private readonly DiscordShardedClient _discordShardedClient;
    private readonly IBungieClient _bungieClient;
    private readonly IDbContextCaller _dbContextCaller;

    public BackgroundBroadcastService(
        ILogger<BackgroundBroadcastService> logger,
        ISystemsStatusService systemsStatusService,
        DiscordShardedClient discordShardedClient,
        IBungieClient bungieClient,
        IDbContextCaller dbContextCaller) : base(logger)
    {
        _logger = logger;
        _systemsStatusService = systemsStatusService;
        _discordShardedClient = discordShardedClient;
        _bungieClient = bungieClient;
        _dbContextCaller = dbContextCaller;
    }

    protected override Task BeforeExecutionAsync(CancellationToken stoppingToken)
    {
        ChangeTimerSafe(TimeSpan.FromSeconds(120));
        return Task.CompletedTask;
    }

    protected override async Task OnTimerExecuted(CancellationToken cancellationToken)
    {
        if (!_systemsStatusService.DiscordBotIsReady)
        {
            await Task.Delay(1000, cancellationToken);
            return;
        }

        var userBroadcasts = await _dbContextCaller.GetAllUnannouncedUserBroadcasts(cancellationToken);

        foreach (var guildGroupedBroadcasts in userBroadcasts.GroupBy(x => x.GuildId))
        {
            var chunkedGuilds = guildGroupedBroadcasts.Chunk(24);
            foreach (var guildsChunk in chunkedGuilds)
            foreach (var typeGroupedBroadcasts in guildsChunk.GroupBy(x => x.Type))
            foreach (var hashGroupedBroadcasts in typeGroupedBroadcasts.GroupBy(x => x.DefinitionHash))
            {
                await SendGroupedUserBroadcasts(
                    guildGroupedBroadcasts.Key,
                    typeGroupedBroadcasts.Key,
                    (uint)hashGroupedBroadcasts.Key,
                    hashGroupedBroadcasts,
                    cancellationToken);
            }
        }

        var clanBroadcasts = await _dbContextCaller.GetAllUnannouncedClanBroadcasts(cancellationToken);

        foreach (var clanBroadcast in clanBroadcasts)
        {
            await SendClanBroadcast(clanBroadcast, cancellationToken);
        }
    }

    private async Task SendUserBroadcast(
        UserBroadcastDbModel destinyUserBroadcast,
        CancellationToken cancellationToken)
    {
        var userName = await _dbContextCaller.GetDestinyUserDisplayNameAsync(
            destinyUserBroadcast.MembershipId,
            cancellationToken);

        if (string.IsNullOrEmpty(userName))
        {
            await _dbContextCaller.MarkUserBroadcastSent(destinyUserBroadcast, cancellationToken);
            _logger.LogWarning("Failed to send broadcast due to username being null {@Broadcast}", destinyUserBroadcast);
            return;
        }

        var clanData = await _dbContextCaller.GetClanAsync(destinyUserBroadcast.ClanId, cancellationToken);
        if (clanData is null)
        {
            await _dbContextCaller.MarkUserBroadcastSent(destinyUserBroadcast, cancellationToken);
            _logger.LogWarning("Failed to send broadcast due to clan data not being found {@Broadcast}", destinyUserBroadcast);
            return;
        }

        var guild = _discordShardedClient.GetGuild(destinyUserBroadcast.GuildId);
        if (guild is null)
        {
            await _dbContextCaller.MarkUserBroadcastSent(destinyUserBroadcast, cancellationToken);
            _logger.LogWarning("Failed to send broadcast due to guild being null {@Broadcast}", destinyUserBroadcast);
            return;
        }

        var channelId = await _dbContextCaller.GetGuildBroadcastChannelAsync(
            destinyUserBroadcast.GuildId,
            cancellationToken);
        
        if (channelId is null)
        {
            await _dbContextCaller.MarkUserBroadcastSent(destinyUserBroadcast, cancellationToken);
            _logger.LogWarning("Failed to send broadcast due to channelId being null {@Broadcast}", destinyUserBroadcast);
            return;
        }

        var channel = guild.GetTextChannel(channelId.Value);

        if (channel is not null)
        {
            await channel.SendMessageAsync(
                embed: Embeds.Broadcast.BuildDestinyUserBroadcast(
                    destinyUserBroadcast,
                    clanData,
                    _bungieClient,
                    userName));

            await _dbContextCaller.MarkUserBroadcastSent(destinyUserBroadcast, cancellationToken);
        }
        else
        {
            await _dbContextCaller.MarkUserBroadcastSent(destinyUserBroadcast, cancellationToken);
            _logger.LogWarning("Failed to send broadcast due to channel being null {@Broadcast}", destinyUserBroadcast);
            return;
        }
    }

    private async Task SendGroupedUserBroadcasts(
        ulong guildId,
        DbAccess.EntityFramework.Models.Broadcasts.BroadcastType broadcastType,
        uint definitionHash,
        IGrouping<long, UserBroadcastDbModel> broadcasts,
        CancellationToken cancellationToken)
    {
        var amountOfBroadcasts = broadcasts.Count();
        if (amountOfBroadcasts == 1)
        {
            await SendUserBroadcast(broadcasts.First(), cancellationToken);
            return;
        }

        var userNamesKeyedByMembershipId = new Dictionary<long, string>(amountOfBroadcasts);
        foreach (var broadcast in broadcasts)
        {
            var userName = await _dbContextCaller.GetDestinyUserDisplayNameAsync(
                broadcast.MembershipId,
                cancellationToken);

            if (string.IsNullOrEmpty(userName))
                continue;

            userNamesKeyedByMembershipId.Add(broadcast.MembershipId, userName);
        }

        var clanIds = broadcasts
            .DistinctBy(x => x.ClanId)
            .Select(x => x.ClanId);

        var clansData = new List<ClanDbModel>();

        foreach (var clanId in clanIds)
        {
            var clanData = await _dbContextCaller.GetClanAsync(clanId, cancellationToken);
            if (clanData is null)
                return;

            clansData.Add(clanData);
        }

        var clansDictionary = clansData.ToDictionary(x => x.ClanId, x => x);

        var guild = _discordShardedClient.GetGuild(guildId);
        if (guild is null)
        {

            foreach (var broadcast in broadcasts)
            {
                await _dbContextCaller.MarkUserBroadcastSent(broadcast, cancellationToken);
                _logger.LogWarning("Failed to send broadcast due to guild being null {@Broadcast}", broadcast);
            }
            return;
        }

        var channelId = await _dbContextCaller.GetGuildBroadcastChannelAsync(guildId,
            cancellationToken);

        if (channelId is null)
        {

            foreach (var broadcast in broadcasts)
            {
                await _dbContextCaller.MarkUserBroadcastSent(broadcast, cancellationToken);
                _logger.LogWarning("Failed to send broadcast due to channelId being {@Broadcast}", broadcast);
            }
            return;
        }

        var channel = guild.GetTextChannel(channelId.Value);

        if (channel is not null)
        {
            var embed = Embeds.Broadcast.BuildDestinyUserGroupedBroadcast(
                broadcasts,
                broadcastType,
                definitionHash,
                clansDictionary,
                _bungieClient,
                userNamesKeyedByMembershipId);

            await channel.SendMessageAsync(embed: embed);

            foreach (var broadcast in broadcasts)
                await _dbContextCaller.MarkUserBroadcastSent(broadcast, cancellationToken);
        }
        else
        {
            foreach (var broadcast in broadcasts)
            {
                await _dbContextCaller.MarkUserBroadcastSent(broadcast, cancellationToken);
                _logger.LogWarning("Failed to send broadcast due to channel being null {@Broadcast}", broadcast);
            }
            return;
        }
    }

    private async Task SendClanBroadcast(
        ClanBroadcastDbModel clanBroadcast,
        CancellationToken cancellationToken)
    {
        var clanData = await _dbContextCaller.GetClanAsync(clanBroadcast.ClanId, cancellationToken);
        if (clanData is null)
            return;

        var guild = _discordShardedClient.GetGuild(clanBroadcast.GuildId);
        if (guild is null)
            return;

        var channelId = await _dbContextCaller.GetGuildBroadcastChannelAsync(
            clanBroadcast.GuildId,
            cancellationToken);
        
        if (channelId is null)
            return;

        var channel = guild.GetTextChannel(channelId.Value);

        if (channel is not null)
        {
            await channel.SendMessageAsync(
                embed: Embeds.Broadcast.BuildDestinyClanBroadcast(
                    clanBroadcast,
                    clanData));

            await _dbContextCaller.MarkClanBroadcastSent(clanBroadcast, cancellationToken);
        }
    }
}