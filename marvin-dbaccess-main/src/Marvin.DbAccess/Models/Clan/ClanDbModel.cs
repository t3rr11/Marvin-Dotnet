using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.Clan;

/// <summary>
///     General clan data
/// </summary>
[MapDapperProperties]
public class ClanDbModel
{
    /// <summary>
    ///     Bungie.net GroupV2 clan Id
    /// </summary>
    [DapperColumn("clan_id")]
    public long ClanId { get; set; }

    /// <summary>
    ///     Clan display name
    /// </summary>
    [DapperColumn("clan_name")]
    public string ClanName { get; set; }

    /// <summary>
    ///     Clan callsign
    /// </summary>
    [DapperColumn("clan_callsign")]
    public string ClanCallsign { get; set; }

    /// <summary>
    ///     Clan progression level in the current season
    /// </summary>
    [DapperColumn("clan_level")]
    public int ClanLevel { get; set; }

    /// <summary>
    ///     How many members are there in clan overall
    /// </summary>
    [DapperColumn("member_count")]
    public int MemberCount { get; set; }

    /// <summary>
    ///     How many members are online currently
    /// </summary>
    [DapperColumn("members_online")]
    public int MembersOnline { get; set; }

    /// <summary>
    ///     Whether this clan will force scanned
    /// </summary>
    [DapperColumn("forced_scan")]
    public bool IsForcedScan { get; set; }

    /// <summary>
    ///     Is this clan being tracked by the bot
    /// </summary>
    [DapperColumn("is_tracking")]
    public bool IsTracking { get; set; }

    /// <summary>
    ///     When this clan was registered in bot
    /// </summary>
    [DapperColumn("joined_on")]
    public DateTime JoinedOn { get; set; }

    /// <summary>
    ///     When was the last scan made for this clan
    /// </summary>
    [DapperColumn("last_scan")]
    public DateTime LastScan { get; set; }

    /// <summary>
    ///     Whether this clan is a member on Patreon
    /// </summary>
    [DapperColumn("patreon")]
    public bool IsPatron { get; set; }
    
    /// <summary>
    ///     Banner data for this clan
    /// </summary>
    [DapperColumn("clan_banner_config")]
    public ClanBannerDataDbModel? BannerData { get; set; }
}