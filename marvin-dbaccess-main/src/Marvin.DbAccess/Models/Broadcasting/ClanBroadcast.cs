using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.Broadcasting;

/// <summary>
///     Broadcast that is sent when clan property changes
/// </summary>
[MapDapperProperties]
public class ClanBroadcast : BroadcastBase
{
    /// <summary>
    ///     Old value, determine exact type by looking at broadcast type
    /// </summary>
    [DapperColumn("old_value")]
    public string OldValue { get; set; }

    /// <summary>
    ///     New value, determine exact type by looking at broadcast type
    /// </summary>
    [DapperColumn("new_value")]
    public string NewValue { get; set; }
}