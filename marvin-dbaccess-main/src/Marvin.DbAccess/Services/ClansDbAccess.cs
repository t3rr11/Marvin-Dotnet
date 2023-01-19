using DotNetBungieAPI.HashReferences;
using DotNetBungieAPI.Models.GroupsV2;
using Marvin.DbAccess.Models.Clan;
using Marvin.DbAccess.Models.Guild;
using Marvin.DbAccess.Options;
using Marvin.DbAccess.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marvin.DbAccess.Services;

public class ClansDbAccess : PostgresDbAccessBase, IClansDbAccess
{
    private const string GetClanQuery = @"SELECT * FROM clan WHERE clan_id = @GroupId";

    // GOTCHA: Online members will need to be passed in as a varaible after scanning all clan members to determine how many are online.
    private const string UpdateClanQuery = @"
INSERT INTO clan (clan_id, clan_name, clan_callsign, clan_level, member_count, members_online) 
VALUES (@GroupId, @Name, @ClanCallSign, @Level, @MemberCount, @MemberCount) 
ON CONFLICT(clan_id) 
DO UPDATE SET 
    clan_id = @GroupId,
    clan_name = @Name,
    clan_callsign = @ClanCallSign,
    clan_level = @Level,
    member_count = @MemberCount,
    members_online = @MembersOnline,
    last_scan = @Date
RETURNING *";

    private const string GetClansForFirstTimeScanningQuery = @"
SELECT * FROM clans_to_scan";

    private const string DeleteClanFromFirstScanListQuery = @"
DELETE FROM clans_to_scan
WHERE clan_id = @ClanId";

    private const string GetGeneralClanIdsForScanningQuery = @"
SELECT clan_id FROM clan 
WHERE patreon = false AND is_tracking = true
ORDER BY last_scan ASC";

    private const string GetPatreonClanIdsForScanningQuery = @"
SELECT clan_id FROM clan 
WHERE patreon = true AND is_tracking = true
ORDER BY last_scan ASC";

    private const string SetClanForcedFlagToFalseQuery = @"
UPDATE clan
SET forced_scan = false
WHERE clan_id = @ClanId";

    private const string GetAllLinkedDiscordGuildsQuery = @"
SELECT 
    guild.guild_id as Key,
    guild.broadcasts_config as Value
FROM guild
WHERE guild.clans @> @GroupId::jsonb
  AND guild.is_tracking = true
  AND (guild.broadcasts_config -> 'is_broadcasting')::boolean = true
  AND (guild.broadcasts_config -> 'channel_id') IS NOT NULL";

    public ClansDbAccess(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger<ClansDbAccess> logger) : base(databaseOptions, logger)
    {
    }

    public async ValueTask<IEnumerable<ClanDbModel>> GetClan(long groupId, CancellationToken cancellationToken)
    {
        return await QueryAsync<ClanDbModel>(
            GetClanQuery,
            new
            {
                GroupId = groupId
            },
            cancellationToken);
    }

    public async ValueTask<IEnumerable<ClanDbModel>> UpdateClan(
        GroupResponse groupResponse,
        int onlineUsers,
        CancellationToken cancellationToken)
    {
        return await QueryAsync<ClanDbModel>(UpdateClanQuery,
            new
            {
                groupResponse.Detail.GroupId,
                groupResponse.Detail.Name,
                groupResponse.Detail.ClanInfo.ClanCallSign,
                groupResponse.Detail.ClanInfo.D2ClanProgressions[DefinitionHashes.Progressions.ClanLevel].Level,
                groupResponse.Detail.MemberCount,
                MembersOnline = onlineUsers,
                Date = DateTime.UtcNow
            },
            cancellationToken);
    }

    public async ValueTask<IEnumerable<FirstTimeScanEntry>> GetClansForFirstTimeScanning(
        CancellationToken cancellationToken)
    {
        return await QueryAsync<FirstTimeScanEntry>(GetClansForFirstTimeScanningQuery, null, cancellationToken);
    }

