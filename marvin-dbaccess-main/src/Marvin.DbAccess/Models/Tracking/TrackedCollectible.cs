using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.Tracking;

[MapDapperProperties]
public class TrackedCollectible
{
    [DapperColumn("hash")] public long Hash { get; set; }

    [DapperColumn("display_name")] public string DisplayName { get; set; }

    [DapperColumn("custom_description")] public string CustomDescription { get; set; }

    [DapperColumn("type")] public string Type { get; set; }
    [DapperColumn("is_broadcasting")] public bool IsBroadcasting { get; set; }
}