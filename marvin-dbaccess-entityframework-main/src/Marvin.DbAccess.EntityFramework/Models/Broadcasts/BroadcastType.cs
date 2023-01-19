using DotNetBungieAPI.Models.Destiny.Definitions.Collectibles;
using DotNetBungieAPI.Models.Destiny.Definitions.Records;

namespace Marvin.DbAccess.EntityFramework.Models.Broadcasts;

/// <summary>
///     Types of things that are tracked
/// </summary>
public enum BroadcastType
{
    /// <summary>
    ///     This indicates that reported value is <see cref="DestinyRecordDefinition" /> hash, but it's a title
    /// </summary>
    Title = 0,

    /// <summary>
    ///     This indicates that reported value is <see cref="DestinyRecordDefinition" /> hash, but it's a title gilding
    /// </summary>
    GildedTitle = 1,

    /// <summary>
    ///     This indicates that reported value is <see cref="DestinyRecordDefinition" /> hash
    /// </summary>
    Triumph = 2,

    /// <summary>
    ///     This indicates that reported value is <see cref="DestinyCollectibleDefinition" /> hash
    /// </summary>
    Collectible = 3,

    /// <summary>
    ///     This indicates that reported value is clan level
    /// </summary>
    ClanLevel = 4,

    /// <summary>
    ///     This indicates that reported value is clan name
    /// </summary>
    ClanName = 5,

    /// <summary>
    ///     This indicates that reported value is clan call sign
    /// </summary>
    ClanCallSign = 6,

    /// <summary>
    ///     This indicates that reported value is <see cref="DestinyRecordDefinition" /> hash, but it's a objective completion
    /// </summary>
    RecordStepObjectiveCompleted = 7,

    /// <summary>
    ///     This indicates clan first scan was done
    /// </summary>
    ClanScanFinished = 8
}