    public async Task DeleteClanFromFirstScanList(
        long clanId,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            DeleteClanFromFirstScanListQuery,
            new
            {
                ClanId = clanId
            },
            cancellationToken);
    }

    public async ValueTask<IEnumerable<long>> GetGeneralClanIdsForScanning(
        CancellationToken cancellationToken)
    {
        return await QueryAsync<long>(GetGeneralClanIdsForScanningQuery, null, cancellationToken);
    }

    public async ValueTask<IEnumerable<long>> GetPatreonClanIdsForScanning(
        CancellationToken cancellationToken)
    {
        return await QueryAsync<long>(GetPatreonClanIdsForScanningQuery, null, cancellationToken);
    }

    public async Task SetClanForcedFlagToFalse(
        long clanId,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            SetClanForcedFlagToFalseQuery,
            new
            {
                ClanId = clanId
            },
            cancellationToken);
    }

    public async ValueTask<Dictionary<ulong, GuildBroadcastsConfig>> GetAllLinkedDiscordGuilds(
        long groupId,
        CancellationToken cancellationToken)
    {
        var result = await QueryAsync<KeyValuePair<ulong, GuildBroadcastsConfig>>(
            GetAllLinkedDiscordGuildsQuery,
            new
            {
                GroupId = groupId.ToString()
            },
            cancellationToken);

        return new Dictionary<ulong, GuildBroadcastsConfig>(result);
    }

    private const string GetClanNamesQuery = @"
SELECT 
    clan_id as Key,
    clan_name as Value
FROM clan
WHERE 
    clan_id = ANY(@Ids)";

    public async Task<Dictionary<long, string>> GetClanNamesAsync(
        List<long> clanIds,
        CancellationToken cancellationToken)
    {
        var queryResult = await QueryAsync<KeyValuePair<long, string>>(
            GetClanNamesQuery,
            new
            {
                Ids = clanIds.ToArray()
            },
            cancellationToken);
        return new Dictionary<long, string>(queryResult);
    }

    private const string GetClanUserNamesQuery = @"
SELECT
    membership_id as Key,
    display_name as Value
FROM 
    destiny_user
WHERE 
    clan_id = @ClanId";

    public async Task<Dictionary<long, string>> GetClanUserNamesAsync(
        long clanId,
        CancellationToken cancellationToken)
    {
        return new Dictionary<long, string>(await QueryAsync<KeyValuePair<long, string>>(
            GetClanUserNamesQuery,
            new
            {
                ClanId = clanId
            },
            cancellationToken));
    }

    private const string UpsertClanQuery = @"
INSERT INTO clan (clan_id, clan_name, clan_callsign, clan_level, member_count, members_online, forced_scan, is_tracking, joined_on, patreon, clan_banner_config) 
VALUES (@GroupId, @Name, @ClanCallSign, @Level, @MemberCount, @MemberCount, @ForcedScan, @IsTracking, @JoinedOn, @IsPatron, CAST(@ClanBannerConfig as jsonb)) 
ON CONFLICT(clan_id) 
DO UPDATE SET 
    clan_name = @Name,
    clan_callsign = @ClanCallSign,
    clan_level = @Level,
    member_count = @MemberCount,
    members_online = @MembersOnline,
    forced_scan = @ForcedScan,
    is_tracking = @IsTracking,
    joined_on = @JoinedOn,
    patreon = @IsPatron,
    last_scan = @Date,
    clan_banner_config = CAST(@ClanBannerConfig as jsonb)";
    
    public async Task UpsertClanAsync(
        ClanDbModel clanDbModel,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            UpsertClanQuery,
            new
            {
                GroupId = clanDbModel.ClanId,
                Name = clanDbModel.ClanName,
                ClanCallSign = clanDbModel.ClanCallsign,
                Level = clanDbModel.ClanLevel,
                MemberCount = clanDbModel.MemberCount,
                MembersOnline = clanDbModel.MembersOnline,
                ForcedScan = clanDbModel.IsForcedScan,
                IsTracking = clanDbModel.IsTracking,
                JoinedOn = clanDbModel.JoinedOn,
                IsPatron = clanDbModel.IsPatron,
                Date = clanDbModel.LastScan,
                ClanBannerConfig = clanDbModel.BannerData
            },
            cancellationToken);
    }

    private const string MarkClanAsDeletedQuery = @"
UPDATE clan
SET 
    clan_name = @Name,
    is_tracking = @IsTracking
WHERE
    clan_id = @ClanId";
    public async Task MarkClanAsDeletedAsync(
        ClanDbModel clanDbModel,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            MarkClanAsDeletedQuery,
            new
            {
                ClanId = clanDbModel.ClanId,
                Name = $"{clanDbModel.ClanName} - Deleted",
                IsTracking = false
            },
            cancellationToken);
    }
}