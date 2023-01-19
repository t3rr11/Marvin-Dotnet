using Marvin.DbAccess.Models.User;
using Marvin.DbAccess.Options;
using Marvin.DbAccess.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marvin.DbAccess.Services;

public class DestinyProfileDbAccess : PostgresDbAccessBase, IDestinyProfileDbAccess
{
    private const string GetDestinyProfileByMembershipIdQuery = @"
SELECT *
FROM destiny_user
WHERE membership_id = @MembershipId";

    private const string UpsertDestinyProfileDataQuery = @"
INSERT INTO destiny_user
(clan_id,
 display_name,
 membership_id,
 time_played,
 clan_join_date,
 last_played,
 last_updated,
 private,
 first_scan,
 current_activity,
 date_activity_started,
 forced_scan,
 metrics,
 records,
 progressions,
 items,
 recent_items,
 computed_data)
VALUES (@ClanId,
        @DisplayName,
        @MembershipId,
        @TimePlayed,
        @ClanJoinDate,
        @LastPlayed,
        @LastUpdated,
        @Private,
        @FirstScan,
        @CurrentActivity,
        @DateActivityStarted,
        @ForcedScan,
        CAST(@Metrics AS json),
        CAST(@Records AS json),
        CAST(@Progressions AS json),
        CAST(@Items AS json),
        CAST(@RecentItems AS json),
        CAST(@ComputedData as json))
ON CONFLICT(membership_id)
    DO UPDATE SET clan_id               = @ClanId,
                  display_name          = @DisplayName,
                  time_played           = @TimePlayed,
                  clan_join_date        = @ClanJoinDate,
                  last_played           = @LastPlayed,
                  last_updated          = @LastUpdated,
                  private               = @Private,
                  first_scan            = @FirstScan,
                  current_activity      = @CurrentActivity,
                  date_activity_started = @DateActivityStarted,
                  forced_scan           = @ForcedScan,
                  metrics               = CAST(@Metrics AS json),
                  records               = CAST(@Records AS json),
                  progressions          = CAST(@Progressions AS json),
                  items                 = CAST(@Items AS json),
                  recent_items          = CAST(@RecentItems AS json),
                  computed_data         = CAST(@ComputedData as json)";

    private const string GetCurrentClanMembersQuery = @"
SELECT clan_id,
       membership_id
FROM destiny_user
WHERE clan_id = @ClanId";

    private const string UpdateDestinyProfileClanIdQuery = @"
UPDATE destiny_user
SET clan_id = @ClanId
WHERE membership_id = @MembershipId";

    public DestinyProfileDbAccess(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger<DestinyProfileDbAccess> logger) : base(databaseOptions, logger)
    {
    }

    public async ValueTask<DestinyProfile?> GetDestinyProfileByMembershipId(
        long membershipId,
        CancellationToken cancellationToken)
    {
        var queryResult = await QueryAsync<DestinyProfile>(
            GetDestinyProfileByMembershipIdQuery,
            new
            {
                MembershipId = membershipId
            },
            cancellationToken);

        return queryResult.FirstOrDefault();
    }

    public async Task UpsertDestinyProfileData(
        DestinyProfile destinyProfile,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            UpsertDestinyProfileDataQuery,
            new
            {
                destinyProfile.ClanId,
                destinyProfile.DisplayName,
                destinyProfile.MembershipId,
                destinyProfile.TimePlayed,
                destinyProfile.ClanJoinDate,
                destinyProfile.LastPlayed,
                destinyProfile.LastUpdated,
                destinyProfile.Private,
                destinyProfile.FirstScan,
                destinyProfile.CurrentActivity,
                destinyProfile.DateActivityStarted,
                destinyProfile.ForcedScan,
                destinyProfile.Metrics,
                destinyProfile.Records,
                destinyProfile.Progressions,
                destinyProfile.RecentItems,
                destinyProfile.Items,
                destinyProfile.ComputedData
            },
            cancellationToken,
            logErrorParameters: false);
    }

    public async ValueTask<IEnumerable<DestinyProfileClanMemberReference>> GetClanMemberReferencesAsync(
        long clanId,
        CancellationToken cancellationToken)
    {
        return await QueryAsync<DestinyProfileClanMemberReference>(GetCurrentClanMembersQuery,
            new
            {
                ClanId = clanId
            },
            cancellationToken);
    }

    public async Task UpdateDestinyProfileClanId(
        long? clanId,
        long membershipId,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            UpdateDestinyProfileClanIdQuery,
            new
            {
                ClanId = clanId,
                MembershipId = membershipId
            },
            cancellationToken);
    }

    private const string SearchProfilesByNameQuery = @"
SELECT 
    membership_id, 
    display_name as name
FROM 
    destiny_user
WHERE display_name ~* @Input";

    public async Task<IEnumerable<ProfileSearchEntry>> SearchProfilesByNameAsync(
        string input,
        CancellationToken cancellationToken)
    {
        return await QueryAsync<ProfileSearchEntry>(
            SearchProfilesByNameQuery,
            new
            {
                Input = input
            },
            cancellationToken);
    }

    private const string RemoveDestinyUserFromDbQuery = @"
DELETE FROM destiny_user
WHERE membership_id = @MembershipId";

    public async Task RemoveDestinyUserFromDbAsync(
        long membershipId,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            RemoveDestinyUserFromDbQuery,
            new
            {
                MembershipId = membershipId
            },
            cancellationToken);
    }
}