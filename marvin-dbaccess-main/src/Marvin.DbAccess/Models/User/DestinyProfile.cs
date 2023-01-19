using DotNetBungieAPI.Models.Destiny.Responses;
using DotNetBungieAPI.Models.GroupsV2;
using Marvin.DbAccess.Attributes;
using Marvin.DbAccess.Extensions;
using Marvin.Extensions;

namespace Marvin.DbAccess.Models.User;

[MapDapperProperties]
public class DestinyProfile
{
    public DestinyProfile()
    {
    }

    public DestinyProfile(
        GroupMember clanMembership,
        DestinyProfileResponse destinyProfileResponse)
    {
        if (destinyProfileResponse.Characters.Data.Count > 0)
        {
            var mostPlayedCharacter = destinyProfileResponse.Characters.GetMostPlayedCharacterSafe();
            if (destinyProfileResponse.CharacterActivities.Data.TryGetValue(
                    mostPlayedCharacter.CharacterId,
                    out var activitiesComponent))
            {
                CurrentActivity = activitiesComponent.CurrentActivity.Hash;
                DateActivityStarted = activitiesComponent.DateActivityStarted;
            }
            TimePlayed = destinyProfileResponse.Characters.Data.Values.Sum(x => x.MinutesPlayedTotal);
        }

        ClanId = clanMembership.GroupId;
        ClanJoinDate = clanMembership.JoinDate;
        LastPlayed = destinyProfileResponse.Profile.Data.DateLastPlayed;
        DisplayName = clanMembership.GetDisplayName();
        FirstScan = false;
        ForcedScan = false;
        LastUpdated = DateTime.UtcNow;
        MembershipId = clanMembership.DestinyUserInfo.MembershipId;
        Metrics = new Dictionary<uint, UserMetricData>();
        Progressions = new Dictionary<uint, UserProgressionData>();
        Records = new Dictionary<uint, UserRecordData>();
        Items = new List<uint>();
        RecentItems = new List<uint>();
        Private = !destinyProfileResponse.HasPublicRecords();
        ComputedData = new DestinyProfileComputedData();
    }

    /// <summary>
    ///     Clan that this profile is linked to
    /// </summary>
    [DapperColumn("clan_id")]
    public long? ClanId { get; set; }

    /// <summary>
    ///     Profile display name
    /// </summary>
    [DapperColumn("display_name")]
    public string DisplayName { get; set; }

    /// <summary>
    ///     Destiny membership Id
    /// </summary>
    [DapperColumn("membership_id")]
    public long MembershipId { get; set; }

    /// <summary>
    ///     How long this profile has been playing Destiny 2
    /// </summary>
    [DapperColumn("time_played")]
    public long TimePlayed { get; set; }

    /// <summary>
    ///     When did the user join current clan
    /// </summary>
    [DapperColumn("clan_join_date")]
    public DateTime ClanJoinDate { get; set; }

    /// <summary>
    ///     Last time user played
    /// </summary>
    [DapperColumn("last_played")]
    public DateTime LastPlayed { get; set; }

    /// <summary>
    ///     When was the last account update
    /// </summary>
    [DapperColumn("last_updated")]
    public DateTime LastUpdated { get; set; }

    /// <summary>
    ///     Whether this account is private
    /// </summary>
    [DapperColumn("private")]
    public bool Private { get; set; }

    /// <summary>
    ///     Whether it's the first scan
    /// </summary>
    [DapperColumn("first_scan")]
    public bool FirstScan { get; set; }

    /// <summary>
    ///     Current played activity
    /// </summary>
    [DapperColumn("current_activity")]
    public long? CurrentActivity { get; set; }

    /// <summary>
    ///     When started playing current activity
    /// </summary>
    [DapperColumn("date_activity_started")]
    public DateTime? DateActivityStarted { get; set; }

    /// <summary>
    ///     Whether should be force scanned
    /// </summary>
    [DapperColumn("forced_scan")]
    public bool ForcedScan { get; set; }

    /// <summary>
    ///     All tracked metrics related to this account
    /// </summary>
    [DapperColumn("metrics")]
    public Dictionary<uint, UserMetricData> Metrics { get; set; }

    /// <summary>
    ///     All tracked progressions related to this account
    /// </summary>
    [DapperColumn("progressions")]
    public Dictionary<uint, UserProgressionData> Progressions { get; set; }

    /// <summary>
    ///     All tracked records related to this account
    /// </summary>
    [DapperColumn("records")]
    public Dictionary<uint, UserRecordData> Records { get; set; }

    /// <summary>
    ///     All tracked items related to this account
    /// </summary>
    [DapperColumn("items")]
    public List<uint> Items { get; set; }

    /// <summary>
    ///     All tracked recent items related to this account
    /// </summary>
    [DapperColumn("recent_items")]
    public List<uint> RecentItems { get; set; }

    [DapperColumn("computed_data")] public DestinyProfileComputedData? ComputedData { get; set; }
}