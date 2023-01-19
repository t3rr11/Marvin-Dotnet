using System.Text.Json.Serialization;
using Marvin.DbAccess.Attributes;

namespace Marvin.DbAccess.Models.User;

[MapDapperProperties]
public class ProfileSearchEntry
{
    [DapperColumn("membership_id")]
    [JsonPropertyName("membershipId")]
    public string MembershipId { get; set; }

    [DapperColumn("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }
}