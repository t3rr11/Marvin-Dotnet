using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.Guild;

[MapDapperProperties]
public class ClanDisplayProperties
{
    [DapperColumn("clan_id")] public long ClanId { get; set; }

    [DapperColumn("clan_name")] public string ClanName { get; set; }
}