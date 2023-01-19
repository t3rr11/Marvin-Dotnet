using Marvin.DbAccess.Models.Broadcasting;
using Marvin.DbAccess.Options;
using Marvin.DbAccess.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marvin.DbAccess.Services;

public class BroadcastsDbAccess : PostgresDbAccessBase, IBroadcastsDbAccess
{
    private const string SendClanBroadcastQuery = @"
INSERT INTO clan_broadcasts
(
    guild_id,
    clan_id,
    type,
    was_announced,
    date,
    old_value,
    new_value
)
VALUES 
(
    @GuildId,
    @ClanId,
    @Type,
    @WasAnnounced,
    @Date,
    @OldValue,
    @NewValue
)";

    private const string SendDestinyUserBroadcastQuery = @"
INSERT INTO user_broadcasts
(
    guild_id,
    clan_id,
    type,
    was_announced,
    date,
    membership_id,
    hash,
    additional_data
)
VALUES 
(
    @GuildId,
    @ClanId,
    @Type,
    @WasAnnounced,
    @Date,
    @MembershipId,
    @DefinitionHash,
    CAST(@AdditionalData as jsonb)
)";

    private readonly ILogger<BroadcastsDbAccess> _logger;

    public BroadcastsDbAccess(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger<BroadcastsDbAccess> logger) : base(databaseOptions, logger)
    {
        _logger = logger;
    }


    public async Task SendClanBroadcast(
        ClanBroadcast clanBroadcast,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending clan broadcast, data = {@Broadcast}", clanBroadcast);
        await ExecuteAsync(
            SendClanBroadcastQuery,
            new
            {
                GuildId = clanBroadcast.GuildId.ToString(),
                clanBroadcast.ClanId,
                clanBroadcast.Type,
                clanBroadcast.WasAnnounced,
                clanBroadcast.Date,
                clanBroadcast.OldValue,
                clanBroadcast.NewValue
            },
            cancellationToken);
    }

    public async Task SendDestinyUserBroadcast(
        DestinyUserBroadcast destinyUserBroadcast,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending user broadcast, data = {@Broadcast}", destinyUserBroadcast);
        await ExecuteAsync(
            SendDestinyUserBroadcastQuery,
            new
            {
                GuildId = destinyUserBroadcast.GuildId.ToString(),
                destinyUserBroadcast.ClanId,
                destinyUserBroadcast.Type,
                destinyUserBroadcast.WasAnnounced,
                destinyUserBroadcast.Date,
                destinyUserBroadcast.MembershipId,
                destinyUserBroadcast.DefinitionHash,
                destinyUserBroadcast.AdditionalData
            },
            cancellationToken);
    }

    private const string GetAllDestinyUserBroadcastsQuery = @"
SELECT 
    guild_id,
    clan_id,
    type,
    was_announced,
    date,
    membership_id,
    hash,
    additional_data 
FROM user_broadcasts
WHERE 
    was_announced = false";

    public async Task<IEnumerable<DestinyUserBroadcast>> GetAllDestinyUserBroadcastsAsync(
        CancellationToken cancellationToken)
    {
        return await QueryAsync<DestinyUserBroadcast>(GetAllDestinyUserBroadcastsQuery, null, cancellationToken);
    }

    private const string GetAllClanBroadcastsQuery = @"
SELECT 
    guild_id,
    clan_id,
    type,
    was_announced,
    date,
    old_value,
    new_value
FROM clan_broadcasts
WHERE 
    was_announced = false";
    public async Task<IEnumerable<ClanBroadcast>> GetAllClanBroadcastsAsync(
        CancellationToken cancellationToken)
    {
        return await QueryAsync<ClanBroadcast>(GetAllClanBroadcastsQuery, null, cancellationToken);
    }
}